// This file is auto-generated by @hey-api/openapi-ts

import type { CancelablePromise } from './core/CancelablePromise';
import { OpenAPI } from './core/OpenAPI';
import { request as __request } from './core/request';
import type { PostApiChatRoomClientSendTextMessageToChannelData, PostApiChatRoomClientSendTextMessageToChannelResponse, GetApiChatRoomClientGetRoomCheckpointsResponse, GetApiChatRoomClientUnloadCheckpointResponse, GetApiChatRoomClientLoadCheckpointData, GetApiChatRoomClientLoadCheckpointResponse, GetApiChatRoomClientSaveCheckpointResponse, GetApiChatRoomClientDeleteCheckpointByCheckpointPathData, GetApiChatRoomClientDeleteCheckpointByCheckpointPathResponse, GetApiChatRoomClientGetChannelsResponse, GetApiChatRoomClientGetUserInfoResponse, GetApiChatRoomClientClearHistoryByChannelNameData, GetApiChatRoomClientClearHistoryByChannelNameResponse, GetApiChatRoomClientNewMessageSseByChannelNameData, GetApiChatRoomClientNewMessageSseByChannelNameResponse, PostApiChatRoomClientGenerateNextReplyData, PostApiChatRoomClientGenerateNextReplyResponse, GetApiChatRoomClientGetRoomMembersResponse, GetApiChatRoomClientGetChannelInfoByChannelNameData, GetApiChatRoomClientGetChannelInfoByChannelNameResponse, PostApiChatRoomClientEditTextMessageData, PostApiChatRoomClientEditTextMessageResponse, GetApiChatRoomClientDeleteMessageByChannelNameByMessageIdData, GetApiChatRoomClientDeleteMessageByChannelNameByMessageIdResponse, PostApiChatRoomClientGetChannelMembersData, PostApiChatRoomClientGetChannelMembersResponse, PostApiChatRoomClientGetChannelChatHistoryData, PostApiChatRoomClientGetChannelChatHistoryResponse, PostApiChatRoomClientCreateChannelData, PostApiChatRoomClientCreateChannelResponse, PostApiChatRoomClientJoinChannelData, PostApiChatRoomClientJoinChannelResponse, PostApiChatRoomClientLeaveChannelData, PostApiChatRoomClientLeaveChannelResponse, PostApiChatRoomClientDeleteChannelData, PostApiChatRoomClientDeleteChannelResponse, PostApiChatRoomClientAddAgentToChannelData, PostApiChatRoomClientAddAgentToChannelResponse, PostApiChatRoomClientRemoveAgentFromChannelData, PostApiChatRoomClientRemoveAgentFromChannelResponse, GetApiChatRoomClientGetOrchestratorsResponse, GetApiChatRoomClientGetChannelOrchestratorsData, GetApiChatRoomClientGetChannelOrchestratorsResponse, PostApiChatRoomClientAddOrchestratorToChannelData, PostApiChatRoomClientAddOrchestratorToChannelResponse, PostApiChatRoomClientRemoveOrchestratorFromChannelData, PostApiChatRoomClientRemoveOrchestratorFromChannelResponse, PostApiChatRoomClientCloneChannelData, PostApiChatRoomClientCloneChannelResponse, PostApiChatRoomClientEditChannelNameData, PostApiChatRoomClientEditChannelNameResponse } from './types.gen';

/**
 * @param data The data for the request.
 * @param data.requestBody
 * @returns unknown OK
 * @throws ApiError
 */
export const postApiChatRoomClientSendTextMessageToChannel = (data: PostApiChatRoomClientSendTextMessageToChannelData = {}): CancelablePromise<PostApiChatRoomClientSendTextMessageToChannelResponse> => { return __request(OpenAPI, {
    method: 'POST',
    url: '/api/ChatRoomClient/SendTextMessageToChannel',
    body: data.requestBody,
    mediaType: 'application/json'
}); };

/**
 * @returns string OK
 * @throws ApiError
 */
export const getApiChatRoomClientGetRoomCheckpoints = (): CancelablePromise<GetApiChatRoomClientGetRoomCheckpointsResponse> => { return __request(OpenAPI, {
    method: 'GET',
    url: '/api/ChatRoomClient/GetRoomCheckpoints'
}); };

/**
 * @returns unknown OK
 * @throws ApiError
 */
export const getApiChatRoomClientUnloadCheckpoint = (): CancelablePromise<GetApiChatRoomClientUnloadCheckpointResponse> => { return __request(OpenAPI, {
    method: 'GET',
    url: '/api/ChatRoomClient/UnloadCheckpoint'
}); };

/**
 * @param data The data for the request.
 * @param data.checkpointName
 * @returns unknown OK
 * @throws ApiError
 */
export const getApiChatRoomClientLoadCheckpoint = (data: GetApiChatRoomClientLoadCheckpointData = {}): CancelablePromise<GetApiChatRoomClientLoadCheckpointResponse> => { return __request(OpenAPI, {
    method: 'GET',
    url: '/api/ChatRoomClient/LoadCheckpoint',
    query: {
        checkpointName: data.checkpointName
    }
}); };

/**
 * @returns unknown OK
 * @throws ApiError
 */
export const getApiChatRoomClientSaveCheckpoint = (): CancelablePromise<GetApiChatRoomClientSaveCheckpointResponse> => { return __request(OpenAPI, {
    method: 'GET',
    url: '/api/ChatRoomClient/SaveCheckpoint'
}); };

/**
 * @param data The data for the request.
 * @param data.checkpointPath
 * @returns unknown OK
 * @throws ApiError
 */
export const getApiChatRoomClientDeleteCheckpointByCheckpointPath = (data: GetApiChatRoomClientDeleteCheckpointByCheckpointPathData): CancelablePromise<GetApiChatRoomClientDeleteCheckpointByCheckpointPathResponse> => { return __request(OpenAPI, {
    method: 'GET',
    url: '/api/ChatRoomClient/DeleteCheckpoint/{checkpointPath}',
    path: {
        checkpointPath: data.checkpointPath
    }
}); };

/**
 * @returns ChannelInfo OK
 * @throws ApiError
 */
export const getApiChatRoomClientGetChannels = (): CancelablePromise<GetApiChatRoomClientGetChannelsResponse> => { return __request(OpenAPI, {
    method: 'GET',
    url: '/api/ChatRoomClient/GetChannels'
}); };

/**
 * @returns AgentInfo OK
 * @throws ApiError
 */
export const getApiChatRoomClientGetUserInfo = (): CancelablePromise<GetApiChatRoomClientGetUserInfoResponse> => { return __request(OpenAPI, {
    method: 'GET',
    url: '/api/ChatRoomClient/GetUserInfo'
}); };

/**
 * @param data The data for the request.
 * @param data.channelName
 * @returns unknown OK
 * @throws ApiError
 */
export const getApiChatRoomClientClearHistoryByChannelName = (data: GetApiChatRoomClientClearHistoryByChannelNameData): CancelablePromise<GetApiChatRoomClientClearHistoryByChannelNameResponse> => { return __request(OpenAPI, {
    method: 'GET',
    url: '/api/ChatRoomClient/ClearHistory/{channelName}',
    path: {
        channelName: data.channelName
    }
}); };

/**
 * @param data The data for the request.
 * @param data.channelName
 * @returns unknown OK
 * @throws ApiError
 */
export const getApiChatRoomClientNewMessageSseByChannelName = (data: GetApiChatRoomClientNewMessageSseByChannelNameData): CancelablePromise<GetApiChatRoomClientNewMessageSseByChannelNameResponse> => { return __request(OpenAPI, {
    method: 'GET',
    url: '/api/ChatRoomClient/NewMessageSse/{channelName}',
    path: {
        channelName: data.channelName
    }
}); };

/**
 * @param data The data for the request.
 * @param data.requestBody
 * @returns GenerateNextReplyResponse OK
 * @throws ApiError
 */
export const postApiChatRoomClientGenerateNextReply = (data: PostApiChatRoomClientGenerateNextReplyData = {}): CancelablePromise<PostApiChatRoomClientGenerateNextReplyResponse> => { return __request(OpenAPI, {
    method: 'POST',
    url: '/api/ChatRoomClient/GenerateNextReply',
    body: data.requestBody,
    mediaType: 'application/json'
}); };

/**
 * @returns AgentInfo OK
 * @throws ApiError
 */
export const getApiChatRoomClientGetRoomMembers = (): CancelablePromise<GetApiChatRoomClientGetRoomMembersResponse> => { return __request(OpenAPI, {
    method: 'GET',
    url: '/api/ChatRoomClient/GetRoomMembers'
}); };

/**
 * @param data The data for the request.
 * @param data.channelName
 * @returns ChannelInfo OK
 * @throws ApiError
 */
export const getApiChatRoomClientGetChannelInfoByChannelName = (data: GetApiChatRoomClientGetChannelInfoByChannelNameData): CancelablePromise<GetApiChatRoomClientGetChannelInfoByChannelNameResponse> => { return __request(OpenAPI, {
    method: 'GET',
    url: '/api/ChatRoomClient/GetChannelInfo/{channelName}',
    path: {
        channelName: data.channelName
    }
}); };

/**
 * @param data The data for the request.
 * @param data.requestBody
 * @returns unknown OK
 * @throws ApiError
 */
export const postApiChatRoomClientEditTextMessage = (data: PostApiChatRoomClientEditTextMessageData = {}): CancelablePromise<PostApiChatRoomClientEditTextMessageResponse> => { return __request(OpenAPI, {
    method: 'POST',
    url: '/api/ChatRoomClient/EditTextMessage',
    body: data.requestBody,
    mediaType: 'application/json'
}); };

/**
 * @param data The data for the request.
 * @param data.channelName
 * @param data.messageId
 * @returns unknown OK
 * @throws ApiError
 */
export const getApiChatRoomClientDeleteMessageByChannelNameByMessageId = (data: GetApiChatRoomClientDeleteMessageByChannelNameByMessageIdData): CancelablePromise<GetApiChatRoomClientDeleteMessageByChannelNameByMessageIdResponse> => { return __request(OpenAPI, {
    method: 'GET',
    url: '/api/ChatRoomClient/DeleteMessage/{channelName}/{messageId}',
    path: {
        channelName: data.channelName,
        messageId: data.messageId
    }
}); };

/**
 * @param data The data for the request.
 * @param data.requestBody
 * @returns AgentInfo OK
 * @throws ApiError
 */
export const postApiChatRoomClientGetChannelMembers = (data: PostApiChatRoomClientGetChannelMembersData = {}): CancelablePromise<PostApiChatRoomClientGetChannelMembersResponse> => { return __request(OpenAPI, {
    method: 'POST',
    url: '/api/ChatRoomClient/GetChannelMembers',
    body: data.requestBody,
    mediaType: 'application/json'
}); };

/**
 * @param data The data for the request.
 * @param data.requestBody
 * @returns ChatMsg OK
 * @throws ApiError
 */
export const postApiChatRoomClientGetChannelChatHistory = (data: PostApiChatRoomClientGetChannelChatHistoryData = {}): CancelablePromise<PostApiChatRoomClientGetChannelChatHistoryResponse> => { return __request(OpenAPI, {
    method: 'POST',
    url: '/api/ChatRoomClient/GetChannelChatHistory',
    body: data.requestBody,
    mediaType: 'application/json'
}); };

/**
 * @param data The data for the request.
 * @param data.requestBody
 * @returns unknown OK
 * @throws ApiError
 */
export const postApiChatRoomClientCreateChannel = (data: PostApiChatRoomClientCreateChannelData = {}): CancelablePromise<PostApiChatRoomClientCreateChannelResponse> => { return __request(OpenAPI, {
    method: 'POST',
    url: '/api/ChatRoomClient/CreateChannel',
    body: data.requestBody,
    mediaType: 'application/json'
}); };

/**
 * @param data The data for the request.
 * @param data.requestBody
 * @returns unknown OK
 * @throws ApiError
 */
export const postApiChatRoomClientJoinChannel = (data: PostApiChatRoomClientJoinChannelData = {}): CancelablePromise<PostApiChatRoomClientJoinChannelResponse> => { return __request(OpenAPI, {
    method: 'POST',
    url: '/api/ChatRoomClient/JoinChannel',
    body: data.requestBody,
    mediaType: 'application/json'
}); };

/**
 * @param data The data for the request.
 * @param data.requestBody
 * @returns unknown OK
 * @throws ApiError
 */
export const postApiChatRoomClientLeaveChannel = (data: PostApiChatRoomClientLeaveChannelData = {}): CancelablePromise<PostApiChatRoomClientLeaveChannelResponse> => { return __request(OpenAPI, {
    method: 'POST',
    url: '/api/ChatRoomClient/LeaveChannel',
    body: data.requestBody,
    mediaType: 'application/json'
}); };

/**
 * @param data The data for the request.
 * @param data.requestBody
 * @returns unknown OK
 * @throws ApiError
 */
export const postApiChatRoomClientDeleteChannel = (data: PostApiChatRoomClientDeleteChannelData = {}): CancelablePromise<PostApiChatRoomClientDeleteChannelResponse> => { return __request(OpenAPI, {
    method: 'POST',
    url: '/api/ChatRoomClient/DeleteChannel',
    body: data.requestBody,
    mediaType: 'application/json'
}); };

/**
 * @param data The data for the request.
 * @param data.requestBody
 * @returns unknown OK
 * @throws ApiError
 */
export const postApiChatRoomClientAddAgentToChannel = (data: PostApiChatRoomClientAddAgentToChannelData = {}): CancelablePromise<PostApiChatRoomClientAddAgentToChannelResponse> => { return __request(OpenAPI, {
    method: 'POST',
    url: '/api/ChatRoomClient/AddAgentToChannel',
    body: data.requestBody,
    mediaType: 'application/json'
}); };

/**
 * @param data The data for the request.
 * @param data.requestBody
 * @returns unknown OK
 * @throws ApiError
 */
export const postApiChatRoomClientRemoveAgentFromChannel = (data: PostApiChatRoomClientRemoveAgentFromChannelData = {}): CancelablePromise<PostApiChatRoomClientRemoveAgentFromChannelResponse> => { return __request(OpenAPI, {
    method: 'POST',
    url: '/api/ChatRoomClient/RemoveAgentFromChannel',
    body: data.requestBody,
    mediaType: 'application/json'
}); };

/**
 * @returns string OK
 * @throws ApiError
 */
export const getApiChatRoomClientGetOrchestrators = (): CancelablePromise<GetApiChatRoomClientGetOrchestratorsResponse> => { return __request(OpenAPI, {
    method: 'GET',
    url: '/api/ChatRoomClient/GetOrchestrators'
}); };

/**
 * @param data The data for the request.
 * @param data.channel
 * @returns string OK
 * @throws ApiError
 */
export const getApiChatRoomClientGetChannelOrchestrators = (data: GetApiChatRoomClientGetChannelOrchestratorsData = {}): CancelablePromise<GetApiChatRoomClientGetChannelOrchestratorsResponse> => { return __request(OpenAPI, {
    method: 'GET',
    url: '/api/ChatRoomClient/GetChannelOrchestrators',
    query: {
        channel: data.channel
    }
}); };

/**
 * @param data The data for the request.
 * @param data.requestBody
 * @returns unknown OK
 * @throws ApiError
 */
export const postApiChatRoomClientAddOrchestratorToChannel = (data: PostApiChatRoomClientAddOrchestratorToChannelData = {}): CancelablePromise<PostApiChatRoomClientAddOrchestratorToChannelResponse> => { return __request(OpenAPI, {
    method: 'POST',
    url: '/api/ChatRoomClient/AddOrchestratorToChannel',
    body: data.requestBody,
    mediaType: 'application/json'
}); };

/**
 * @param data The data for the request.
 * @param data.requestBody
 * @returns unknown OK
 * @throws ApiError
 */
export const postApiChatRoomClientRemoveOrchestratorFromChannel = (data: PostApiChatRoomClientRemoveOrchestratorFromChannelData = {}): CancelablePromise<PostApiChatRoomClientRemoveOrchestratorFromChannelResponse> => { return __request(OpenAPI, {
    method: 'POST',
    url: '/api/ChatRoomClient/RemoveOrchestratorFromChannel',
    body: data.requestBody,
    mediaType: 'application/json'
}); };

/**
 * @param data The data for the request.
 * @param data.requestBody
 * @returns unknown OK
 * @throws ApiError
 */
export const postApiChatRoomClientCloneChannel = (data: PostApiChatRoomClientCloneChannelData = {}): CancelablePromise<PostApiChatRoomClientCloneChannelResponse> => { return __request(OpenAPI, {
    method: 'POST',
    url: '/api/ChatRoomClient/CloneChannel',
    body: data.requestBody,
    mediaType: 'application/json'
}); };

/**
 * @param data The data for the request.
 * @param data.requestBody
 * @returns unknown OK
 * @throws ApiError
 */
export const postApiChatRoomClientEditChannelName = (data: PostApiChatRoomClientEditChannelNameData = {}): CancelablePromise<PostApiChatRoomClientEditChannelNameResponse> => { return __request(OpenAPI, {
    method: 'POST',
    url: '/api/ChatRoomClient/EditChannelName',
    body: data.requestBody,
    mediaType: 'application/json'
}); };