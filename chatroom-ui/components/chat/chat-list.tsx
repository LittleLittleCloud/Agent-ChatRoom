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
  const [eventSource, setEventSource] = React.useState<EventSource | null>(null);
  const [orchstratorSettings, setOrchstratorSettings] = React.useState<OrchestrationSettings>({ orchestrator: "llm", maxReply: 10 });
  const [remainingTurns, setRemainingTurns] = React.useState<number>(0);
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

  const onOrchestrationClickNext = async (remainingTurn: number) => {
    if (remainingTurn <= 0) {
      return;
    }
    console.log("Orchestration next");
    if (channel.members === undefined || channel.members === null || channel.members.length === 0) {
      return
    }
    var es = new EventSource(`${OpenAPI.BASE}/api/ChatRoomClient/NewMessageSse/${channel.name}`);
    es.addEventListener("message", async (event) => {
      const newMessage: ChatMsg = JSON.parse(event.data);
      console.log(newMessage);
      await onReloadMessages();
    });

    es.onopen = (event) => {
      console.log("Connection opened");
    }

    es.onerror = (event) => {
      console.log("Error", event);
    }
    setEventSource(es);

    var response = await postApiChatRoomClientGenerateNextReply({
      requestBody: {
        channelName: channel.name,
        chatMsgs: messages,
        candidates: channel.members.map((member) => member.name!),
      },
    });

    console.log(response);
    if (response.message === undefined || response.message === null) {
      console.log("response is null");
      setRemainingTurns(0);
    }
    else {
      console.log("remaining turn", remainingTurn - 1);
      setRemainingTurns(remainingTurn - 1);
    }

    eventSource?.close();
  }

  useEffect(() => {
    onReloadMessages();
  }, [channel]);

  React.useEffect(() => {
    if (messagesContainerRef.current) {
      messagesContainerRef.current.scrollTop =
        messagesContainerRef.current.scrollHeight;
    }

    if (remainingTurns > 0) {
      onOrchestrationClickNext(remainingTurns);
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

    await onReloadMessages();

    // if  orchestrator is llm, then generate next reply
    if (orchstratorSettings.orchestrator === "llm") {
      setRemainingTurns(orchstratorSettings.maxReply);
    };
  };

  return (
    <div className="w-full overflow-x-hidden overflow-y-auto h-full flex flex-col justify-end">
      <div className="static">
        <ChatTopbar
          channel={channel}
          orchestrationSettings={orchstratorSettings}
          onContinue={async () => await onOrchestrationClickNext(orchstratorSettings.orchestrator === "llm" ? orchstratorSettings.maxReply : 1)}
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
                onDeleted={deleteMessageHandler}/>
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