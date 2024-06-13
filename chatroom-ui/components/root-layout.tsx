"use client"

import Link from "next/link";
import { Button, buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { GitHubLogoIcon } from "@radix-ui/react-icons";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { ThemeProvider } from "@/components/theme-provider";
import { useTheme } from "next-themes";
import { ThemeSwitch } from "./theme-switch";
import { Welcome } from "./welcome";

export default function RootLayout({
    children,
}: Readonly<{
    children: React.ReactNode;
}>) {
    return (
        <ThemeProvider
            defaultTheme="light"
            attribute="class"
        >
            <div className="justify-between px-10 py-4 w-full h-screen">
                <div className="flex w-full justify-between items-center">
                    <Link href="#" className="text-4xl w-full font-bold text-gradient">Agent Chatroom</Link>
                    <div className="flex space-x-5 items-center justify-end ">
                        <Welcome />
                        <ThemeSwitch />
                        <Link
                            href="https://github.com/LittleLittleCloud/Agent-ChatRoom"
                            className={cn(
                                buttonVariants({ variant: "ghost", size: "icon" }),
                                "h-10 w-10"
                            )}
                        >
                            <GitHubLogoIcon className="w-7 h-7 text-muted-foreground" />
                        </Link>
                    </div>
                </div>

                <main className="h-[calc(100vh-80px)] items-center py-4 relative w-full">

                    {children}
                </main>
            </div>
        </ThemeProvider>
    );
}