import React, { useEffect } from 'react'
import { Avatar, AvatarImage } from '../ui/avatar'
import { UserData } from '@/types/Message';
import { ArrowRight, Info, Phone, RotateCcw, StepForward, Trash, Video } from 'lucide-react';
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

export interface OrchestrationSettings {
  orchestrator: "llm" | "manual" | string;
  maxReply: number;
}

export interface OrchestrationProps {
  orchestrationSettings: OrchestrationSettings;
  onOrchestrationChange?: (settings: OrchestrationSettings) => void;
  onContinue?: () => void;
}

export interface ChatTopbarProps extends OrchestrationProps {
  channel: ChannelInfo;
  onRefresh?: () => void;
  onDeleteChatHistory?: () => void;
}

export const TopbarIcons = [{ icon: Phone }, { icon: Video }, { icon: Info }];

export default function ChatTopbar({
  channel,
  onRefresh,
  onDeleteChatHistory,
  orchestrationSettings,
  onOrchestrationChange,
  onContinue,
}: ChatTopbarProps) {
  const [members, setMembers] = React.useState<AgentInfo[]>(channel.members || []);

  useEffect(() => {
    setMembers(channel.members || []);
  }
  , [channel]);

  return (
    <div className="w-full h-20 flex p-4 pl-8 justify-start gap-10 items-center border-b">
      <div className="flex flex-col">
        <span className="font-medium">{channel.name}</span>
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
                  value={orchestrationSettings.orchestrator}
                  onValueChange={(value) => {
                    onOrchestrationChange?.({
                      ...orchestrationSettings,
                      orchestrator: value,
                    });
                  }}>
                  <SelectTrigger>
                    <SelectValue
                      placeholder="Select orchestrator" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectGroup>
                      <SelectLabel
                        className='text-primary/50'
                      >Built-in orchestrator</SelectLabel>
                      <SelectItem value="llm">LLM</SelectItem>
                      <SelectItem value="manual">Manual</SelectItem>
                      <SelectLabel
                        className='text-primary/50'>Custom orchestrator</SelectLabel>
                      <SelectItem value="custom">Custom</SelectItem>
                    </SelectGroup>
                  </SelectContent>
                </Select>
              </div>
              <div>
                <span>Max Reply</span>
              </div>
              <div className='col-span-2'>
                <Input
                  type="number"
                  defaultValue={orchestrationSettings.maxReply}
                  className="w-full"
                  onChange={(e) => {
                    onOrchestrationChange?.({
                      ...orchestrationSettings,
                      maxReply: parseInt(e.target.value),
                    });
                  }}
                />
              </div>
            </div>
          </PopoverContent>
        </Popover>

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
      </div>

      <div className='flex gap-2 w-full justify-end'>
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