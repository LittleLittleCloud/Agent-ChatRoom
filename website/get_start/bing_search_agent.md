## Introduction
`ChatRoom.BingSearch` agent is a chatbot agent that provides Bing search capability. The agent uses the Bing Search API to search for the user's query and returns the search results to the user.

## Installation
To install `ChatRoom.BingSearch` agent, run the following command:
```bash
dotnet tool install --global ChatRoom.BingSearch
```

## Configuration
You can configure `ChatRoom.BingSearch` agent by creating a configuration file and pass it to the agent. Below is an example of a configuration file for the agent:

> [!NOTE]
> You need to create a Bing Search API key for bing search capability. You can refer to the [Bing Search API](https://www.microsoft.com/en-us/bing/apis/bing-web-search-api) for more information.

[!code-json[](../../configuration/chatroom-bingsearch.json)]

## Usage
After creating the configuration and saving it to `chatroom-bingsearch.json`, you can start the agent with the following command:

> [!NOTE]
> Before starting the agent, make sure the `ChatRoom.Client` is running.

```bash
chatroom-bing-search --room "room" --port 30000 --config chatroom-bingsearch.json
```
