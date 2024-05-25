## Introduction

`ChatRoom.PowerShell` provides the following agents
- `ps-gpt`: A powershell gpt agent that can write powershell scripts based on given task. The powershell script block will be put between ```pwsh and ```.
- `ps-runner`: A powershell runner agent that can run powershell scripts and return the output to the user. It retrieves the most recent ```pwsh and ``` block from last n previous messages and run it using powershell.

## Installation

To install `ChatRoom.PowerShell` agent, run the following command:
```bash
dotnet tool install --global ChatRoom.PowerShell
```

## Configuration

You can configure `ChatRoom.PowerShell` agent by creating a configuration file and pass it to the agent. Below is an example of a configuration file for the agent:

[!code-json[](../../configuration/chatroom-powershell.json)]

## Usage

After creating the configuration and saving it to `chatroom-powershell.json`, you can start the agent with the following command:

> [!NOTE]
> Before starting the agent, make sure the `ChatRoom.Client` is running.

```bash
chatroom-powershell --room "room" --port 30000 --config chatroom-powershell.json
```
