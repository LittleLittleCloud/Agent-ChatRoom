import { getApiChatRoomClientGetRoomCheckpoints, getApiChatRoomClientLoadCheckpoint, getApiChatRoomClientSaveCheckpoint } from "@/chatroom-client";
import { useEffect, useState } from "react";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "./ui/select";
import { Button } from "./ui/button";
import { toast } from "sonner"
import { on } from "events";

interface CheckpointSelectorProps {
    onSelectedCheckpoint?: (checkpoint: string | undefined) => void;
    onSaveCheckpoint?: () => void;
}

export function CheckpointSelector({
    onSelectedCheckpoint,
    onSaveCheckpoint,
}: CheckpointSelectorProps) {
    const [checkpoints, setCheckpoints] = useState<string[]>([]);
    const [selectedCheckpointPath, setSelectedCheckpoint] = useState<string | undefined>(undefined);

    useEffect(() => {
        getApiChatRoomClientGetRoomCheckpoints()
            .then((res) => {
                setCheckpoints(res);
            })
            .catch((err) => {
                console.log(err);
            });
    }, []);

    const selectCheckpointHandler = async (checkpoint: string) => {
        if (checkpoint !== undefined) {
            console.log("Loading checkpoint", checkpoint);
            await getApiChatRoomClientLoadCheckpoint({
                checkpointPath: checkpoint,
            });

            setSelectedCheckpoint(checkpoint);
            onSelectedCheckpoint?.(checkpoint);
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

    return (
        <div className="flex items-center">
            <Select
                onValueChange={(value) => selectCheckpointHandler(value)}
            >
                <SelectTrigger>
                    <SelectValue placeholder={selectedCheckpointPath || "Select Checkpoint"} />
                </SelectTrigger>
                <SelectContent>
                    {checkpoints.map((checkpoint) => (
                        <SelectItem
                            key={checkpoint}
                            value={checkpoint}
                        >
                            {checkpoint}
                        </SelectItem>
                    ))}
                </SelectContent>
            </Select>
            <Button
                onClick={saveCheckpointHandler}
                variant="outline"
                className="ml-4"
            >
                Save Checkpoint
            </Button>
        </div>
    );
}