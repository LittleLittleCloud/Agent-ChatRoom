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
import { CheckpointSelector } from "./checkpoint-selector";
import { Toaster } from "sonner";
import { TopBar } from "./chatroom-topbar";
import React from "react";

interface RootLayoutProps {
    children: React.ReactNode;
    onSelectedCheckpoint?: (checkpoint: string | undefined) => void;
}
export default function RootLayout({
    children,
    onSelectedCheckpoint,
}: Readonly<RootLayoutProps>) {
    return (
        <ThemeProvider
            defaultTheme="light"
            attribute="class"
        >
            <div className="justify-between px-10 py-4 w-full h-screen">
                <TopBar onSelectedCheckpoint={onSelectedCheckpoint}  />
                <main
                    className="h-[calc(100vh-80px)] items-center py-4 relative w-full">
                    {children}
                </main>
                <Toaster />
            </div>
        </ThemeProvider>
    );
}