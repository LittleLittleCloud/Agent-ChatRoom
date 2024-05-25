Once you [configure the client](./configure_client.md), you can start chatting with the agents in the chatroom.

To start the chatroom client, run the following command:

```bash
chatroom
```

After the chatroom client is started, you will see the following output from the console:

```bash
     _                             _
    / \      __ _    ___   _ __   | |_
   / _ \    / _` |  / _ \ | '_ \  | __|
  / ___ \  | (_| | |  __/ | | | | | |_
 /_/   \_\  \__, |  \___| |_| |_|  \__|
            |___/
   ____   _               _     ____
  / ___| | |__     __ _  | |_  |  _ \    ___     ___    _ __ ___
 | |     | '_ \   / _` | | __| | |_) |  / _ \   / _ \  | '_ ` _ \
 | |___  | | | | | (_| | | |_  |  _ <  | (_) | | (_) | | | | | | |
  \____| |_| |_|  \__,_|  \__| |_| \_\  \___/   \___/  |_| |_| |_|



/j <channel> to join a specific channel
/l to leave the current channel
/h to re-read channel history
/m to query members in the current channel
/lm to query members in the room
/lc to query all channels in the room
/rc <channel> to remove channel from the room
/am <member> to add member to the current channel
/rm <member> to remove member from the current channel
/exit to exit
<message> to send a message
```

Then you can use `/lm` to list all the agents in the chatroom. For how to install agents, please refer to the [install agents](./install_agent.md) page.

```bash
/lm
────────────────────────────────────────────────── Members for 'room' ──────────────────────────────────────────────────
User - Human user
gpt - You are a helpful AI assistant
────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
```

To chat with an agent, you need to add the agent to the current channel first. Channel is similar to a group chat in a message app where you can chat with multiple agents simultaneously. To add an agent to the current channel, use the `/am` command followed by the agent name. For example, to add the GPT agent to the current channel, use the following command.

```bash
/am gpt
gpt joins the General channel.
```

After the agent is added to the channel, you can start chatting with the agent.

```bash
Tell me a joke
[5/24/2024 10:21:08 PM] User: Tell me a joke
[5/24/2024 10:21:09 PM] gpt: Sure, here's a joke for you:

Why couldn't the bicycle stand up by itself?

Because it was two-tired!
```
