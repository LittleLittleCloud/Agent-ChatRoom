// This file is auto-generated by @hey-api/openapi-ts

import type { CancelablePromise } from './core/CancelablePromise';
import { OpenAPI } from './core/OpenAPI';
import { request as __request } from './core/request';
import type { PostApiChatRoomClientSendTextMessageToChannelData, PostApiChatRoomClientSendTextMessageToChannelResponse, GetApiChatRoomClientGetChannelsResponse, GetApiChatRoomClientGetUserInfoResponse, GetApiChatRoomClientGetRoomMembersResponse, PostApiChatRoomClientGetChannelMembersData, PostApiChatRoomClientGetChannelMembersResponse, PostApiChatRoomClientGetChannelChatHistoryData, PostApiChatRoomClientGetChannelChatHistoryResponse, PostApiChatRoomClientCreateChannelData, PostApiChatRoomClientCreateChannelResponse, PostApiChatRoomClientJoinChannelData, PostApiChatRoomClientJoinChannelResponse, PostApiChatRoomClientLeaveChannelData, PostApiChatRoomClientLeaveChannelResponse, PostApiChatRoomClientDeleteChannelData, PostApiChatRoomClientDeleteChannelResponse, PostApiChatRoomClientAddAgentToChannelData, PostApiChatRoomClientAddAgentToChannelResponse, PostApiChatRoomClientRemoveAgentFromChannelData, PostApiChatRoomClientRemoveAgentFromChannelResponse } from './types.gen';

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
 * @returns AgentInfo OK
 * @throws ApiError
 */
export const getApiChatRoomClientGetRoomMembers = (): CancelablePromise<GetApiChatRoomClientGetRoomMembersResponse> => { return __request(OpenAPI, {
    method: 'GET',
    url: '/api/ChatRoomClient/GetRoomMembers'
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