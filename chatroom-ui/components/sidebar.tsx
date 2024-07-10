"use client";

import Link from "next/link";
import { MoreHorizontal, SquarePen, SquarePlus } from "lucide-react";
import { cn } from "@/lib/utils";
import { buttonVariants } from "@/components/ui/button";
import { ChannelInfo, postApiChatRoomClientCloneChannel, postApiChatRoomClientCreateChannel, postApiChatRoomClientDeleteChannel } from "@/chatroom-client";
import { ChannelItem } from "@/components/channel-item";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@radix-ui/react-tooltip";
import { useState } from "react";
import { ChannelConfigModal } from "./modal/channel-modal";
import { Channel } from "@/types/channel";

interface SidebarProps {
  isCollapsed: boolean;
  channels: Channel[];
  onClick?: (channel: Channel) => void;
  onAddChannel?: (channel: Channel) => void;
  onCloneChannel?: (channel: Channel) => void;
  onEditChannel?: (channel: Channel) => void;
  onDeleteChannel?: (channel: Channel) => void;
  onSelectedChannel?: (channel: Channel) => void;
  isMobile: boolean;
}

export function Sidebar({channels, isCollapsed, isMobile, onEditChannel, onCloneChannel, onAddChannel, onDeleteChannel, onSelectedChannel }: SidebarProps) {
  const [channelConfigModalChannel, setChannelConfigModalChannel] = useState<Channel | undefined>(undefined);
  const [selectedChannel, setSelectedChannel] = useState<Channel | undefined>(undefined);

  const handleCloneChannelClick = async (channel: Channel) => {
    await postApiChatRoomClientCloneChannel({
      requestBody: {
        channelName: channel.name,
        newChannelName: `${channel.name}-clone`,
      }
    });

    console.log("Cloned channel", channel);

    onCloneChannel?.(channel);
  }
  const handleEditChannelClick = (channel: Channel | undefined) => {
    setChannelConfigModalChannel(channel);
  }

  const handleSelectedChannel = (channel: Channel) => {
    setSelectedChannel(channel);
    onSelectedChannel?.(channel);
  }

  const handleDeleteChannelClick = async (channel: Channel) => {
    if (confirm(`Are you sure you want to delete ${channel.name}?`) === false) {
      return;
    }

    await postApiChatRoomClientDeleteChannel({
      requestBody: {
        channelName: channel.name
      }
    });
    onDeleteChannel?.(channel);
  }

  return (
    <div
      data-collapsed={isCollapsed}
      className="relative group flex flex-col h-full p-2 data-[collapsed=true]:p-2 "
    >
      {channelConfigModalChannel && <ChannelConfigModal
        channel={channelConfigModalChannel}
        onSave={(channel) => {
          if (channelConfigModalChannel.name !== undefined) {
            // todo
            // check if name conflict with existing channels
            onEditChannel?.(channel);
          } else {
            onAddChannel?.(channel);
          }
        }}
        onClose={() => setChannelConfigModalChannel(undefined)} />}
      {!isCollapsed && (
        <div className="flex justify-between p-2 items-center">
          <div className="flex gap-2 items-center text-xl">
            <p className="font-medium">Channels</p>
            <span className="text-zinc-300">({channels.length})</span>
          </div>

          <div>
            <Link
              href="#"
              className={cn(
                buttonVariants({ variant: "ghost", size: "icon" }),
                "h-9 w-9"
              )}
            >
              <TooltipProvider>
                <Tooltip>
                  <TooltipTrigger asChild>
                    <SquarePlus size={20} onClick={() => {
                      setChannelConfigModalChannel({} as Channel);
                    }} />
                  </TooltipTrigger>
                  <TooltipContent side="bottom" className="flex items-center gap-10">
                    Add Channel
                  </TooltipContent>
                </Tooltip>
              </TooltipProvider>
            </Link>
          </div>
        </div>
      )}
      <nav className="grid gap-1 px-2 group-[[data-collapsed=true]]:justify-center group-[[data-collapsed=true]]:px-2">
        {channels.map((channel, index) =>
          <ChannelItem
            key={index}
            channel={channel}
            isSelected={selectedChannel?.name === channel.name}
            isCollapsed={isCollapsed}
            onCloneChannel={handleCloneChannelClick}
            onClickChannel={handleSelectedChannel}
            onEditChannel={handleEditChannelClick}
            onDeleteChannel={handleDeleteChannelClick} />
        )}
      </nav>
    </div>
  );
}