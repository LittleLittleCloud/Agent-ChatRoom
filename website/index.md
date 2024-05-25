## Agent Chatroom: An extensible multi-agent platform built on top of AutoGen.Net and Orleans.

---

## 🌟 Highlights
- **Multi-Agent Chat**: Chat with multiple agents simultaneously.
- **Extensible**: Create your own agents and integrate them into the chatroom.

---

## 🚀 Quick Start

- 🛠️ Install the Client
To install the client, run the following command:
```bash
dotnet tool install --global ChatRoom.Client
```

- 🧩 Install the Agent
To install the OpenAI agent, run the following command:
```bash
dotnet tool install --global ChatRoom.OpenAI
```

> [!Note]
> As of 2024/05/24, the following agents are available as dotnet tools from NuGet:
> - `ChatRoom.OpenAI`: OpenAI agent.
> - `ChatRoom.Powershell`: Powershell GPT agent and Powershell executor agent.
> - `ChatRoom.BingSearch`: Bing search agent.

You can search for and install these agents from [nuget.org](https://www.nuget.org/).

- 🚪 Start the Chatroom
To start the chatroom service as an Orleans silo, run:
```bash
chatroom
```

- 🤖 Start the OpenAI Agent and Join the Chatroom
To start the OpenAI agent, run:
```bash
chatroom-openai
```

After the OpenAI agent is started, you will see the following message in the chatroom:
```bash
gpt joined the chatroom.
```

- 💬 Add the OpenAI Agent to the Current Channel and Start Chatting
Once the GPT agent is in the chatroom, you can add it to the current channel and start chatting with it using the following command:
```bash
/am gpt
Hey, tell me a joke.
```