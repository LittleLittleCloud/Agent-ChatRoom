import Image from "next/image";
import { Inter } from "next/font/google";
import { ChatLayout } from "@/components/chat/chat-layout";
import { cookies } from "next/headers";
import { useEffect, useState } from "react";

const inter = Inter({ subsets: ["latin"] });

export default function Home() {
  const [layout, _] = useState(undefined);
  const defaultLayout = layout ? JSON.parse(layout) : undefined;
  useEffect(() => {
    localStorage.getItem("react-resizable-panels:layout");
  }, [layout]);
  return (
      <div className="z-10 border rounded-lg w-full h-full text-sm">
        <ChatLayout defaultLayout={defaultLayout} navCollapsedSize={8} />
      </div>
  );
}
