using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using ChatRoom.Github;
using ChatRoom.SDK;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Octokit;
using OpenAI;

var roomConfig = new RoomConfiguration
{
    Room = "room",
    Port = 30000,
};

var serverConfig = new ChatRoomServerConfiguration
{
    RoomConfig = roomConfig,
    YourName = "User",
    ServerConfig = new ServerConfiguration
    {
        Urls = "http://localhost:50001",
    },
};

using var host = Host.CreateDefaultBuilder()
    .UseChatRoomServer(serverConfig)
    .Build();

await host.StartAsync();
var client = host.Services.GetRequiredService<ChatPlatformClient>();

var openAIApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? throw new Exception("OPENAI_API_KEY is not set.");
var openAIClient = new OpenAIClient(openAIApiKey);
var chatClient = openAIClient.GetChatClient("gpt-4o-mini");
var ghClient = new GitHubClient(new ProductHeaderValue("ChatRoom"));
var repoOwner = "LittleLittleCloud";
var repoName = "Agent-ChatRoom";

var issueHelper = GithubAgentFactory.CreateIssueHelperAgent(chatClient, ghClient, repoOwner, repoName);

var gpt4oWriter = new OpenAIChatAgent(
    chatClient: chatClient,
    name: "release-note-writer",
    systemMessage: """
    You write release notes based on github issues.

    The release note should include the following sections:
    - New Features
    - Bug Fixes
    - Improvements
    - Documentation
    """)
    .RegisterMessageConnector()
    .RegisterPrintMessage();

var groupChatAdmin = new OpenAIChatAgent(
    chatClient: openAIClient.GetChatClient("gpt-4o-mini"),
    name: "admin")
    .RegisterMessageConnector()
    .RegisterPrintMessage();


// the user agent's name must match with ChatRoomServerConfiguration.YourName field.
// When chatroom starts, it will be replaced by a built-in user agent.
var userAgent = new DefaultReplyAgent("User", "<dummy>");

var groupChat = new GroupChat([userAgent, issueHelper, gpt4oWriter], admin: groupChatAdmin);

// add groupchat to chatroom
await client.RegisterAutoGenGroupChatAsync("release-room", groupChat);

// a web ui will start at http://localhost:50001
await host.WaitForShutdownAsync();
