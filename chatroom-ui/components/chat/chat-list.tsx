import { Message, UserData } from "@/types/Message";
import { cn } from "@/lib/utils";
import React, { useEffect, useRef } from "react";
import { Avatar, AvatarImage } from "../ui/avatar";
import ChatBottombar from "./chat-bottombar";
import { AnimatePresence, motion } from "framer-motion";
import { AgentInfo, ChannelInfo, ChatMsg, OpenAPI, getApiChatRoomClientClearHistoryByChannelName, getApiChatRoomClientDeleteMessageByChannelNameByMessageId, postApiChatRoomClientEditTextMessage, postApiChatRoomClientGenerateNextReply, postApiChatRoomClientGetChannelChatHistory, postApiChatRoomClientSendTextMessageToChannel } from "@/chatroom-client";
import ChatTopbar, { OrchestrationSettings } from "./chat-topbar";
import { ChatMessage } from "./chat-message";
import { on } from "events";
import { GetTextContent } from "@/chatroom-client/types.extension";
import { toast } from "sonner";

interface ChatListProps {
  selectedUser: AgentInfo;
  isMobile: boolean;
  channel: ChannelInfo;
}


export function ChatList({
  selectedUser,
  isMobile,
  channel,
}: ChatListProps) {
  const [messages, setMessages] = React.useState<ChatMsg[]>([]);
  const messagesContainerRef = useRef<HTMLDivElement>(null);
  const [orchstratorSettings, setOrchstratorSettings] = React.useState<OrchestrationSettings>({ orchestrator: undefined, maxReply: 10 });
  const [remainingTurns, setRemainingTurns] = React.useState<number>(0);
  const [eventSource, setEventSource] = React.useState<EventSource | undefined>(undefined);
  const onReloadMessages = async () => {
    console.log("Reloading messages");
    var data = await postApiChatRoomClientGetChannelChatHistory({
      requestBody: {
        channelName: channel.name,
        count: 1000,
      },
    });

    setMessages(data);
    console.log(data);
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

    await onReloadMessages();
  }

  const onOrchestrationClickPause = async () => {
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

    await onReloadMessages();
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

    await onReloadMessages();
  };

  const onOrchestrationClickNext = async (remainingTurn: number, msgs: ChatMsg[]) => {
    console.log("Orchestration next");
    if (channel.members === undefined || channel.members === null || channel.members.length === 0) {
      return
    }

    if (orchstratorSettings.orchestrator == undefined) {
      setRemainingTurns(0);
      toast("The orchestrator is not selected, please select an orchestrator first.");
      return;
    }
    setRemainingTurns(remainingTurn);

    var response = await postApiChatRoomClientGenerateNextReply({
      requestBody: {
        channelName: channel.name,
        chatMsgs: msgs,
        candidates: channel.members.map((member) => member.name!),
        orchestrator: orchstratorSettings.orchestrator,
      },
    });

    console.log(response);
    if (response.message === undefined || response.message === null || remainingTurn <= 0) {
      toast("No more received messages, it's your turn to reply now.");
      setRemainingTurns(0);
    }
    else
    {
      setRemainingTurns(remainingTurn - 1);
    }
  }

  useEffect(() => {
    var es = new EventSource(`${OpenAPI.BASE}/api/ChatRoomClient/NewMessageSse/${channel.name}`);
    es.addEventListener("message", async (event) => {
      const newMessage: ChatMsg = JSON.parse(event.data);
      await onReloadMessages();
    });

    es.onopen = (event) => {
      console.log("Connection opened");
    }

    es.onerror = (event) => {
      console.log("Error", event);
    }

    setEventSource(es);
    onReloadMessages();

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

    if (orchstratorSettings.maxReply > 0 && remainingTurns > 0) {
      console.log("message received, calling next");
      onOrchestrationClickNext(remainingTurns, messages);
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
    setRemainingTurns(orchstratorSettings.maxReply);
    await onReloadMessages();
  };

  return (
    <div className="w-full overflow-x-hidden overflow-y-auto h-full flex flex-col justify-end">
      <div className="static">
        <ChatTopbar
          onPause={onOrchestrationClickPause}
          remainingTurns={remainingTurns}
          channel={channel}
          orchestrationSettings={orchstratorSettings}
          onContinue={async () => {
            var remainingTurn = orchstratorSettings.maxReply > 0 ? orchstratorSettings.maxReply : 1;
            await onOrchestrationClickNext(remainingTurn, messages);
          }}
          onOrchestrationChange={setOrchstratorSettings}
          onRefresh={onReloadMessages}
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