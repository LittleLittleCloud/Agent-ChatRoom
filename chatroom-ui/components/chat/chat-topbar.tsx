import React, { useEffect } from 'react'
import { Avatar, AvatarImage } from '../ui/avatar'
import { UserData } from '@/types/Message';
import { Info, Phone, RotateCcw, Trash, Video } from 'lucide-react';
import Link from 'next/link';
import { cn } from '@/lib/utils';
import { buttonVariants } from '../ui/button';
import { AgentInfo, ChannelInfo, postApiChatRoomClientGetChannelMembers } from '@/chatroom-client';
import { AgentAvatar } from '../agent-avatar';
import { TooltipProvider } from '@radix-ui/react-tooltip';
import { Tooltip, TooltipContent, TooltipTrigger } from '../ui/tooltip';

interface ChatTopbarProps {
  channel: ChannelInfo;
  onRefresh?: () => void;
  onDeleteChatHistory?: () => void;
}

export const TopbarIcons = [{ icon: Phone }, { icon: Video }, { icon: Info }];

export default function ChatTopbar({
    channel,
    onRefresh,
    onDeleteChatHistory,
  }: ChatTopbarProps) {
  const [members, setMembers] = React.useState<AgentInfo[]>(channel.members || []);
  
  useEffect(() => {
    setMembers(channel.members || []);
  }
  , [channel]);

  return (
    <div className="w-full h-20 flex p-4 justify-between items-center border-b">
      <div className="flex items-center gap-5">
        <div className="flex flex-col">
          <span className="font-medium">{channel.name}</span>
        </div>
        {/* // refresh button */}
        <Link
          href="#"
          onClick={onRefresh}
          className={
            cn(buttonVariants({ variant: "ghost", size: "icon" }), "h-9, w-9")
          }>
          <TooltipProvider>
            <Tooltip>
              <TooltipTrigger asChild>
                <RotateCcw size={15} />
              </TooltipTrigger>
              <TooltipContent side="bottom" className="flex items-center gap-4">
                Refresh
              </TooltipContent>
            </Tooltip>
          </TooltipProvider>
        </Link>

        {/* delete chat history icon */}
        <Link
          href="#"
          onClick={onDeleteChatHistory}
          className={
            cn(buttonVariants({ variant: "ghost", size: "icon" }), "h-9, w-9")
          }>
          <TooltipProvider>
            <Tooltip>
              <TooltipTrigger asChild>
                <Trash size={15} />
              </TooltipTrigger>
              <TooltipContent side="bottom" className="flex items-center gap-4">
                Delete Chat History
              </TooltipContent>
            </Tooltip>
          </TooltipProvider>
        </Link>
      </div>

      <div className='flex gap-2'>
        {members.map((agent, index) => (
          <AgentAvatar key={index} agent={agent} />
        ))}
      </div>
    </div>
  )
}