`Agent Chatroom` is published as dotnet tool on nuget.org.

> [!NOTE]
> To use `Agent Chatroom`, make sure you have [.NET 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) installed.

## Package overview
- [ChatRoom.Client](https://www.nuget.org/packages/ChatRoom.Client): The **all-in-one** package for `Agent Chatroom`. It contains the multi-agent webui, every agent packages, and an optional console ui for chatroom client.
- [ChatRoom.SDK](https://www.nuget.org/packages/ChatRoom.SDK): The SDK for developing agents and orchestrators for `Agent Chatroom`.

### Agent packages
- [ChatRoom.OpenAI](https://www.nuget.org/packages/ChatRoom.OpenAI): An agent package for `Agent Chatroom` that provides OpenAI agents.
- [ChatRoom.Powershell](https://www.nuget.org/packages/ChatRoom.Powershell): An agent package for `Agent Chatroom` that provides Powershell helper agents.
- [ChatRoom.WebSearch](https://www.nuget.org/packages/ChatRoom.WebSearch): An agent package for `Agent Chatroom` that provides Bing search agent and Google search agent.
- [ChatRoom.Github](https://www.nuget.org/packages/ChatRoom.Github): An agent package for `Agent Chatroom` that provides Github issue helper agents.
- [ChatRoom.Planner](https://www.nuget.org/packages/ChatRoom.Planner): An agent package for `Agent Chatroom` that provides a react planner agent and corresponding orchestrator for multi-agent groups.


## Install client
You can install the latest `Agent Chatroom` client from nuget.org by running the following command, this will install the `ChatRoom.Client` globally:
```bash
dotnet tool install --global ChatRoom.Client
```