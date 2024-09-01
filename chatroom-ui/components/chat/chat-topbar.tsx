import React, { useEffect } from 'react'
import { Avatar, AvatarImage } from '../ui/avatar'
import { ArrowRight, Info, Pause, Phone, RotateCcw, StepForward, Trash, Video } from 'lucide-react';
import Link from 'next/link';
import { cn } from '@/lib/utils';
import { Button, buttonVariants } from '../ui/button';
import { AgentInfo, ChannelInfo, postApiChatRoomClientGetChannelMembers } from '@/chatroom-client';
import { AgentAvatar } from '../agent-avatar';
import { IconTooltip, Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from '../ui/tooltip';
import { Badge } from '../ui/badge';
import { Popover, PopoverContent, PopoverTrigger } from '../ui/popover';
import { Select, SelectGroup, SelectLabel, SelectTrigger, SelectValue, SelectContent, SelectItem } from '../ui/select';
import { Input } from '../ui/input';
import { Channel } from '@/types/channel';

export interface OrchestrationSettings {
  orchestrator: string | undefined;
  maxReply: number;
}

export interface ChatTopbarProps {
  channel: Channel;
  remainingTurns: number;
  onChooseNextSpeaker?: (agent: AgentInfo) => void;
  onRefresh?: () => void;
  onDeleteChatHistory?: () => void;
  onChannelChange?: (channel: Channel) => void;
  onContinue?: () => void;
  onPause?: () => void;
}

export const TopbarIcons = [{ icon: Phone }, { icon: Video }, { icon: Info }];

export default function ChatTopbar({
  channel,
  remainingTurns,
  onRefresh,
  onChooseNextSpeaker,
  onDeleteChatHistory,
  onChannelChange,
  onContinue,
  onPause,
}: ChatTopbarProps) {
  const [members, setMembers] = React.useState<AgentInfo[]>(channel.members || []);
  const [currentChannel, setCurrentChannel] = React.useState<Channel>(channel);
  const iconSize = 14;
  useEffect(() => {
    setMembers(channel.members || []);
    setCurrentChannel(channel);
  }
    , [channel]);

  return (
    <div className="w-full flex h-full p-3 pl-8 justify-start gap-3 items-center border-b">
      <div className="flex flex-col">
        <span className="font-medium text-nowrap">{channel.name}</span>
      </div>
      <div className="flex items-center gap-2">
        {/* // refresh button */}
        <Link
          href="#"
          onClick={onRefresh}
          className={
            cn(buttonVariants({ variant: "outline", size: "tiny" }))
          }>
          <IconTooltip content="Refresh">
            <RotateCcw size={iconSize} />
          </IconTooltip>
        </Link>

        {/* delete chat history icon */}
        <Link
          href="#"
          onClick={onDeleteChatHistory}
          className={
            cn(buttonVariants({ variant: "outline", size: "tiny" }))
          }>
          <IconTooltip content="Delete Chat History">
            <Trash size={iconSize} />
          </IconTooltip>
        </Link>
      </div>
      {/* orchestration */}
      <div className="flex items-center gap-2 grow">
        <Popover>
          <PopoverTrigger asChild>
            <Button
              size={"tiny"}
              variant={"outline"}
            >
              Orchestration
            </Button>
          </PopoverTrigger>
          <PopoverContent
            side="bottom"
            className="w-80">
            <div className="grid grid-cols-3 items-center gap-4 rounded-md text-sm p-2">
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
              cn(buttonVariants({ variant: "grey", size: "tiny" }))
            }>
            <IconTooltip content="Cancelling">
              <StepForward size={iconSize} />
            </IconTooltip>
          </Link>
        }
        {
          remainingTurns === 0 &&
          <Link
            href="#"
            onClick={onContinue}
            className={
              cn(buttonVariants({ variant: "outline", size: "tiny" }))
            }>
            <IconTooltip content="Continue">
              <StepForward size={iconSize} />
            </IconTooltip>
          </Link>
        }
        {
          remainingTurns > 0 &&
          <Link
            href="#"
            onClick={onPause}
            className={
              cn(buttonVariants({ variant: "ghost", size: "tiny" }))
            }>
            <IconTooltip content="Pause">
              <Pause size={iconSize} />
            </IconTooltip>
          </Link>
        }
      </div>

      <div
        className='flex gap-2 flex-wrap justify-end'>
        {members.map((agent, index) => (
          <TooltipProvider key={index}>
            <Tooltip>
              <TooltipTrigger>
                <Badge
                  variant={"accent"}
                  className={
                    cn(
                      "text-nowrap",
                      // if remaining turn is 0, user can click on the agent to proceed the next turn
                      remainingTurns === 0 && "cursor-pointer",
                      remainingTurns !== 0 && "bg-accent/20 cursor-not-allowed text-accent/80"
                    )
                  }
                  onClick={() => onChooseNextSpeaker?.(agent)}
                  key={index}>
                  {agent.name}
                </Badge>
              </TooltipTrigger>
              <TooltipContent side="bottom" className="flex items-center gap-2">
                {remainingTurns === 0 && <span>Choose {agent.name} as next speaker</span>}
                {remainingTurns !== 0 && <span>Conversation is undergoing, please wait the current turn to finish</span>}
              </TooltipContent>
            </Tooltip>
          </TooltipProvider>
        ))}
      </div>
    </div>
  )
}