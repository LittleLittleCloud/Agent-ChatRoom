This example shows how to set up `AutoGen Chatroom` with Powershell helper agents.

> [!NOTE]
> You can find the complete configuration in [Powershell-Chatroom](https://github.com/LittleLittleCloud/Powershell-ChatRoom) repository.

![gif](https://github.com/LittleLittleCloud/Powershell-ChatRoom/blob/main/asset/switch-theme.gif?raw=true)



## About Chatroom.Powershell package
[Chatroom.Powershell](https://www.nuget.org/packages/ChatRoom.Powershell) is an agent package for `Agent Chatroom` that provides the following Powershell helper agents:
- `ps-gpt`: A Powershell agent that write powershell scripts to resolve your tasks.
- `ps-runner`: An agent that run powershell scripts from chat history.

## Getting Started

1. Clone the [Powershell-Chatroom](https://github.com/LittleLittleCloud/Powershell-ChatRoom) repository and navigate to the repository folder.

```bash
git clone https://github.com/LittleLittleCloud/Powershell-ChatRoom
cd Powershell-ChatRoom
```

2. Install the `ChatRoom.Client` and `ChatRoom.Powershell` tools by running `dotnet tool restore`.

```bash
dotnet tool restore
```

3. Set up OpenAI API key in [client.json](https://github.com/LittleLittleCloud/Powershell-ChatRoom/blob/main/client.json) and [powershell.json](https://github.com/LittleLittleCloud/Powershell-ChatRoom/blob/main/powershell.json)

4. Start the `ChatRoom.Client` with the following command:

```bash
dotnet chatroom -c client.json
```

You will see the following output similar to the following, then you can navigate to the web UI in your browser and start chatting with the agents.
```bash
web ui is available at: http://localhost:51234;https://localhost:51235
```