import { getApiChatRoomClientDeleteCheckpointByCheckpointPath, getApiChatRoomClientGetRoomCheckpoints, getApiChatRoomClientLoadCheckpoint, getApiChatRoomClientSaveCheckpoint, getApiChatRoomClientUnloadCheckpoint } from "@/chatroom-client";
import { useEffect, useState } from "react";
import { Select, SelectContent, SelectItem, SelectSeparator, SelectTrigger, SelectValue } from "./ui/select";
import { Button } from "./ui/button";
import { toast } from "sonner"
import { on } from "events";
import { Trash } from "lucide-react";
import { cn } from "@/lib/utils";

interface CheckpointSelectorProps {
    onSelectedCheckpoint?: (checkpoint: string | undefined) => void;
    onSaveCheckpoint?: () => void;
}

export function CheckpointSelector({
    onSelectedCheckpoint,
    onSaveCheckpoint,
}: CheckpointSelectorProps) {
    const [checkpoints, setCheckpoints] = useState<string[]>([]);
    const [selectedCheckpointPath, setSelectedCheckpoint] = useState<string | "None">("None");

    useEffect(() => {
        getApiChatRoomClientGetRoomCheckpoints()
            .then((res) => {
                setCheckpoints(res);
            })
            .catch((err) => {
                console.log(err);
            });
    }, []);

    const selectCheckpointHandler = async (checkpoint: string | "None") => {
        if (checkpoint !== "None") {
            console.log("Loading checkpoint", checkpoint);
            await getApiChatRoomClientLoadCheckpoint({
                checkpointName: checkpoint,
            });

            setSelectedCheckpoint(checkpoint);
            onSelectedCheckpoint?.(checkpoint);
        }
        else {
            await getApiChatRoomClientUnloadCheckpoint();
            setSelectedCheckpoint("None");
            onSelectedCheckpoint?.("None");
        }
    }

    const saveCheckpointHandler = async () => {
        console.log("Saving checkpoint");
        await getApiChatRoomClientSaveCheckpoint();

        var checkpoints = await getApiChatRoomClientGetRoomCheckpoints();
        setCheckpoints(checkpoints);
        toast("checkpoint has been saved successfully!");
        onSaveCheckpoint?.();
    }

    const handleDelete = async (checkpoint: string) => {
        if (confirm("Are you sure you want to delete this checkpoint?") === false) {
            return;
        }
        await getApiChatRoomClientDeleteCheckpointByCheckpointPath({
            checkpointPath: checkpoint,
        });

        var checkpoints = await getApiChatRoomClientGetRoomCheckpoints();
        setCheckpoints(checkpoints);
    }

    return (
        <div className="flex items-center">
            <Select
                value={selectedCheckpointPath}
                onValueChange={(value) => selectCheckpointHandler(value)}
            >
                <SelectTrigger>
                    <SelectValue placeholder={selectedCheckpointPath === "None" ? "Select Checkpoint" : selectedCheckpointPath}>
                        <span
                            className="overflow-visible whitespace-nowrap"
                        >
                            {selectedCheckpointPath === "None" ? "Select Checkpoint" : selectedCheckpointPath}
                        </span>
                    </SelectValue>
                </SelectTrigger>
                <SelectContent className="overflow-y-auto">
                    <SelectItem
                        key = "None"
                        value="None"
                    >
                        None
                    </SelectItem>
                    <SelectSeparator />
                    {checkpoints.map((checkpoint) => (

                        <div
                            key={checkpoint}
                            className="pr-2 flex gap-2 overflow-visible whitespace-nowrap group/settings hover:bg-accent">
                            <SelectItem
                                value={checkpoint}
                            >
                                {checkpoint}
                            </SelectItem>
                            <Button
                                variant={"ghost"}
                                size={"tiny"}
                                onClick={() => handleDelete(checkpoint)}
                                className={cn("invisible",
                                    selectedCheckpointPath != checkpoint && "group-hover/settings:visible"
                                )}
                            >
                                <Trash size={14} />
                            </Button>
                        </div>
                    ))}
                </SelectContent>

            </Select>
            <Button
                onClick={saveCheckpointHandler}
                variant="outline"
                className="ml-4"
            >
                Create Checkpoint
            </Button>
        </div>
    );
}