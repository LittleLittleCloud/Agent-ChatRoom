import { Label } from "@radix-ui/react-label";
import { useTheme } from "next-themes";
import { useEffect, useState } from "react";
import { Switch } from "./ui/switch";

export function ThemeSwitch() {
    const [mounted, setMounted] = useState(false)
    const { theme, setTheme } = useTheme()

    // useEffect only runs on the client, so now we can safely show the UI
    useEffect(() => {
        setMounted(true)
    }, [])

    if (!mounted) {
        return null
    }

    return (
        <div className="flex items-center space-x-2 h-full">
            <Switch id="dark-mode" checked={theme === "dark"} onClick={() => {
                setTheme(theme === "dark" ? "light" : "dark");
            }} />
            <Label htmlFor="dark-mode" className="text-nowrap">Dark Mode</Label>
        </div>
    )
}

export default ThemeSwitch;