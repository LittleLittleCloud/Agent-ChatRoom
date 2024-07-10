using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatRoom.SDK;

namespace ChatRoom.Client;

internal class CreateConfigurationCommand : CreateConfigurationFromTemplateCommand
{
    public CreateConfigurationCommand()
        : base(
            "chatroom_configuration_schema.json",
            [
                "chatroom_empty",
                "chatroom_openai",
                "chatroom_powershell",
                "chatroom_github",
                "chatroom_websearch",
                "chatroom_planner",
                "chatroom_all_in_one",
            ])
    {
    }
}

internal class ListTemplatesCommand : ListAvailableTemplatesCommand
{
    public ListTemplatesCommand()
        : base(new Dictionary<string, string>
        {
            ["chatroom_empty"] = "get start with empty chatroom",
            ["chatroom_openai"] = "get start with openai agents",
            ["chatroom_powershell"] = "get start with powershell gpt and runner",
            ["chatroom_github"] = "get start with github agents",
            ["chatroom_websearch"] = "get start with websearch agents",
            ["chatroom_planner"] = "get start with planner agents",
            ["chatroom_all_in_one"] = "get start with all available agents",
        })
    {
    }
}
