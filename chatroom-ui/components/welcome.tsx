import { AgentInfo, getApiChatRoomClientGetUserInfo } from "@/chatroom-client";
import { Label } from "@radix-ui/react-label";
import { useState, useEffect } from "react";

export function Welcome() {
    const [user, setUser] = useState<AgentInfo | undefined>(undefined);

    useEffect(() => {
        const user = getApiChatRoomClientGetUserInfo()
            .then((res) => {
                setUser(res);
            })
            .catch((err) => {
                console.log(err);
            });
    }, []);

    return (
        <div className="h-full items-center space-x-5">
            {user && (
                <label className="text-nowrap font-bold">Welcome {user?.name}</label>)
            }
        </div>
    );
}