"use client";
import Link from "next/link";
import { cn } from "@/lib/utils";
import { Button, buttonVariants } from "@/components/ui/button";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
  TooltipProvider
} from "@/components/ui/tooltip";
import { Avatar, AvatarFallback, AvatarImage } from "./ui/avatar";
import { ChannelInfo } from "@/chatroom-client";
import { Copy, Settings, Trash } from "lucide-react";

interface ChannelItemProps {
  channel: ChannelInfo & { avatar?: string; variant: "grey" | "ghost" };
  isCollapsed: boolean;
  isSelected: boolean;
  onClickChannel?: (channel: ChannelInfo) => void;
  onCloneChannel?: (channel: ChannelInfo) => void;
  onEditChannel?: (channel: ChannelInfo) => void;
  onDeleteChannel?: (channel: ChannelInfo) => void;
}

export function ChannelItem({ channel, isSelected, isCollapsed, onCloneChannel, onEditChannel, onDeleteChannel, onClickChannel }: ChannelItemProps) {
  if (isCollapsed) {
    return (
      <TooltipProvider>
        <Tooltip>
          <TooltipTrigger asChild>
            <Link
              href="#"
              className={cn(
                buttonVariants({ variant: channel.variant, size: "icon" }),
                "h-11 w-11 md:h-16 md:w-16",
                isSelected &&
                "dark:bg-muted dark:text-muted-foreground dark:hover:bg-muted dark:hover:text-white"
              )}
              onClick={(e) => {
                onClickChannel?.(channel);
                e.preventDefault();
              }}
            >
              <Avatar className="flex justify-center items-center">
                <AvatarImage
                  src={channel.avatar}
                  alt={channel.avatar}
                  width={6}
                  height={6}
                  className="w-10 h-10 " />
                {channel.name && <AvatarFallback className="w-10 h-10">{channel.name.charAt(0)}</AvatarFallback>}
              </Avatar>{" "}
              <span className="sr-only">{channel.name}</span>
            </Link>
          </TooltipTrigger>
          <TooltipContent
            side="right"
            className="flex items-center gap-4"
          >
            {channel.name}
          </TooltipContent>
        </Tooltip>
      </TooltipProvider>
    );
  } else {
    return (
      <div
        className={cn(
          buttonVariants({ variant: channel.variant, size: "xl" }),
          isSelected &&
          "flex w-full dark:bg-muted dark:text-white dark:hover:bg-muted dark:hover:text-white shrink",
          "justify-start gap-1"
        )}
        onClick={(e) => {
          onClickChannel?.(channel);
          e.preventDefault();
        }}
      >
        <div
          className="flex w-full items-center justify-between group/channel">
          <span
            className="w-full grow"
          >
            {channel.name}
          </span>
          <div
            className={cn(
              buttonVariants({ variant: "ghost", size: "icon" }),
              "invisible h-9 w-9 group-hover/channel:visible"
            )}
          >
            <TooltipProvider>
              <Tooltip>
                <TooltipTrigger asChild>
                  <Copy
                    size={15}
                    onClick={(e) => {
                      onCloneChannel?.(channel);
                      e.stopPropagation();
                    }} />
                </TooltipTrigger>
                <TooltipContent side="bottom" className="flex items-center gap-10">
                  Clone Channel
                </TooltipContent>
              </Tooltip>
            </TooltipProvider>
          </div>
          <div
            className={cn(
              buttonVariants({ variant: "ghost", size: "icon" }),
              "invisible h-9 w-9 group-hover/channel:visible"
            )}
          >
            <TooltipProvider>
              <Tooltip>
                <TooltipTrigger asChild>
                  <Settings
                    size={15}
                    onClick={(e) => {
                      onEditChannel?.(channel);
                      e.stopPropagation();
                    }} />
                </TooltipTrigger>
                <TooltipContent side="bottom" className="flex items-center gap-10">
                  Edit Channel
                </TooltipContent>
              </Tooltip>
            </TooltipProvider>
          </div>
          <div
            className={cn(
              buttonVariants({ variant: "ghost", size: "icon" }),
              "invisible h-9 w-9 group-hover/channel:visible"
            )}
          >
            <TooltipProvider>
              <Tooltip>
                <TooltipTrigger asChild>
                  <Trash
                    size={15}
                    onClick={(e) => {
                      onDeleteChannel?.(channel);
                      e.stopPropagation();
                    }} />
                </TooltipTrigger>
                <TooltipContent side="bottom" className="flex items-center gap-10">
                  Delete Channel
                </TooltipContent>
              </Tooltip>
            </TooltipProvider>
          </div>
        </div>
      </div>
    );
  }
}
