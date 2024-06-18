import React from "react";
import { AgentAvatar } from "../agent-avatar";
import { cn } from "@/lib/utils";
import { Markdown } from "../markdown";
import { Badge } from "../ui/badge";
import { AgentInfo, ChatMsg, getApiChatRoomClientDeleteMessageByChannelNameByMessageId } from "@/chatroom-client";
import { ToggleGroup, ToggleGroupItem } from "../ui/toggle-group";
import { Columns, Columns2, Copy, Edit, RotateCcw, Trash, Type } from "lucide-react";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@radix-ui/react-tooltip";
import { Button } from "../ui/button";
import { CopyToClipboardIcon } from "../copy-to-clipboard-icon";
import { channel } from "diagnostics_channel";
import { on } from "events";
import EmojiPicker from "@emoji-mart/react";
import { GetTextContent } from "@/chatroom-client/types.extension";

export interface ChatMessageProps {
  message: ChatMsg;
  selectedUser: AgentInfo;
  onDeleted?: (msg: ChatMsg) => void;
  onResend?: (msg: ChatMsg) => void;
  onEdit?: (msg: ChatMsg) => void;
}

export function ChatMessage({ message, selectedUser, onDeleted, onResend, onEdit, }: ChatMessageProps) {
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
    else{
      setMarkdown(textContent);
    }
  }, [message]);

  const handleDelete = async (msg: ChatMsg) => {
    onDeleted?.(msg);
  }

  const handleEditing = async (msg: ChatMsg) => {
    setIsEditing(false);
    onEdit?.(msg);
  }

  return (
    <div className={cn("flex flex-col w-full ", isFromSelectedUser ? "items-start" : "items-end")}>
      <div className="flex flex-col m-3 w-9/12 border-solid border-2 border-accent rounded-md">
        <div
          className="flex items-center py-2 pr-3 bg-accent group/settings">
          <Badge className="text-nowrap" variant={"accent"}>{message.from}</Badge>
          <div className="invisible flex flex-grow items-center justify-end gap-2 group-hover/settings:visible">
            {/* <ToggleGroup defaultValue="markdown" type="single" size={"tiny"}
              variant={"accent"}
            >
              <ToggleGroupItem
                value="markdown"
                aria-label="Toggle markdown"
                onClick={() => setShowMarkdown(true)}>
                <TooltipProvider>
                  <Tooltip>
                    <TooltipTrigger asChild>
                      <Columns size={14} />
                    </TooltipTrigger>
                    <TooltipContent side="bottom" className="flex items-center gap-10">
                      Markdown
                    </TooltipContent>
                  </Tooltip>
                </TooltipProvider>
              </ToggleGroupItem>
              <ToggleGroupItem
                onClick={() => setShowMarkdown(false)}
                value="plain"
                aria-label="Toggle plain">
                <TooltipProvider>
                  <Tooltip>
                    <TooltipTrigger asChild>
                      <Type size={14} />
                    </TooltipTrigger>
                    <TooltipContent side="bottom" className="flex items-center gap-10">
                      Plain Text
                    </TooltipContent>
                  </Tooltip>
                </TooltipProvider>
              </ToggleGroupItem>
            </ToggleGroup> */}
            {
              isFromSelectedUser &&
              <Button variant={"ghost"} size={"tiny"} onClick={() => {
                setEditingText(GetTextContent(message) ?? '');
                setIsEditing(true);
              }}>
                <Edit size={14} />
              </Button>
            }
            {
              isFromSelectedUser &&
              <Button variant={"ghost"} size={"tiny"}>
                <RotateCcw size={14} />
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
        </div>
        <div className="p-3 overflow-y-auto">
          { isEditing ?
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
                onClick={() => handleEditing({ ...message, parts: [{ textPart: editingText }]})}
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
