This example shows how to set up `AutoGen Chatroom` with OpenAI agent.

> [!NOTE]
> You can find the complete configuration in [OpenAI-Chatroom](https://github.com/LittleLittleCloud/OpenAI-Chatroom) repository.

![gif](https://github.com/LittleLittleCloud/OpenAI-Chatroom/blob/main/assets/agent-chatroom-openai.gif?raw=true)


## Getting Started

1. Clone the [OpenAI-Chatroom](https://github.com/LittleLittleCloud/OpenAI-Chatroom) repository and navigate to the repository folder.

```bash
git clone https://github.com/LittleLittleCloud/OpenAI-Chatroom
cd OpenAI-Chatroom
```

2. Install the `ChatRoom.Client` and `ChatRoom.OpenAI` tools by running `dotnet tool restore`.

```bash
dotnet tool restore
```

3. Set up OpenAI API key in [client.json](https://github.com/LittleLittleCloud/OpenAI-Chatroom/blob/main/client.json) and [openai.json](https://github.com/LittleLittleCloud/OpenAI-Chatroom/blob/main/openai.json)

4. Start the `ChatRoom.Client` with the following command:

```bash
dotnet chatroom -c client.json
```

You will see the following output similar to the following, then you can navigate to the web UI in your browser and start chatting with the agents.
```bash
web ui is available at: http://localhost:51234;https://localhost:51235
```