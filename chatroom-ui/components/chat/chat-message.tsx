import React from "react";
import { AgentAvatar } from "../agent-avatar";
import { cn } from "@/lib/utils";
import { Markdown } from "../markdown";
import { Badge } from "../ui/badge";
import { AgentInfo, ChatMsg } from "@/chatroom-client";
import { ToggleGroup, ToggleGroupItem } from "../ui/toggle-group";
import { Columns, Columns2, Copy, Edit, RotateCcw, Trash, Type } from "lucide-react";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@radix-ui/react-tooltip";
import { Button } from "../ui/button";

export interface ChatMessageProps {
  message: ChatMsg;
  selectedUser: AgentInfo;
  onDeleted?: (msg: ChatMsg) => void;
  onResend?: (msg: ChatMsg) => void;
  onEdit?: (msg: ChatMsg) => void;
}

export function ChatMessage({ message, selectedUser }: ChatMessageProps) {
  const isFromSelectedUser = message.from === selectedUser.name;
  const [markdown, setMarkdown] = React.useState<string>('');
  const [showResend, setShowResend] = React.useState<boolean>(message.from == selectedUser.name);
  const [showMarkdown, setShowMarkdown] = React.useState<boolean>(true);
  React.useEffect(() => {
    if (message.text === undefined || message.text === null) {
      return;
    }

    setMarkdown(message.text);
  }, []);
  return (
    <div className={cn("flex flex-col w-full ", isFromSelectedUser ? "items-start" : "items-end")}>
      <div className="flex flex-col m-3 w-9/12 border-solid border-2 border-accent rounded-md">
        <div
          className="flex items-center py-2 bg-accent group/settings">
          <Badge variant={"accent"}>{message.from}</Badge>
          <div className="invisible flex flex-grow w-full items-center justify-end gap-2 group-hover/settings:visible">
            <ToggleGroup defaultValue="markdown" type="single" size={"tiny"}
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
            </ToggleGroup>
            {
              isFromSelectedUser &&
              <Button variant={"ghost"} size={"tiny"}>
                <Edit size={14} />
              </Button>
            }
            {
              isFromSelectedUser &&
              <Button variant={"ghost"} size={"tiny"}>
                <RotateCcw size={14} />
              </Button>
            }
            <Button variant={"ghost"} size={"tiny"}>
              <Copy size={14} />
            </Button>
            <Button variant={"ghost"} size={"tiny"}>
              <Trash size={14} />
            </Button>
          </div>
        </div>
        <div className="p-3 overflow-y-auto">
          {
            showMarkdown ? <Markdown>{markdown}</Markdown> : <span>{markdown}</span>
          }
        </div>
      </div>
    </div>
  );
}
