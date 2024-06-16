import Image from "next/image";
import { Inter } from "next/font/google";
import { ChatLayout } from "@/components/chat/chat-layout";
import { cookies } from "next/headers";
import { useEffect, useState } from "react";
import { AgentInfo, getApiChatRoomClientGetUserInfo } from "@/chatroom-client";
import { Label } from "@radix-ui/react-label";

const inter = Inter({ subsets: ["latin"] });

interface HomeProps {
  checkpoint?: string;
}
export default function Home(
  {
    checkpoint,
  }: HomeProps
) {
  const [layout, _] = useState(undefined);
  const [user, setUser] = useState<AgentInfo | undefined>(undefined);

  useEffect(() => {
      getApiChatRoomClientGetUserInfo()
          .then((res) => {
              setUser(res);
          })
          .catch((err) => {
              console.log(err);
          });
  }, []);
  const defaultLayout = layout ? JSON.parse(layout) : undefined;
  useEffect(() => {
    localStorage.getItem("react-resizable-panels:layout");
  }, [layout]);

  if (user){
    return (
      <div className="z-10 border rounded-lg w-full h-full text-sm">
      <ChatLayout checkPoint={checkpoint} selectedUser={user} defaultCollapsed={true} defaultLayout={defaultLayout} navCollapsedSize={8} />
    </div>
    );
  }
  else
  {
    // show loading screen
    return (
      <div className="flex items-center justify-center h-screen">
        <div className="flex flex-col items-center">
          <Label className="text-lg">Loading...</Label>
        </div>
      </div>
    );
  }
}
