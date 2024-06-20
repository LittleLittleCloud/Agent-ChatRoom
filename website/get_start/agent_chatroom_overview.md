Agent Chatroom is a multi-agent platform built on top of AutoGen.Net, shadcn and Orleans. It comes with a webui that allows you to chat with multiple agents simultaneously in a web browser. You can also run `ChatRoom.Client` as a restful server. Agent Chatroom is extensible, allowing you to create your own agents and orchestrators and integrate them into the chatroom.

![switch-theme](../assets/agent-chatroom-switch-theme.gif)

## Key Concept
### Agent
In Agent Chatroom, an agent is a conversational agent that can send and receive messages. An agent can be a human (e.g. You) or an LLM chatbot. You can add agents to the room by installing agent packages (e.g `ChatRoom.OpenAI`) or by developing your own agents using `ChatRoom.SDK`.

## Channel
A channel is simply a "group chat" for agents to communicate with. Each agent can send and receive messages as long as they are in the same channel. In Agent Chatroom, you chat with multiple agents by creating a channel and adding agents to it.

## Channel orchestrator
A channel orchestrator manages how agents interact with each other in a channel. For example, a `RoundRobinOrchestrator` will send messages to agents in a round-robin fashion.

Agent Chatroom provides the following channel orchestrators:
- `RoundRobinOrchestrator`: Sends messages to agents in a round-robin fashion.
- `DynamicGroupChat`: The dynamic group chat orchestrator from AutoGen.Net dynamically orchestrate conversation among agents based on conversation history.
- `Human-Bot`: An orchestrator that drives conversation between human and bot agents in an alternating fashion.

You can also create your own orchestrators using `ChatRoom.SDK`.

## Room
A room is a collection of channels.