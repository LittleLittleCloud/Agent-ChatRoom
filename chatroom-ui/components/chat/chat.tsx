import { Message, UserData } from "@/types/Message";
import ChatTopbar from "@/components/chat/chat-topbar";
import { ChatList } from "@/components/chat/chat-list";
import React from "react";
import { ChannelInfo } from "@/chatroom-client";

interface ChatProps {
  messages?: Message[];
  selectedUser: UserData;
  isMobile: boolean;
  channel: ChannelInfo;
}

export function Chat({ messages, selectedUser, isMobile, channel }: ChatProps) {
  const [messagesState, setMessages] = React.useState<Message[]>(
    messages ?? []
  );

  const sendMessage = (newMessage: Message) => {
    setMessages([...messagesState, newMessage]);
  };

  return (
    <div className="flex flex-col justify-between w-full h-full">
      <ChatTopbar channel={channel} />
      <ChatList
        messages={messagesState}
        selectedUser={selectedUser}
        sendMessage={sendMessage}
        isMobile={isMobile}
      />
    </div>
  );
}