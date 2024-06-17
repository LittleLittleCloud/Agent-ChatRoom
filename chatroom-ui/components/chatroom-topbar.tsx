import { cn } from "@/lib/utils"
import { GitHubLogoIcon } from "@radix-ui/react-icons"
import { CheckpointSelector } from "./checkpoint-selector"
import ThemeSwitch from "./theme-switch"
import { buttonVariants } from "./ui/button"
import { Welcome } from "./welcome"
import Link from "next/link"

interface TopBarProps {
    onSelectedCheckpoint?: (checkpoint: string | undefined) => void;
}
export const TopBar = ({
    onSelectedCheckpoint,
}: TopBarProps) => {
    return (
        <div className="flex justify-between items-center gap-2">
        <span className="text-xl font-bold text-gradient text-nowrap">Agent Chatroom</span>
        <CheckpointSelector onSelectedCheckpoint={onSelectedCheckpoint} />
        <div className="flex w-full space-x-5 items-center justify-end ">
            <Welcome />
            <ThemeSwitch />
            <Link
                href="https://github.com/LittleLittleCloud/Agent-ChatRoom"
                className={cn(
                    buttonVariants({ variant: "ghost", size: "icon" }),
                    "bg-transparent",
                )}>
            <GitHubLogoIcon
                className="w-7 h-7 text-muted-foreground" />
            </Link>
        </div>
    </div>
    )
}