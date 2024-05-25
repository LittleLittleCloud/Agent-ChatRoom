You can install the latest chatroom client from nuget.org by running the following command:
```bash
dotnet tool install --global ChatRoom.Client
```

To start the chatroom client, run:
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