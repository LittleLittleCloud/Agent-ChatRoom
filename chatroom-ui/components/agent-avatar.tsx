import { AgentInfo } from "@/chatroom-client";
import { Avatar, AvatarFallback, AvatarImage } from "./ui/avatar";
import { Tooltip, TooltipProvider } from "@radix-ui/react-tooltip";
import { TooltipContent, TooltipTrigger } from "./ui/tooltip";

interface AgentAvatarProps {
    agent: AgentInfo & { avatar?: string };
    size?: number;
}

export function AgentAvatar({ agent, size = 6 }: AgentAvatarProps) {
    return (
        <TooltipProvider>
            <Tooltip>
                <TooltipTrigger asChild>
                    <Avatar className="flex justify-center items-center">
                        <AvatarImage
                            src={agent.avatar}
                            alt={agent.avatar}
                            width={size}
                            height={size}
                            className="w-10 h-10 " />
                        {agent.name && <AvatarFallback className="w-10 h-10">{agent.name.charAt(0)}</AvatarFallback>}
                    </Avatar>
                </TooltipTrigger>
                <TooltipContent side="bottom" className="flex items-center gap-10">
                    {agent.name}
                </TooltipContent>
            </Tooltip>
        </TooltipProvider>

    );
}