import { Message, UserData } from "@/types/Message";
import { cn } from "@/lib/utils";
import React, { useEffect, useRef } from "react";
import { Avatar, AvatarImage } from "../ui/avatar";
import ChatBottombar from "./chat-bottombar";
import { AnimatePresence, motion } from "framer-motion";
import { AgentInfo, ChannelInfo, ChatMsg, postApiChatRoomClientGetChannelChatHistory, postApiChatRoomClientSendTextMessageToChannel } from "@/chatroom-client";
import { AgentAvatar } from "../agent-avatar";
import ChatTopbar from "./chat-topbar";

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

  const onReloadMessages = async () => {
    postApiChatRoomClientGetChannelChatHistory({
      requestBody: {
        channelName: channel.name,
        count: 1000,
      },
    })
      .then((data) => {
        setMessages(data);
      })
      .catch((err) => {
        console.log(err);
      });
  }
  useEffect(() => {
    onReloadMessages();
    }, []);

  React.useEffect(() => {
    if (messagesContainerRef.current) {
      messagesContainerRef.current.scrollTop =
        messagesContainerRef.current.scrollHeight;
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
    )
      .then((data) => {
        setMessages([...messages, newMessage]);
      })
      .catch((err) => {
        console.log(err);
      });
  };

  return (
    <div className="w-full overflow-x-hidden overflow-y-auto h-full flex flex-col justify-end">
      <div className="static">
        <ChatTopbar channel={channel} onRefresh={onReloadMessages} />
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
                "flex flex-col gap-2 p-4 whitespace-pre-wrap",
                message.from !== selectedUser.name ? "items-end" : "items-start"
              )}
            >
              <div className="flex gap-3 items-center">
                {message.from === selectedUser.name && (
                  <AgentAvatar agent={{name: message.from}} />
                )}
                <span className=" bg-accent p-3 rounded-md max-w-xs">
                  {message.text}
                </span>
                {message.from !== selectedUser.name && (
                  <AgentAvatar agent={{name: message.from}} />
                )}
              </div>
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