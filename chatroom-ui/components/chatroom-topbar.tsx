import { cn } from "@/lib/utils"
import { GitHubLogoIcon } from "@radix-ui/react-icons"
import { CheckpointSelector } from "./checkpoint-selector"
import ThemeSwitch from "./theme-switch"
import { buttonVariants } from "./ui/button"
import { Welcome } from "./welcome"
import Link from "next/link"
import { useEffect, useState } from "react"
import { getApiChatRoomClientVersion } from "@/chatroom-client"
import { Badge } from "./ui/badge"

interface TopBarProps {
    onSelectedCheckpoint?: (checkpoint: string | undefined) => void;
}
export const TopBar = ({
    onSelectedCheckpoint,
}: TopBarProps) => {
    const [serverVersion, setServerVersion] = useState<string | undefined>(undefined)

    useEffect(() => {
        getApiChatRoomClientVersion()
        .then((response) => {
            setServerVersion(response)
        })
        .catch((error) => {
            console.error("Failed to get server version", error)
        })

    }
    , [])
    return (
        <div className="flex flex-wrap items-center gap-5">
        <span className="text-xl font-bold text-gradient text-nowrap">Agent Chatroom</span>
        <Badge variant="accent" className="text-nowrap">version: {serverVersion}</Badge>
        <div className="flex grow space-x-5 items-center justify-end ">
        <CheckpointSelector onSelectedCheckpoint={onSelectedCheckpoint} />
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