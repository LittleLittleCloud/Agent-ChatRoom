import { AgentInfo } from "@/chatroom-client";
import { Avatar, AvatarFallback, AvatarImage } from "./ui/avatar";

interface AgentAvatarProps {
    agent: AgentInfo & {avatar?: string};
}

export function AgentAvatar({ agent } : AgentAvatarProps) {
    return (
        <Avatar className="flex justify-center items-center">
          <AvatarImage
            src={agent.avatar}
            alt={agent.avatar}
            width={6}
            height={6}
            className="w-10 h-10 " />
          {agent.name && <AvatarFallback className="w-10 h-10">{agent.name}</AvatarFallback>}
        </Avatar>
    );
}