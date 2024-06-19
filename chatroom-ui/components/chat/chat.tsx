import { ChatList } from "@/components/chat/chat-list";
import React from "react";
import { AgentInfo, ChannelInfo, ChatMsg, postApiChatRoomClientSendTextMessageToChannel } from "@/chatroom-client";
import { Channel } from "@/types/channel";

interface ChatProps {
  selectedUser: AgentInfo;
  isMobile: boolean;
  channel: Channel;
  onChannelChange?: (channel: Channel) => void;
}

export function Chat({
  selectedUser,
  isMobile,
  channel,
  onChannelChange,
}: ChatProps) {
  return (
    <div className="flex flex-col justify-between w-full h-full">
      <ChatList
        selectedUser={selectedUser}
        isMobile={isMobile}
        channel={channel}
        onChannelChange={onChannelChange}
      />
    </div>
  );
}