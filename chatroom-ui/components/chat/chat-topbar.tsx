import React, { useEffect } from 'react'
import { Avatar, AvatarImage } from '../ui/avatar'
import { ArrowRight, Info, Pause, Phone, RotateCcw, StepForward, Trash, Video } from 'lucide-react';
import Link from 'next/link';
import { cn } from '@/lib/utils';
import { Button, buttonVariants } from '../ui/button';
import { AgentInfo, ChannelInfo, postApiChatRoomClientGetChannelMembers } from '@/chatroom-client';
import { AgentAvatar } from '../agent-avatar';
import { TooltipProvider } from '@radix-ui/react-tooltip';
import { IconTooltip, Tooltip, TooltipContent, TooltipTrigger } from '../ui/tooltip';
import { Badge } from '../ui/badge';
import { Popover, PopoverContent, PopoverTrigger } from '../ui/popover';
import { Select, SelectGroup, SelectLabel, SelectTrigger, SelectValue, SelectContent, SelectItem } from '../ui/select';
import { Input } from '../ui/input';
import { Channel } from '@/types/channel';

export interface OrchestrationSettings {
  orchestrator: string | undefined;
  maxReply: number;
}

export interface OrchestrationProps {
  onContinue?: () => void;
  onPause?: () => void;
}

export interface ChatTopbarProps extends OrchestrationProps {
  channel: Channel;
  remainingTurns: number;
  onRefresh?: () => void;
  onDeleteChatHistory?: () => void;
  onChannelChange?: (channel: Channel) => void;
}

export const TopbarIcons = [{ icon: Phone }, { icon: Video }, { icon: Info }];

export default function ChatTopbar({
  channel,
  remainingTurns,
  onRefresh,
  onDeleteChatHistory,
  onChannelChange,
  onContinue,
  onPause,
}: ChatTopbarProps) {
  const [members, setMembers] = React.useState<AgentInfo[]>(channel.members || []);
  const [currentChannel, setCurrentChannel] = React.useState<Channel>(channel);
  useEffect(() => {
    setMembers(channel.members || []);
    setCurrentChannel(channel);
  }
    , [channel]);

  return (
    <div className="w-full flex flex-wrap h-full p-4 pl-8 justify-start gap-10 items-center border-b">
      <div className="flex flex-col">
        <span className="font-medium text-nowrap">{channel.name}</span>
      </div>
      <div className="flex items-center gap-2">
        {/* // refresh button */}
        <Link
          href="#"
          onClick={onRefresh}
          className={
            cn(buttonVariants({ variant: "ghost", size: "icon" }), "h-9, w-9")
          }>
          <IconTooltip content="Refresh">
            <RotateCcw size={15} />
          </IconTooltip>
        </Link>

        {/* delete chat history icon */}
        <Link
          href="#"
          onClick={onDeleteChatHistory}
          className={
            cn(buttonVariants({ variant: "ghost", size: "icon" }), "h-9, w-9")
          }>
          <IconTooltip content="Delete Chat History">
            <Trash size={15} />
          </IconTooltip>
        </Link>
      </div>
      {/* orchestration */}
      <div className="flex items-center gap-2">
        <Popover>
          <PopoverTrigger asChild>
            <Button
              size={"sm"}
              variant={"outline"}
            >
              Orchestration
            </Button>
          </PopoverTrigger>
          <PopoverContent
            side="bottom"
            className="w-80">
            <div className="grid grid-cols-3 items-center gap-4 rounded-md">
              <div>
                <span>Orchestrator</span>
              </div>
              <div className='col-span-2'>
                <Select
                  value={currentChannel.orchestrator}
                  onValueChange={(value) => {
                    onChannelChange?.({
                      ...currentChannel,
                      orchestrator: value,
                    });
                  }}>
                  <SelectTrigger>
                    <SelectValue
                      placeholder="Select orchestrator" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectGroup>
                      {currentChannel.orchestrators?.map((orchestrator, index) => (
                        <SelectItem
                          key={index}
                          value={orchestrator}>
                          {orchestrator}
                        </SelectItem>
                      ))}
                    </SelectGroup>
                  </SelectContent>
                </Select>
              </div>
              <div>
                <span>Max AutoReply</span>
              </div>
              <div className='col-span-2'>
                <Input
                  type="number"
                  defaultValue={currentChannel.maxReply}
                  className="w-full"
                  onChange={(e) => {
                    var autoReply = parseInt(e.target.value);
                    if (autoReply < 0) {
                      alert("Max AutoReply must be equal or greater than 0");
                      autoReply = 0;
                    }
                    console.log("autoReply", autoReply);
                    onChannelChange?.({
                      ...currentChannel,
                      maxReply: autoReply,
                    });
                  }}
                />
              </div>
            </div>
          </PopoverContent>
        </Popover>
        {
          remainingTurns === -1 &&
          <Link
            href="#"
            className={
              cn(buttonVariants({ variant: "grey", size: "icon" }), "h-9, w-9")
            }>
            <IconTooltip content="Cancelling">
              <StepForward size={15} />
            </IconTooltip>
          </Link>
        }
        {
          remainingTurns === 0 &&
          <Link
            href="#"
            onClick={onContinue}
            className={
              cn(buttonVariants({ variant: "ghost", size: "icon" }), "h-9, w-9")
            }>
            <IconTooltip content="Continue">
              <StepForward size={15} />
            </IconTooltip>
          </Link>
        }
        {
          remainingTurns > 0 &&
          <Link
            href="#"
            onClick={onPause}
            className={
              cn(buttonVariants({ variant: "ghost", size: "icon" }), "h-9, w-9")
            }>
            <IconTooltip content="Pause">
              <Pause size={15} />
            </IconTooltip>
          </Link>
        }
      </div>

      <div className='flex gap-2 grow justify-end'>
        {members.map((agent, index) => (
          <Badge
            variant={"accent"}
            className='text-nowrap'
            key={index}>
            {agent.name}
          </Badge>
        ))}
      </div>
    </div>
  )
}