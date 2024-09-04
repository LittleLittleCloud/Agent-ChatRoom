import { cn } from "@/lib/utils";
import React, { useEffect, useRef } from "react";
import { Avatar, AvatarImage } from "../ui/avatar";
import ChatBottombar from "./chat-bottombar";
import { AnimatePresence, motion } from "framer-motion";
import { AgentInfo, ApiError, ChannelInfo, ChatMsg, OpenAPI, getApiChatRoomClientClearHistoryByChannelName, getApiChatRoomClientDeleteMessageByChannelNameByMessageId, postApiChatRoomClientEditTextMessage, postApiChatRoomClientGenerateNextReply, postApiChatRoomClientGetChannelChatHistory, postApiChatRoomClientSendTextMessageToChannel } from "@/chatroom-client";
import ChatTopbar, { OrchestrationSettings } from "./chat-topbar";
import { ChatMessage } from "./chat-message";
import { on } from "events";
import { GetTextContent } from "@/chatroom-client/types.extension";
import { useToast } from "@/components/ui/use-toast"
import { ToastAction } from "../ui/toast";
import { Channel } from "@/types/channel";

interface ChatListProps {
  selectedUser: AgentInfo;
  isMobile: boolean;
  channel: Channel;
  onChannelChange?: (channel: Channel) => void;
}

export function ChatList({
  selectedUser,
  isMobile,
  channel,
  onChannelChange,
}: ChatListProps) {
  const [messages, setMessages] = React.useState<ChatMsg[]>([]);
  const messagesContainerRef = useRef<HTMLDivElement>(null);
  const [remainingTurns, setRemainingTurns] = React.useState<number>(0); // -1 for cancalling, 0 for no more turns, > 0 for remaining turns
  const [eventSource, setEventSource] = React.useState<EventSource | undefined>(undefined);
  const [currentChannel, setCurrentChannel] = React.useState<Channel>(channel);
  const { toast } = useToast();

  useEffect(() => {
    setCurrentChannel(channel);
  }
    , [channel]);

  const onReloadMessages = async (currentMessage: ChatMsg[]) => {
    var data = await postApiChatRoomClientGetChannelChatHistory({
      requestBody: {
        channelName: channel.name,
        count: 1000,
      },
    });

    // if data != messages, then update messages
    var dataJson = JSON.stringify(data);
    var messagesJson = JSON.stringify(currentMessage);
    if (dataJson === messagesJson) {
      console.log("No new messages");
      return;
    }
    else {
      setMessages(data);
    }
  }

  const onDeleteMessages = async () => {
    if (confirm(`Are you sure you want to delete all messages in ${channel.name}?`) === false) {
      return;
    }
    if (channel.name === undefined || channel.name === null) {
      return;
    }

    await getApiChatRoomClientClearHistoryByChannelName({
      channelName: channel.name
    });

    await onReloadMessages(messages);
  }

  const onOrchestrationClickPause = async () => {
    setRemainingTurns(-1);
  };

  const onChooseNextSpeaker = async (agent: AgentInfo) => {
    console.log("Choosing next speaker", agent);
    if (channel.name === undefined || channel.name === null || channel.members === undefined || channel.members === null) {
      return;
    }

    if (remainingTurns !== 0) {
      toast(
        {
          title: "Orchestration in progress",
          description: "Please wait for the current orchestration to complete",
          variant: "default",
        }
      );
      return;
    }
    var availableCandidate = channel.members.find((member) => member.name === agent.name);

    if (availableCandidate?.name === undefined || availableCandidate?.name === null) {
      toast(
        {
          title: "Candidate not found",
          description: "The selected candidate is not a member of the channel",
          variant: "default",
        }
      );
      return;
    }
    setRemainingTurns(1);
    await postApiChatRoomClientGenerateNextReply({
      requestBody: {
        channelName: channel.name,
        chatMsgs: messages,
        candidates: [availableCandidate.name],
      },
    });
    setRemainingTurns(0);
  };

  const deleteMessageHandler = async (message: ChatMsg) => {
    if (confirm(`Are you sure you want to delete this message?`) === false) {
      return;
    }
    if (channel.name === undefined || channel.name === null || message.id === undefined || message.id === null) {
      return;
    }

    await getApiChatRoomClientDeleteMessageByChannelNameByMessageId({
      channelName: channel.name,
      messageId: message.id
    });

    await onReloadMessages(messages);
  }

  const deleteMessageAboveHandler = async (message: ChatMsg) => {
    if (confirm(`Are you sure you want to delete all messages above this message?`) === false) {
      return;
    }
    if (channel.name === undefined || channel.name === null || message.id === undefined || message.id === null) {
      return;
    }

    var index = messages.indexOf(message);
    var messagesToDelete = messages.slice(0, index);
    for (var i = 0; i < messagesToDelete.length; i++) {
      var message = messagesToDelete[i];
      if (message.id === undefined || message.id === null) {
        continue;
      }
      await getApiChatRoomClientDeleteMessageByChannelNameByMessageId({
        channelName: channel.name,
        messageId: message.id
      });
    }

    await onReloadMessages(messages);
  }

  const deleteMessageBelowHandler = async (message: ChatMsg) => {
    if (confirm(`Are you sure you want to delete all messages below this message?`) === false) {
      return;
    }
    if (channel.name === undefined || channel.name === null || message.id === undefined || message.id === null) {
      return;
    }

    var index = messages.indexOf(message);
    var messagesToDelete = messages.slice(index + 1);
    for (var i = 0; i < messagesToDelete.length; i++) {
      var message = messagesToDelete[i];
      if (message.id === undefined || message.id === null) {
        continue;
      }
      await getApiChatRoomClientDeleteMessageByChannelNameByMessageId({
        channelName: channel.name,
        messageId: message.id
      });
    }

    await onReloadMessages(messages);
  }

  const editMessageHandler = async (message: ChatMsg) => {
    if (channel.name === undefined || channel.name === null || message.id === undefined || message.id === null) {
      return;
    }

    await postApiChatRoomClientEditTextMessage(
      {
        requestBody: {
          channelName: channel.name,
          messageId: message.id,
          newText: GetTextContent(message),
        }
      });

    await onReloadMessages(messages);
  };

  const onOrchestrationClickNext = async (msgs: ChatMsg[]) => {
    if (channel.members === undefined || channel.members === null || channel.members.length === 0) {
      return
    }

    if (channel.orchestrator == undefined) {
      setRemainingTurns(0);
      toast(
        {
          title: "Orchestrator not selected",
          description: "Please select an orchestrator",
          variant: "default",
        }
      );
      return;
    }

    try {
      var response = await postApiChatRoomClientGenerateNextReply({
        requestBody: {
          channelName: channel.name,
          chatMsgs: msgs,
          candidates: channel.members.map((member) => member.name!),
          orchestrator: channel.orchestrator,
        },
      });

      if (response.message === undefined || response.message === null) {
        toast({
          title: "Orchestration complete",
          description: "No more turns left",
          variant: "default",
        });
        setRemainingTurns(0);
      }
      else {
        setRemainingTurns((prev) => {
          prev = prev - 1;
          if (prev <= 0) {
            toast({
              title: "Orchestration complete",
              description: "No more turns left",
              variant: "default",
            });
            return 0;
          }

          return prev;
        }
        );
      }
    }
    catch (error) {

      if (error instanceof ApiError) {
        console.log("Error", error.response);
        toast({
          title: "Orchestration error",
          description: error.response.body,
          variant: "default",
        });
      }

      setRemainingTurns(0);
    }
  }

  useEffect(() => {
    var es = new EventSource(`${OpenAPI.BASE}/api/ChatRoomClient/NewMessageSse/${channel.name}`);
    es.addEventListener("message", async (event) => {
      console.log("New message received");
      await onReloadMessages(messages);
    });

    es.onopen = (_) => {
      console.log("Connection opened");
    }

    es.onerror = (event) => {
      console.log("Error", event);
    }

    setEventSource(es);
    console.log("Event source set");
    onReloadMessages(messages);

    return () => {
      console.log("Closing event source");
      es.close();
    }
  }, [channel]);

  React.useEffect(() => {
    if (messagesContainerRef.current) {
      messagesContainerRef.current.scrollTop =
        messagesContainerRef.current.scrollHeight;
    }

    if (channel.maxReply > 0 && remainingTurns > 0) {
      console.log("message received, calling next");
      onOrchestrationClickNext(messages);
    }
  }, [messages]);

  const sendMessage = async (newMessage: ChatMsg) => {
    await postApiChatRoomClientSendTextMessageToChannel(
      {
        requestBody: {
          channelName: channel.name,
          message: newMessage,
        },
      }
    );
    setRemainingTurns(channel.maxReply);
  };

  return (
    <div className="w-full overflow-x-hidden overflow-y-auto h-full flex flex-col justify-end">
      <div className="static">
        <ChatTopbar
          onPause={onOrchestrationClickPause}
          remainingTurns={remainingTurns}
          channel={currentChannel}
          onContinue={async () => {
            var remainingTurn = currentChannel.maxReply > 0 ? currentChannel.maxReply : 1;
            console.log("remainingTurns", remainingTurn)
            setRemainingTurns(remainingTurn);
            await onOrchestrationClickNext(messages);
          }}
          onChooseNextSpeaker={onChooseNextSpeaker}
          onChannelChange={onChannelChange}
          onRefresh={() => onReloadMessages(messages)}
          onDeleteChatHistory={onDeleteMessages} />
      </div>
      <div
        ref={messagesContainerRef}
        className="w-full overflow-y-auto overflow-x-hidden h-full flex flex-col grow"
      >
        <AnimatePresence>
          {messages?.map((message, index) => (
            <motion.div
              key={index}
              layout
              initial={{ opacity: 0, scale: 1, y: 50, x: 0 }}
              animate={{ opacity: 1, scale: 1, y: 0, x: 0 }}
              exit={{ opacity: 0, scale: 1, y: 1, x: 0 }}
              transition={{
                opacity: { duration: 0.1 },
                layout: {
                  type: "spring",
                  bounce: 0.3,
                  duration: messages.indexOf(message) * 0.05 + 0.2,
                },
              }}
              style={{
                originX: 0.5,
                originY: 0.5,
              }}
              className={cn(
                "flex flex-col gap-2 p-4 whitespace-pre-wrap"
              )}
            >
              <ChatMessage
                key={index}
                message={message}
                selectedUser={selectedUser}
                onEdit={editMessageHandler}
                onDeletedMessageAbove={deleteMessageAboveHandler}
                onDeletedMessageBelow={deleteMessageBelowHandler}
                onDeleted={deleteMessageHandler} />
            </motion.div>
          ))}
        </AnimatePresence>
      </div>
      <div className="static">
        <ChatBottombar user={selectedUser} sendMessage={sendMessage} isMobile={isMobile} />
      </div>
    </div>
  );
}