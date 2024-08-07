"use client";

import React, { useEffect, useState } from "react";
import {
  ResizableHandle,
  ResizablePanel,
  ResizablePanelGroup,
} from "@/components/ui/resizable";
import { cn } from "@/lib/utils";
import { Sidebar } from "@/components/sidebar";
import { Chat } from "./chat";
import { AgentInfo, ChannelInfo, getApiChatRoomClientGetChannels, getApiChatRoomClientGetOrchestrators } from "@/chatroom-client";
import { Channel } from "@/types/channel";

interface ChatLayoutProps {
  defaultLayout: number[] | undefined;
  defaultCollapsed?: boolean;
  navCollapsedSize: number;
  selectedUser: AgentInfo;
  checkPoint: string | "None";
}

export function ChatLayout({
  defaultLayout = [320, 480],
  defaultCollapsed = false,
  navCollapsedSize,
  selectedUser,
  checkPoint,
}: ChatLayoutProps) {
  const [isCollapsed, setIsCollapsed] = React.useState(defaultCollapsed);
  const [isMobile, setIsMobile] = useState(false);
  const [channels, setChannels] = useState<Channel[] | undefined>(undefined);
  const [selectedChannel, setSelectedChannel] = useState<Channel | undefined>(undefined);
  useEffect(() => {
    const checkScreenWidth = () => {
      setIsMobile(window.innerWidth <= 768);
    };

    // Initial check
    checkScreenWidth();

    // Event listener for screen width changes
    window.addEventListener("resize", checkScreenWidth);

    // fetch all channels from the server
    reloadChannels();

    // Cleanup the event listener on component unmount
    return () => {
      window.removeEventListener("resize", checkScreenWidth);
    };
  }, []);

  useEffect(() => {
    reloadChannels();
  }, [checkPoint]);

  const reloadChannels = async () => {
    var _channelInfos = await getApiChatRoomClientGetChannels();
    var existingChannels = channels ?? [];
    let _channels = [] as Channel[];

    for (let i = 0; i < _channelInfos.length; i++) {
      let index = existingChannels.findIndex((_channel) => _channel.name === _channelInfos[i].name);
      if (index !== -1) {
        _channels.push({
          ...existingChannels[index],
          ..._channelInfos[i],
        });
      }
      else {
        var orchestrators = _channelInfos[i].orchestrators ?? [undefined];
        _channels.push({
          ..._channelInfos[i],
          maxReply: 10,
          orchestrator: orchestrators[0],
        });
      }
    }

    setChannels(_channels);
    var selectedChannelName = selectedChannel?.name;
    setSelectedChannel(_channels.find(channel => channel.name === selectedChannelName));
  }

  if (channels) {
    return (
      <ResizablePanelGroup
        direction="horizontal"
        onLayout={(sizes: number[]) => {
          document.cookie = `react-resizable-panels:layout=${JSON.stringify(
            sizes
          )}`;
        }}
        className="h-full items-stretch"
      >
        <ResizablePanel
          defaultSize={defaultLayout[0]}
          collapsedSize={navCollapsedSize}
          collapsible={true}
          minSize={isMobile ? 0 : 24}
          maxSize={isMobile ? 8 : 35}
          onCollapse={() => {
            setIsCollapsed(true);
            document.cookie = `react-resizable-panels:collapsed=${JSON.stringify(
              true
            )}`;
          }}
          onExpand={() => {
            setIsCollapsed(false);
            document.cookie = `react-resizable-panels:collapsed=${JSON.stringify(
              false
            )}`;
          }}
          className={cn(
            isCollapsed && "min-w-[50px] md:min-w-[70px] transition-all duration-300 ease-in-out h-full"
          )}
        >
          <Sidebar
            isCollapsed={isCollapsed || isMobile}
            channels={channels.map((channel) => ({
              ...channel,
              variant: "grey",
            }))}
            isMobile={isMobile}
            onAddChannel={async (channel) => {
              console.log(channel);
              await reloadChannels();
            }}
            onEditChannel={async (channel) => {
              console.log('edit channel')
              console.log(channel);
              await reloadChannels();
            }}
            onCloneChannel={
              async (channel) => {
                console.log(channel);
                await reloadChannels();
              }}
            onDeleteChannel={async (channel) => {
              console.log(channel);
              await reloadChannels();
            }}
            onSelectedChannel={setSelectedChannel}
          />
        </ResizablePanel>
        <ResizableHandle withHandle />
        <ResizablePanel defaultSize={defaultLayout[1]} minSize={30}>
          {
            selectedChannel &&
            <Chat
              selectedUser={selectedUser}
              isMobile={isMobile}
              onChannelChange={
                async (channel) => {
                  setChannels((pre) => {
                    return pre?.map((_channel) => {
                      if (_channel.name === channel.name) {
                        return channel;
                      }
                      return _channel;
                    });
                  })
                  setSelectedChannel(channel);
                }}
              channel={selectedChannel} />
          }
          {
            !selectedChannel && <div className="flex flex-col justify-center items-center h-full">
              <p className="text-xl font-bold">Select a channel</p>
            </div>
          }
        </ResizablePanel>
      </ResizablePanelGroup>
    );
  }
  else {
    return (
      <div className="flex flex-col justify-center items-center h-full">
        <p className="text-xl font-bold">Loading...</p>
      </div>
    );
  }
}