import React from "react";
import { AgentAvatar } from "../agent-avatar";
import { ChatMessageProps } from "./chat-list";
import { cn } from "@/lib/utils";
import { Markdown } from "../markdown";


export function ChatMessage({ message, selectedUser }: ChatMessageProps) {
  const isFromSelectedUser = message.from === selectedUser.name;
  const [markdown, setMarkdown] = React.useState<string>('');

  React.useEffect(() => {
    if (message.text === undefined || message.text === null) {
      return;
    }

    setMarkdown(message.text);
  }, []);
  return (
    <div className={cn("flex gap-3 w-full border-solid border-2 border-accent p-3 rounded-md", isFromSelectedUser ? "justify-start" : "justify-end")}>
      {message.from === selectedUser.name && (
        <AgentAvatar agent={{ name: message.from }} />
      )}
      <div className="flex flex-col gap-1">
        <Markdown>
          {markdown}
        </Markdown>
        <span className=" bg-accent p-3 rounded-md">
          {message.text}
        </span>
      </div>
      {message.from !== selectedUser.name && (
        <AgentAvatar agent={{ name: message.from }} />
      )}
    </div>
  );
}
