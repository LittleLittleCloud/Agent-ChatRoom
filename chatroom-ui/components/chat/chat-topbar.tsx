import React, { useEffect } from 'react'
import { Avatar, AvatarImage } from '../ui/avatar'
import { UserData } from '@/types/Message';
import { Info, Phone, Video } from 'lucide-react';
import Link from 'next/link';
import { cn } from '@/lib/utils';
import { buttonVariants } from '../ui/button';
import { AgentInfo, ChannelInfo, postApiChatRoomClientPostChannelMembers } from '@/chatroom-client';
import { AgentAvatar } from '../agent-avatar';

interface ChatTopbarProps {
    channel: ChannelInfo;
    }
    
export const TopbarIcons = [{ icon: Phone }, { icon: Video }, { icon: Info }];

export default function ChatTopbar({channel}: ChatTopbarProps) {
  const [members, setMembers] = React.useState<AgentInfo[]>([]);

  useEffect(() => {
    postApiChatRoomClientPostChannelMembers({
      requestBody: {
        channelName: channel.name
      }
    }).then((data) => {
      setMembers(data);
    });
  }, []);
  return (
    <div className="w-full h-20 flex p-4 justify-between items-center border-b">
        <div className="flex items-center gap-2">
          <div className="flex flex-col">
            <span className="font-medium">{channel.name}</span>
          </div>
        </div>

        <div>
          {members.map((agent, index) => (
            <AgentAvatar key={index} agent={agent} />
          ))}
        </div>
      </div>
  )
}