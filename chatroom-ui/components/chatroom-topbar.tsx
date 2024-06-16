import { cn } from "@/lib/utils"
import { GitHubLogoIcon } from "@radix-ui/react-icons"
import { Link } from "lucide-react"
import { CheckpointSelector } from "./checkpoint-selector"
import ThemeSwitch from "./theme-switch"
import { buttonVariants } from "./ui/button"
import { Welcome } from "./welcome"

interface TopBarProps {
    onSelectedCheckpoint?: (checkpoint: string | undefined) => void;
}
export const TopBar = ({
    onSelectedCheckpoint,
}: TopBarProps) => {
    return (
        <div className="flex justify-between items-center gap-2">
        <span className="text-4xl font-bold text-gradient text-nowrap">Agent Chatroom</span>
        <CheckpointSelector onSelectedCheckpoint={onSelectedCheckpoint} />
        <div className="flex w-full space-x-5 items-center justify-end ">
            <Welcome />
            <ThemeSwitch />
            <GitHubLogoIcon
                href="https://github.com/LittleLittleCloud/Agent-ChatRoom"
                className="w-7 h-7 text-muted-foreground" />
        </div>
    </div>
    )
}