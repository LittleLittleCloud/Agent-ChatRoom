"use client";

import { userData } from "@/types/Message";
import React, { useEffect, useState } from "react";
import {
  ResizableHandle,
  ResizablePanel,
  ResizablePanelGroup,
} from "@/components/ui/resizable";
import { cn } from "@/lib/utils";
import { Sidebar } from "@/components/sidebar";
import { Chat } from "./chat";
import { AgentInfo, ChannelInfo, getApiChatRoomClientGetChannels } from "@/chatroom-client";

interface ChatLayoutProps {
  defaultLayout: number[] | undefined;
  defaultCollapsed?: boolean;
  navCollapsedSize: number;
  selectedUser: AgentInfo;
  checkPoint: string | undefined;
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
  const [channels, setChannels] = useState<ChannelInfo[] | undefined>(undefined);
  const [selectedChannel, setSelectedChannel] = useState<ChannelInfo | undefined>(undefined);

  useEffect(() => {
    const checkScreenWidth = () => {
      setIsMobile(window.innerWidth <= 768);
    };

    // Initial check
    checkScreenWidth();

    // Event listener for screen width changes
    window.addEventListener("resize", checkScreenWidth);

    // fetch all channels from the server
    getApiChatRoomClientGetChannels()
    .then((res) => {
      setChannels(res);
    })
    .catch((err) => {
      console.log(err);
    });

    // Cleanup the event listener on component unmount
    return () => {
      window.removeEventListener("resize", checkScreenWidth);
    };
  }, []);

  useEffect(() => {
    reloadChannels();
  }, [checkPoint]);

  const reloadChannels = async () => {
    var _channels = await getApiChatRoomClientGetChannels();
    setChannels(_channels);
    var selectedChannelName = selectedChannel?.name;
    setSelectedChannel(_channels.find(channel => channel.name === selectedChannelName));
  }

  if (channels){
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
          maxSize={isMobile ? 8 : 30}
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
            selectedChannel && <Chat
            selectedUser={selectedUser}
            isMobile={isMobile}
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