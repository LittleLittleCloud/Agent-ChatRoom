import {
  FileImage,
  Mic,
  Paperclip,
  PlusCircle,
  SendHorizontal,
  Smile,
  ThumbsUp,
} from "lucide-react";
import Link from "next/link";
import React, { useRef, useState } from "react";
import { buttonVariants } from "../ui/button";
import { cn } from "@/lib/utils";
import { AnimatePresence, motion } from "framer-motion";
import { Textarea } from "@/components/ui/textarea";
import { EmojiPicker } from "@/components/emoji-picker";
import { Popover, PopoverContent, PopoverTrigger } from "../ui/popover";
import { AgentInfo, ChatMsg } from "@/chatroom-client";

interface ChatBottombarProps {
  user: AgentInfo
  sendMessage: (newMessage: ChatMsg) => void;
  isMobile: boolean;
}

// export const BottombarIcons = [{icon: PlusCircle}, { icon: FileImage }, { icon: Paperclip }];

export default function ChatBottombar({
  sendMessage,
  isMobile,
  user,
}: ChatBottombarProps) {
  const [message, setMessage] = useState("");
  const inputRef = useRef<HTMLTextAreaElement>(null);
  const [rowNumber, setRowNumber] = useState(1);

  const handleInputChange = (event: React.ChangeEvent<HTMLTextAreaElement>) => {
    const rows = event.target.value.split("\n").length;
    setRowNumber(rows);
    setMessage(event.target.value);
  };

  const handleThumbsUp = () => {
    const newMessage: ChatMsg = {
      from: user.name,
      parts: [{
        textPart: "👍"
      }],
      created: new Date().toISOString(),
    };
    sendMessage(newMessage);
    setMessage("");
  };

  const handleSend = () => {
    if (message.trim()) {
      const newMessage: ChatMsg = {
        from: user.name,
        parts: [{
          textPart: message.trim()
        }],
        created: new Date().toISOString(),
      };
      sendMessage(newMessage);
      setMessage("");

      if (inputRef.current) {
        inputRef.current.focus();
      }
    }
  };

  const handleKeyPress = (event: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (event.key === "Enter" && !event.shiftKey) {
      event.preventDefault();
      handleSend();
    }

    if (event.key === "Enter" && event.shiftKey) {
      // var cursorPosition = event.currentTarget.selectionStart;
      // setMessage((prev) => {
      //   return (
      //     prev.substring(0, cursorPosition) +
      //     "\n" +
      //     prev.substring(cursorPosition)
      //   );
      // });
    }
  };

  return (
    <div className="p-2 flex justify-between w-full items-center gap-2">
      <div className="flex">
        {/* <Popover>
              <PopoverTrigger asChild>
              <Link
            href="#"
            className={cn(
              buttonVariants({ variant: "ghost", size: "icon" }),
              "h-9 w-9",
              "dark:bg-muted dark:text-muted-foreground dark:hover:bg-muted dark:hover:text-white"
            )}
          >
          </Link>
              </PopoverTrigger>
              <PopoverContent 
              side="top"
              className="w-full p-2">
               {message.trim() || isMobile ? (
                 <div className="flex gap-2">
                  <Link 
                href="#"
                className={cn(
                  buttonVariants({ variant: "ghost", size: "icon" }),
                  "h-9 w-9",
                  "dark:bg-muted dark:text-muted-foreground dark:hover:bg-muted dark:hover:text-white"
                )}
                >
                  <Mic size={20} className="text-muted-foreground" />
                </Link>
               </div>
               ) : (
                <Link 
                href="#"
                className={cn(
                  buttonVariants({ variant: "ghost", size: "icon" }),
                  "h-9 w-9",
                  "dark:bg-muted dark:text-muted-foreground dark:hover:bg-muted dark:hover:text-white"
                )}
                >
                  <Mic size={20} className="text-muted-foreground" />
                </Link>
               )}
              </PopoverContent>
            </Popover> */}
        {/* {!message.trim() && !isMobile && (
            <div className="flex">
              {BottombarIcons.map((icon, index) => (
                <Link
                  key={index}
                  href="#"
                  className={cn(
                    buttonVariants({ variant: "ghost", size: "icon" }),
                    "h-9 w-9",
                    "dark:bg-muted dark:text-muted-foreground dark:hover:bg-muted dark:hover:text-white"
                  )}
                >
                  <icon.icon size={20} className="text-muted-foreground" />
                </Link>
              ))}
            </div>
          )} */}
      </div>

      <div
        className="w-full h-full  relative bg-none flex items-center border-accent border-solid rounded-md overflow-y-auto">
        <AnimatePresence initial={false}>
          <motion.div
            key="input"
            className="w-full relative flex items-center gap-2 pl-2"
            layout
            initial={{ opacity: 0, scale: 1 }}
            animate={{ opacity: 1, scale: 1 }}
            exit={{ opacity: 0, scale: 1 }}
            transition={{
              opacity: { duration: 0.05 },
              layout: {
                type: "spring",
                bounce: 0.15,
              },
            }}
          >
            <div
              className={cn(
                "flex w-full p-0 border-2 border-solid border-accent rounded-md",
                rowNumber > 1 ? "flex-col" : ""
              )}>
              <Textarea
                autoComplete="off"
                value={message}
                ref={inputRef}
                rows={rowNumber}
                onKeyDown={handleKeyPress}
                onChange={handleInputChange}
                name="message"
                placeholder="Aa"
                className={
                  cn("h-auto max-h-60 border-none flex items-center bg-background ring-0 focus:ring-0 focus:outline-none resize-none p-2",
                  )} />
              

              <div className="flex gap-3 justify-end items-center">
              <EmojiPicker onChange={(value) => {
                setMessage(message + value)
                if (inputRef.current) {
                  inputRef.current.focus();
                }
              }} />
                {message.trim() ? (
                  <Link
                    href="#"
                    className={cn(
                      buttonVariants({ variant: "ghost", size: "icon" }),
                      "w-12 h-9",
                      "dark:bg-muted dark:text-muted-foreground dark:hover:bg-muted dark:hover:text-white shrink-0"
                    )}
                    onClick={handleSend}
                  >
                    <SendHorizontal size={20} className="text-muted-foreground" />
                  </Link>
                ) : (
                  <Link
                    href="#"
                    className={cn(
                      buttonVariants({ variant: "ghost", size: "icon" }),
                      "w-12 h-9",
                      "dark:bg-muted dark:text-muted-foreground dark:hover:bg-muted dark:hover:text-white shrink-0"
                    )}
                    onClick={handleThumbsUp}
                  >
                    <ThumbsUp size={20} className="text-muted-foreground" />
                  </Link>
                )}
              </div>
            </div>
          </motion.div>
        </AnimatePresence>
      </div>
    </div>
  );
}