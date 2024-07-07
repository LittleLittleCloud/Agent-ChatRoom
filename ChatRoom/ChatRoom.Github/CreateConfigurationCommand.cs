using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatRoom.SDK;

namespace ChatRoom.Github;

internal class CreateConfigurationCommand : CreateConfigurationFromTemplateCommand
{
    public CreateConfigurationCommand()
        : base("chatroom_github_configuration_schema.json", ["chatroom-github"])
    {
    }
}

internal class ListTemplatesCommand : ListAvailableTemplatesCommand
{
    public ListTemplatesCommand()
        : base(new Dictionary<string, string>
        {
            ["chatroom-github"] = "get-started template for chatroom github",
        })
    {
    }
}
