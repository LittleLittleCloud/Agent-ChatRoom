import React from "react";
import { AgentAvatar } from "../agent-avatar";
import { cn } from "@/lib/utils";
import { Markdown } from "../markdown";
import { Badge } from "../ui/badge";
import { AgentInfo, ChatMsg, getApiChatRoomClientDeleteMessageByChannelNameByMessageId } from "@/chatroom-client";
import { ToggleGroup, ToggleGroupItem } from "../ui/toggle-group";
import { Columns, Columns2, Copy, Edit, EllipsisVertical, RotateCcw, Trash, Type } from "lucide-react";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@radix-ui/react-tooltip";
import { Button } from "../ui/button";
import { CopyToClipboardIcon } from "../copy-to-clipboard-icon";
import { channel } from "diagnostics_channel";
import { on } from "events";
import EmojiPicker from "@emoji-mart/react";
import { GetTextContent } from "@/chatroom-client/types.extension";
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuLabel, DropdownMenuSeparator, DropdownMenuTrigger } from "../ui/dropdown-menu";
import { Label } from "../ui/label";

export interface ChatMessageProps {
  message: ChatMsg;
  selectedUser: AgentInfo;
  onDeleted?: (msg: ChatMsg) => void;
  onResend?: (msg: ChatMsg) => void;
  onDeletedMessageAbove?: (msg: ChatMsg) => void;
  onDeletedMessageBelow?: (msg: ChatMsg) => void;
  onEdit?: (msg: ChatMsg) => void;
}

export function ChatMessage({
  message,
  selectedUser,
  onDeleted,
  onDeletedMessageAbove,
  onDeletedMessageBelow,
  onResend,
  onEdit, }: ChatMessageProps) {
  const isFromSelectedUser = message.from === selectedUser.name;
  const [markdown, setMarkdown] = React.useState<string>('');
  const [showResend, setShowResend] = React.useState<boolean>(message.from == selectedUser.name);
  const [showMarkdown, setShowMarkdown] = React.useState<boolean>(true);
  const [isEditing, setIsEditing] = React.useState<boolean>(false);
  const [editingText, setEditingText] = React.useState<string>('');
  React.useEffect(() => {
    var textContent = GetTextContent(message);

    if (textContent === undefined) {
      return;
    }
    else {
      setMarkdown(textContent);
    }
  }, [message]);

  const handleDelete = async (msg: ChatMsg) => {
    onDeleted?.(msg);
  }

  const handleDeleteMessageAbove = async (msg: ChatMsg) => {
    onDeletedMessageAbove?.(msg);
  }

  const handleDeleteMessageBelow = async (msg: ChatMsg) => {
    onDeletedMessageBelow?.(msg);
  }

  const handleEditing = async (msg: ChatMsg) => {
    setIsEditing(false);
    onEdit?.(msg);
  }

  return (
    <div className={cn("flex flex-col w-full items-start")}>
      <div className={cn("flex flex-col w-full m-2 border-solid border-2 border-accent/50 rounded-md", isFromSelectedUser ? "border-accent" : "")}>
        <div
          className={cn("flex items-center p-1 bg-accent/50 group/settings", isFromSelectedUser ? "bg-accent" : "")}>
          <span
            className="text-nowrap bg-transparent hover:bg-transparent cursor-default pl-1"
            >{message.from}</span>
          <div className="invisible flex flex-grow items-center justify-end gap-1 group-hover/settings:visible">
          {/* switch between preview and text */}
          {/* | preview | text | */}
          {
            <div
              className="flex rounded-md"
            >
              <Button
                variant={"disabled"}
                className={cn("bg-transparent",
                  showMarkdown ? "bg-primary/10" : "")}
                size={"tiny"}
                onClick={() => setShowMarkdown(!showMarkdown)}>
                  preview
                </Button>
              <Button
                variant={"disabled"}
                className={cn("bg-transparent",
                  !showMarkdown ? "bg-primary/10" : "")}
                size={"tiny"}
                onClick={() => setShowMarkdown(!showMarkdown)}>
                  text
                </Button>
            </div>
          }
          
            {
              <Button variant={"ghost"} size={"tiny"} onClick={() => {
                setEditingText(GetTextContent(message) ?? '');
                setIsEditing(true);
              }}>
                <Edit size={14} />
              </Button>
            }
            {
              GetTextContent(message) != undefined &&
              <CopyToClipboardIcon size={14} textValue={GetTextContent(message)!} />
            }
            <Button variant={"ghost"} size={"tiny"} onClick={() => handleDelete(message)}>
              <Trash size={14} />
            </Button>
            

          </div>
          <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant={"ghost"} size={"tiny"}>
                  <EllipsisVertical size={14} />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent className="w-40">
                <DropdownMenuLabel className="text-xs">Delete</DropdownMenuLabel>
                <DropdownMenuSeparator />
                <DropdownMenuItem className="text-xs" onSelect={() => handleDeleteMessageAbove(message)}>Delete all messages above</DropdownMenuItem>
                <DropdownMenuItem className="text-xs" onSelect={() => handleDeleteMessageBelow(message)}>Delete all messages below</DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
        </div>
        <div className="p-2 overflow-y-auto">
          {isEditing ?
            <div className="flex flex-col border-2 border-solid border-accent rounded-md">
              <textarea
                value={editingText}
                onChange={(e) => setEditingText(e.target.value)}
                className="w-full p-2 h-auto bg-transparent border-0 focus:outline-none"
              />
              <div className="flex gap-2 p-2 whitespace-pre-wrap justify-end">
                <Button
                  variant={"ghost"}
                  size={"tiny"}
                  onClick={() => handleEditing({ ...message, parts: [{ textPart: editingText }] })}
                >
                  Save
                </Button>
                <Button
                  variant={"warning"}
                  size={"tiny"}
                  onClick={() => setIsEditing(false)}
                >
                  Cancel
                </Button>
              </div>
            </div>
            :
            showMarkdown ? <Markdown>{markdown}</Markdown> : <span>{markdown}</span>
          }
        </div>
      </div>
    </div>
  );
}
