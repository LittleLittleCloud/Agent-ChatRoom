import { AgentInfo, ChannelInfo, getApiChatRoomClientGetChannels, getApiChatRoomClientGetRoomMembers, postApiChatRoomClientAddAgentToChannel, postApiChatRoomClientCreateChannel, postApiChatRoomClientGetChannelMembers, postApiChatRoomClientRemoveAgentFromChannel, getApiChatRoomClientGetOrchestrators, postApiChatRoomClientAddOrchestratorToChannel, getApiChatRoomClientGetChannelInfoByChannelName, postApiChatRoomClientRemoveOrchestratorFromChannel, postApiChatRoomClientEditChannelName } from "@/chatroom-client";
import { Separator } from "@radix-ui/react-separator";
import { StarIcon, ChevronDownIcon, PlusIcon, CircleIcon } from "lucide-react";
import { useEffect, useState } from "react";
import { Button } from "../ui/button";
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from "../ui/card";
import { Label } from "@radix-ui/react-label";
import { Channel } from "@/types/channel";

interface ChannelConfigModalProps {
    onClose: () => void;
    channel: Channel;
    onSave: (channel: Channel) => void;
}

export function ChannelConfigModal({ onClose, channel, onSave }: ChannelConfigModalProps) {
    const [channelName, setChannelName] = useState(channel.name ?? '');
    const [members, setMembers] = useState<AgentInfo[]>([]);
    const [availableMembers, setAvailableMembers] = useState<AgentInfo[] | undefined>(undefined);
    const [orchestrators, setOrchestrators] = useState<string[]>([]);
    const [availableOrchestrators, setAvailableOrchestrators] = useState<string[] | undefined>(undefined);

    useEffect(() => {
        if (availableMembers == undefined) {
            getApiChatRoomClientGetRoomMembers()
                .then((res) => {
                    setAvailableMembers(res);
                })
                .catch((err) => {
                    console.log(err);
                });
        }

        if (availableOrchestrators == undefined) {
            getApiChatRoomClientGetOrchestrators()
                .then((res) => {
                    setAvailableOrchestrators(res);
                })
                .catch((err) => {
                    console.log(err);
                });
        }

        if (channelName) {
            getApiChatRoomClientGetChannelInfoByChannelName({
                channelName: channelName
            })
                .then((res) => {
                    setMembers(res.members ?? []);
                    setOrchestrators(res.orchestrators ?? []);
                })
                .catch((err) => {
                    console.log(err);
                });
        }
    }, []);

    const saveChannelHandler = async () => {
        if (availableMembers == undefined || availableMembers.length == 0) {
            alert("No available agents, please configure the chatroom to include more agents.");
            return;
        }

        if (channelName == '') {
            alert("Please enter a channel name.");
            return;
        }
        var channels = await getApiChatRoomClientGetChannels();

        var channelExists = channels.map(channel => channel.name).includes(channelName);
        if (channel.name == undefined) {
            // create a new channel
            if (channelExists) {
                alert("Channel already exists, please choose a different name.");
                return;
            }
            await postApiChatRoomClientCreateChannel(
                {
                    requestBody: {
                        channelName: channelName
                    }
                });

            await Promise.all(members.map(member => postApiChatRoomClientAddAgentToChannel(
                {
                    requestBody: {
                        agentName: member.name,
                        channelName: channelName,
                    }
                })));

            await Promise.all(orchestrators.map(orchestrator => postApiChatRoomClientAddOrchestratorToChannel(
                {
                    requestBody: {
                        orchestratorName: orchestrator,
                        channelName: channelName,
                    }
                })));
        }
        else {
            // edit existing channel
            if (channelName != channel.name && channelExists) {
                alert("Channel already exists, please choose a different name.");
                return;
            }

            var channelInfo = await getApiChatRoomClientGetChannelInfoByChannelName({ channelName: channel.name })

            var originalMembers = channelInfo.members ?? [];
            var originalOrchestrators = channelInfo.orchestrators ?? [];

            var membersToAdd = members.filter(member => !originalMembers.map(member => member.name).includes(member.name));
            var membersToRemove = originalMembers.filter(member => !members.map(member => member.name).includes(member.name));

            await Promise.all(membersToAdd.map(member => postApiChatRoomClientAddAgentToChannel(
                {
                    requestBody: {
                        agentName: member.name,
                        channelName: channel.name,
                    }
                })));

            await Promise.all(membersToRemove.map(member => postApiChatRoomClientRemoveAgentFromChannel(
                {
                    requestBody: {
                        agentName: member.name,
                        channelName: channel.name,
                    }
                })));

            var orchestratorsToAdd = orchestrators.filter(orchestrator => !originalOrchestrators.includes(orchestrator));
            var orchestratorsToRemove = originalOrchestrators.filter(orchestrator => !orchestrators.includes(orchestrator));

            await Promise.all(orchestratorsToAdd.map(orchestrator => postApiChatRoomClientAddOrchestratorToChannel(
                {
                    requestBody: {
                        orchestratorName: orchestrator,
                        channelName: channel.name,
                    }
                })));

            await Promise.all(orchestratorsToRemove.map(orchestrator => postApiChatRoomClientRemoveOrchestratorFromChannel(
                {
                    requestBody: {
                        orchestratorName: orchestrator,
                        channelName: channel.name,
                    }
                })));

            if (channelName != channel.name) {
                await postApiChatRoomClientEditChannelName({
                    requestBody: {
                        oldChannelName: channel.name,
                        newChannelName: channelName,
                    }
                });
            }
        }

        onSave({
            ...channel,
            name: channelName,
        });
        onClose();
    };

    return (
        <div
            className="relative z-10"
            role="dialog"
            aria-modal="true">
            <div
                className="fixed inset-0 overflow-y-auto z-10 w-screen">
                <div className="flex items-end  justify-center p-4 text-center sm:items-center sm:p-0">
                    <div className=" rounded-lg bg-neutral-100 dark:bg-neutral-900 text-left shadow-xl sm:my-8 sm:w-full sm:max-w-lg">
                        <div className="px-4 pb-4 pt-5 sm:p-6 sm:pb-4">
                            <div className="sm:flex sm:items-start space-x-2">
                                <Label className="text-lg font-bold">Edit Group</Label>
                            </div>
                            <Label className="text-sm font-medium">Group Name</Label>
                            <div className="flex mb-5 rounded-md shadow-sm ring-1 ring-inset focus-within:ring-2 sm:max-w-md mt-2">
                                <input
                                    className="flex-1 border-0 bg-transparent py-1 pl-2 text-slate-900 dark:text-slate-100 focus:ring-0 focus-within:ring-0 sm:text-sm sm:leading-6"
                                    type="text"
                                    value={channelName}
                                    onChange={(e) => setChannelName((e.target as HTMLInputElement).value)} />
                            </div>
                            <Label className="text-sm font-medium">Member</Label>
                            <div className="flex flex-wrap rounded-md shadow-sm  mt-2 space-x-5">
                                {
                                    availableMembers && availableMembers.length > 0 &&
                                    availableMembers.map((agent, index) => (
                                        (agent.name && <div
                                            key={index}
                                            className="flex items-center space-x-2">
                                            <input
                                                id={agent.name}
                                                name="agent"
                                                type="checkbox"
                                                className="focus:ring-0 h-4 w-4 text-neutral-900 border-yellow"
                                                checked={members.map(member => member.name).includes(agent.name)}
                                                onChange={async () => {
                                                    if (members.map(member => member.name).includes(agent.name)) {
                                                        setMembers(members.filter(member => member.name !== agent.name));
                                                    }

                                                    else {
                                                        setMembers([...members, agent]);
                                                    }
                                                }}
                                            />
                                            <Label className="text-nowrap">{agent.name}</Label>
                                        </div>
                                        )))
                                }
                                {
                                    !availableMembers || availableMembers.length == 0 &&
                                    <Label>No available agents, please configure the chatroom to include more agents.</Label>
                                }
                            </div>

                            <Label className="text-sm font-medium">Orchestrator</Label>
                            <div className="flex flex-wrap rounded-md shadow-sm  mt-2 space-x-5">
                                {
                                    availableOrchestrators && availableOrchestrators.length > 0 &&
                                    availableOrchestrators.map((orchestrator, index) => (
                                        (orchestrator && <div
                                            key={index}
                                            className="flex items-center space-x-2">
                                            <input
                                                id={orchestrator}
                                                type="checkbox"
                                                className="focus:ring-0 h-4 w-4 text-neutral-900 border-yellow"
                                                checked={orchestrators.includes(orchestrator)}
                                                onChange={async () => {
                                                    if (orchestrators.includes(orchestrator)) {
                                                        setOrchestrators(orchestrators.filter(orch => orch !== orchestrator))
                                                    }
                                                    else {
                                                        setOrchestrators([...orchestrators, orchestrator]);
                                                    }
                                                }}
                                            />
                                            <Label className="text-nowrap">{orchestrator}</Label>
                                        </div>
                                        )))
                                }
                                {
                                    !availableOrchestrators || availableOrchestrators.length == 0 &&
                                    <Label>No available orchestrators, please configure the chatroom to include more orchestrators.</Label>
                                }
                            </div>
                        </div>
                        <div
                            className="px-4 py-3 sm:px-6 flex justify-end sm:flex sm:flex-row">
                            <button
                                type="button"
                                className="w-full inline-flex justify-center rounded-md border-0 border-transparent shadow-sm px-4 py-2 bg-neutral-500 dark:bg-neutral-700 hover:bg-neutral-400 dark:hover:bg-neutral-500 text-base font-medium text-white hover:bg-neutral-800 focus:outline-none sm:ml-3 sm:w-auto sm:text-sm"
                                onClick={async () => {
                                    await saveChannelHandler();
                                }}>
                                Save
                            </button>
                            <button
                                type="button"
                                className="mt-3 w-full inline-flex justify-center rounded-md border-0 shadow-sm px-4 py-2 bg-red-500 dark:bg-red-700  hover:bg-red-400 dark:hover:bg-red-500 text-base font-medium text-neutral-900 focus:outline-none sm:mt-0 sm:ml-3 sm:w-auto sm:text-sm"
                                onClick={onClose}>
                                Cancel
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}