## Agent Chatroom
#### An extensible multi-agent platform built on top of AutoGen.Net, shadcn and Orleans.

[![NuGet version](https://badge.fury.io/nu/ChatRoom.SDK.svg)](https://badge.fury.io/nu/ChatRoom.Client) | [website](https://littlelittlecloud.github.io/Agent-ChatRoom/) | Join our [Discord](https://discord.gg/UYwcVfGaeq)

![Agent Chatroom](assets/agent-chatroom-switch-theme.gif)

## 🌟 Highlights
- **Multi-Agent WebUI**: Chat with multiple agents simultaneously in a web browser.
- **Server mode**: You can run `ChatRoom.Client` as a restful server. For more information, see [Server mode](#-server-mode).
- **Extensible**: Create your own agents and integrate them into the chatroom.

## 🚀 Quick Start
### Install the `ChatRoom.Client` from NuGet.
```bash
dotnet tool install -g ChatRoom.Client
```

### Choose from one of the available templates to get started.
```bash
chatroom list-templates # list all available templates
```

The command will list all available templates.

![list-templates](assets/list-templates.png)
### Create configuration from the selected template.

Instead of writing configuration from scratch, an easier route is to create a chatroom configuration from the selected template using the `create` command, which will not only create a configuration file but also generate a json schema file for this configuration to provide intellisense in your editor.

```bash
# Create 
chatroom create -t chatroom_openai -o chatroom_openai.json
```

![fill key](assets/fill-openai-key.png)

### Start the chatroom client with the OpenAI agent.

After filling in the OpenAI key in the configuration file, you can start the chatroom client with the following command.

```bash
# start chatroom server
chatroom run -c chatroom_openai.json
```

You will see the following output from chatroom client which indicates the web UI is available at `http://localhost:51237` and `https://localhost:51238`. You can navigate to the web UI in your browser and start chatting with the agents.

```bash
web ui is available at: http://localhost:51237;https://localhost:51238
```

## 📦 Pre-configured Chatrooms
We provide the following configuration to help you get started quickly. More configurations will be added in the future.
- [OpenAI-Chatroom](https://github.com/LittleLittleCloud/OpenAI-Chatroom): chat with OpenAI gpt.
- [Powershell-Chatroom](https://github.com/LittleLittleCloud/Powershell-ChatRoom): chat with `ps-gpt` and `ps-runner` to help you write and run PowerShell scripts.

## 🌐 Server mode
To start a restful backend from `ChatRoom.Client`, add a `server_config` section to the client configuration file.

> [!Note]
> You can find the complete configuration sample in [chatroom-client-server.json](configuration/chatroom-client-server.json)

```json
// file: chatroom-client-server.json
"server_config": {
    "environment": "Development", // one of Development, Staging, Production.
    "urls": "http://localhost:51234;https://localhost:51235"
}
```

Then, run the following command to start the server. And the server will start on the `http://localhost:51234` and `https://localhost:51235`.

```bash
chatroom -c chatroom-client-server.json
```

### Swagger UI for the server
When the environment is set to `Development`, the server will automatically start the Swagger UI at `/swagger` and `/swagger/index.html`. You can also access the complete swagger schema at `/swagger/v1/swagger.json`, or visit [swagger-schema.json](schema/chatroom_client_swagger_schema.json) in this repository.
