`Agent Chatroom` is published as dotnet tool on nuget.org.

> [!NOTE]
> To use `Agent Chatroom`, make sure you have [.NET 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) installed.

## Package overview
- [ChatRoom.Client](https://www.nuget.org/packages/ChatRoom.Client): The client for `Agent Chatroom`. It contains the multi-agent webui, chatroom server and an optional console ui for chatroom client.
- [ChatRoom.SDK](https://www.nuget.org/packages/ChatRoom.SDK): The SDK for developing agents and orchestrators for `Agent Chatroom`.

### Agent packages
- [ChatRoom.OpenAI](https://www.nuget.org/packages/ChatRoom.OpenAI): An agent package for `Agent Chatroom` that provides OpenAI agents.
- [ChatRoom.Powershell](https://www.nuget.org/packages/ChatRoom.Powershell): An agent package for `Agent Chatroom` that provides Powershell helper agents.
- [ChatRoom.BingSearch](https://www.nuget.org/packages/ChatRoom.BingSearch): An agent package for `Agent Chatroom` that provides Bing search agents.


## Install client
You can install the latest `Agent Chatroom` client from nuget.org by running the following command, this will install the `ChatRoom.Client` globally:
```bash
dotnet tool install --global ChatRoom.Client
```

Optionally, you can install the latest `Agent Chatroom` client locally by removing the `--global` flag:
```bash
dotnet tool install ChatRoom.Client
```

## Configure client
After installation, you need to create a configuration file for the `Agent Chatroom` client. The configuration file is a JSON file that contains the configuration for the client, including the agent packages and server configuration.

> [!Tip]
> You can find the schema for configuration files under the [schema](https://github.com/LittleLittleCloud/Agent-ChatRoom/tree/main/schema) folder in the `Agent Chatroom` repository.

Here is an example configuration file for the `Agent Chatroom` client, which starts webui on `http://localhost:51234` and `https://localhost:51235` and uses the `ChatRoom.OpenAI` agent package.

[!code-json[](../../configuration/chatroom-client.json)]

## Usage
After installation, you can start the `Agent Chatroom` client by running the following command:
```bash
chatroom -c /path/to/your/client_configuration.json
```

You will see the following output similar to the following, then you can navigate to the web UI in your browser and start chatting with the agents.
```bash
web ui is available at: http://localhost:51234;https://localhost:51235
```
