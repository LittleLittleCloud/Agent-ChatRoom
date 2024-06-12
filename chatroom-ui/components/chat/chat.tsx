import { Message, UserData } from "@/types/Message";
import ChatTopbar from "@/components/chat/chat-topbar";
import { ChatList } from "@/components/chat/chat-list";
import React from "react";
import { AgentInfo, ChannelInfo, ChatMsg, postApiChatRoomClientSendTextMessageToChannel } from "@/chatroom-client";

interface ChatProps {
  selectedUser: AgentInfo;
  isMobile: boolean;
  channel: ChannelInfo;
}

export function Chat({ selectedUser, isMobile, channel }: ChatProps) {
  return (
    <div className="flex flex-col justify-between w-full h-full">
      <ChatList
        selectedUser={selectedUser}
        isMobile={isMobile}
        channel={channel}
      />
    </div>
  );
}