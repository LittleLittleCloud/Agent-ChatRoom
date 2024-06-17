// This file is auto-generated by @hey-api/openapi-ts

export type AddAgentToChannelRequest = {
    channelName?: string | null;
    agentName?: string | null;
};

export type AddOrchestratorToChannelRequest = {
    channelName?: string | null;
    orchestratorName?: string | null;
};

export type AgentInfo = {
    name?: string | null;
    selfDescription?: string | null;
    isHuman?: boolean;
};

export type ChannelInfo = {
    name?: string | null;
    members?: Array<AgentInfo> | null;
    orchestrators?: Array<(string)> | null;
};

export type ChatMsg = {
    from?: string | null;
    created?: string;
    parts?: Array<ChatMsgPart> | null;
    id?: number;
};

export type ChatMsgPart = {
    textPart?: string | null;
    imageData?: string | null;
    imageUrl?: string | null;
    mimeType?: string | null;
};

export type CreateChannelRequest = {
    channelName?: string | null;
};

export type DeleteChannelRequest = {
    channelName?: string | null;
};

export type EditTextMessageRequest = {
    channelName?: string | null;
    messageId?: number;
    newText?: string | null;
};

export type GenerateNextReplyRequest = {
    channelName?: string | null;
    chatMsgs?: Array<ChatMsg> | null;
    candidates?: Array<(string)> | null;
    orchestrator?: string | null;
};

export type GenerateNextReplyResponse = {
    message?: ChatMsg;
};

export type GetChannelChatHistoryRequest = {
    channelName?: string | null;
    count?: number;
};

export type GetChannelMembersRequest = {
    channelName?: string | null;
};

export type JoinChannelRequest = {
    channelName?: string | null;
    createIfNotExists?: boolean;
};

export type LeaveChannelRequest = {
    channelName?: string | null;
};

export type RemoveAgentFromChannelRequest = {
    channelName?: string | null;
    agentName?: string | null;
};

export type RemoveOrchestratorFromChannelRequest = {
    channelName?: string | null;
    orchestratorName?: string | null;
};

export type SendTextMessageToChannelRequest = {
    channelName?: string | null;
    message?: ChatMsg;
};

export type PostApiChatRoomClientSendTextMessageToChannelData = {
    requestBody?: SendTextMessageToChannelRequest;
};

export type PostApiChatRoomClientSendTextMessageToChannelResponse = unknown;

export type GetApiChatRoomClientGetRoomCheckpointsResponse = Array<(string)>;

export type GetApiChatRoomClientUnloadCheckpointResponse = unknown;

export type GetApiChatRoomClientLoadCheckpointData = {
    checkpointName?: string;
};

export type GetApiChatRoomClientLoadCheckpointResponse = unknown;

export type GetApiChatRoomClientSaveCheckpointResponse = unknown;

export type GetApiChatRoomClientDeleteCheckpointByCheckpointPathData = {
    checkpointPath: string;
};

export type GetApiChatRoomClientDeleteCheckpointByCheckpointPathResponse = unknown;

export type GetApiChatRoomClientGetChannelsResponse = Array<ChannelInfo>;

export type GetApiChatRoomClientGetUserInfoResponse = AgentInfo;

export type GetApiChatRoomClientClearHistoryByChannelNameData = {
    channelName: string;
};

export type GetApiChatRoomClientClearHistoryByChannelNameResponse = unknown;

export type GetApiChatRoomClientNewMessageSseByChannelNameData = {
    channelName: string;
};

export type GetApiChatRoomClientNewMessageSseByChannelNameResponse = unknown;

export type PostApiChatRoomClientGenerateNextReplyData = {
    requestBody?: GenerateNextReplyRequest;
};

export type PostApiChatRoomClientGenerateNextReplyResponse = GenerateNextReplyResponse;

export type GetApiChatRoomClientGetRoomMembersResponse = Array<AgentInfo>;

export type GetApiChatRoomClientGetChannelInfoByChannelNameData = {
    channelName: string;
};

export type GetApiChatRoomClientGetChannelInfoByChannelNameResponse = ChannelInfo;

export type PostApiChatRoomClientEditTextMessageData = {
    requestBody?: EditTextMessageRequest;
};

export type PostApiChatRoomClientEditTextMessageResponse = unknown;

export type GetApiChatRoomClientDeleteMessageByChannelNameByMessageIdData = {
    channelName: string;
    messageId: number;
};

export type GetApiChatRoomClientDeleteMessageByChannelNameByMessageIdResponse = unknown;

export type PostApiChatRoomClientGetChannelMembersData = {
    requestBody?: GetChannelMembersRequest;
};

export type PostApiChatRoomClientGetChannelMembersResponse = Array<AgentInfo>;

export type PostApiChatRoomClientGetChannelChatHistoryData = {
    requestBody?: GetChannelChatHistoryRequest;
};

export type PostApiChatRoomClientGetChannelChatHistoryResponse = Array<ChatMsg>;

export type PostApiChatRoomClientCreateChannelData = {
    requestBody?: CreateChannelRequest;
};

export type PostApiChatRoomClientCreateChannelResponse = unknown;

export type PostApiChatRoomClientJoinChannelData = {
    requestBody?: JoinChannelRequest;
};

export type PostApiChatRoomClientJoinChannelResponse = unknown;

export type PostApiChatRoomClientLeaveChannelData = {
    requestBody?: LeaveChannelRequest;
};

export type PostApiChatRoomClientLeaveChannelResponse = unknown;

export type PostApiChatRoomClientDeleteChannelData = {
    requestBody?: DeleteChannelRequest;
};

export type PostApiChatRoomClientDeleteChannelResponse = unknown;

export type PostApiChatRoomClientAddAgentToChannelData = {
    requestBody?: AddAgentToChannelRequest;
};

export type PostApiChatRoomClientAddAgentToChannelResponse = unknown;

export type PostApiChatRoomClientRemoveAgentFromChannelData = {
    requestBody?: RemoveAgentFromChannelRequest;
};

export type PostApiChatRoomClientRemoveAgentFromChannelResponse = unknown;

export type GetApiChatRoomClientGetOrchestratorsResponse = Array<(string)>;

export type PostApiChatRoomClientAddOrchestratorToChannelData = {
    requestBody?: AddOrchestratorToChannelRequest;
};

export type PostApiChatRoomClientAddOrchestratorToChannelResponse = unknown;

export type PostApiChatRoomClientRemoveOrchestratorFromChannelData = {
    requestBody?: RemoveOrchestratorFromChannelRequest;
};

export type PostApiChatRoomClientRemoveOrchestratorFromChannelResponse = unknown;

export type $OpenApiTs = {
    '/api/ChatRoomClient/SendTextMessageToChannel': {
        post: {
            req: PostApiChatRoomClientSendTextMessageToChannelData;
            res: {
                /**
                 * OK
                 */
                200: unknown;
            };
        };
    };
    '/api/ChatRoomClient/GetRoomCheckpoints': {
        get: {
            res: {
                /**
                 * OK
                 */
                200: Array<(string)>;
            };
        };
    };
    '/api/ChatRoomClient/UnloadCheckpoint': {
        get: {
            res: {
                /**
                 * OK
                 */
                200: unknown;
            };
        };
    };
    '/api/ChatRoomClient/LoadCheckpoint': {
        get: {
            req: GetApiChatRoomClientLoadCheckpointData;
            res: {
                /**
                 * OK
                 */
                200: unknown;
            };
        };
    };
    '/api/ChatRoomClient/SaveCheckpoint': {
        get: {
            res: {
                /**
                 * OK
                 */
                200: unknown;
            };
        };
    };
    '/api/ChatRoomClient/DeleteCheckpoint/{checkpointPath}': {
        get: {
            req: GetApiChatRoomClientDeleteCheckpointByCheckpointPathData;
            res: {
                /**
                 * OK
                 */
                200: unknown;
            };
        };
    };
    '/api/ChatRoomClient/GetChannels': {
        get: {
            res: {
                /**
                 * OK
                 */
                200: Array<ChannelInfo>;
            };
        };
    };
    '/api/ChatRoomClient/GetUserInfo': {
        get: {
            res: {
                /**
                 * OK
                 */
                200: AgentInfo;
            };
        };
    };
    '/api/ChatRoomClient/ClearHistory/{channelName}': {
        get: {
            req: GetApiChatRoomClientClearHistoryByChannelNameData;
            res: {
                /**
                 * OK
                 */
                200: unknown;
            };
        };
    };
    '/api/ChatRoomClient/NewMessageSse/{channelName}': {
        get: {
            req: GetApiChatRoomClientNewMessageSseByChannelNameData;
            res: {
                /**
                 * OK
                 */
                200: unknown;
            };
        };
    };
    '/api/ChatRoomClient/GenerateNextReply': {
        post: {
            req: PostApiChatRoomClientGenerateNextReplyData;
            res: {
                /**
                 * OK
                 */
                200: GenerateNextReplyResponse;
            };
        };
    };
    '/api/ChatRoomClient/GetRoomMembers': {
        get: {
            res: {
                /**
                 * OK
                 */
                200: Array<AgentInfo>;
            };
        };
    };
    '/api/ChatRoomClient/GetChannelInfo/{channelName}': {
        get: {
            req: GetApiChatRoomClientGetChannelInfoByChannelNameData;
            res: {
                /**
                 * OK
                 */
                200: ChannelInfo;
            };
        };
    };
    '/api/ChatRoomClient/EditTextMessage': {
        post: {
            req: PostApiChatRoomClientEditTextMessageData;
            res: {
                /**
                 * OK
                 */
                200: unknown;
            };
        };
    };
    '/api/ChatRoomClient/DeleteMessage/{channelName}/{messageId}': {
        get: {
            req: GetApiChatRoomClientDeleteMessageByChannelNameByMessageIdData;
            res: {
                /**
                 * OK
                 */
                200: unknown;
            };
        };
    };
    '/api/ChatRoomClient/GetChannelMembers': {
        post: {
            req: PostApiChatRoomClientGetChannelMembersData;
            res: {
                /**
                 * OK
                 */
                200: Array<AgentInfo>;
            };
        };
    };
    '/api/ChatRoomClient/GetChannelChatHistory': {
        post: {
            req: PostApiChatRoomClientGetChannelChatHistoryData;
            res: {
                /**
                 * OK
                 */
                200: Array<ChatMsg>;
            };
        };
    };
    '/api/ChatRoomClient/CreateChannel': {
        post: {
            req: PostApiChatRoomClientCreateChannelData;
            res: {
                /**
                 * OK
                 */
                200: unknown;
            };
        };
    };
    '/api/ChatRoomClient/JoinChannel': {
        post: {
            req: PostApiChatRoomClientJoinChannelData;
            res: {
                /**
                 * OK
                 */
                200: unknown;
            };
        };
    };
    '/api/ChatRoomClient/LeaveChannel': {
        post: {
            req: PostApiChatRoomClientLeaveChannelData;
            res: {
                /**
                 * OK
                 */
                200: unknown;
            };
        };
    };
    '/api/ChatRoomClient/DeleteChannel': {
        post: {
            req: PostApiChatRoomClientDeleteChannelData;
            res: {
                /**
                 * OK
                 */
                200: unknown;
            };
        };
    };
    '/api/ChatRoomClient/AddAgentToChannel': {
        post: {
            req: PostApiChatRoomClientAddAgentToChannelData;
            res: {
                /**
                 * OK
                 */
                200: unknown;
            };
        };
    };
    '/api/ChatRoomClient/RemoveAgentFromChannel': {
        post: {
            req: PostApiChatRoomClientRemoveAgentFromChannelData;
            res: {
                /**
                 * OK
                 */
                200: unknown;
            };
        };
    };
    '/api/ChatRoomClient/GetOrchestrators': {
        get: {
            res: {
                /**
                 * OK
                 */
                200: Array<(string)>;
            };
        };
    };
    '/api/ChatRoomClient/AddOrchestratorToChannel': {
        post: {
            req: PostApiChatRoomClientAddOrchestratorToChannelData;
            res: {
                /**
                 * OK
                 */
                200: unknown;
            };
        };
    };
    '/api/ChatRoomClient/RemoveOrchestratorFromChannel': {
        post: {
            req: PostApiChatRoomClientRemoveOrchestratorFromChannelData;
            res: {
                /**
                 * OK
                 */
                200: unknown;
            };
        };
    };
};