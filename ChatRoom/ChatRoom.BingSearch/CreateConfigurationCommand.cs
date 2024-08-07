﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatRoom.SDK;

namespace ChatRoom.WebSearch;

internal class CreateConfigurationCommand : CreateConfigurationFromTemplateCommand
{
    public CreateConfigurationCommand()
        : base("chatroom_web_search_configuration_schema.json", ["chatroom-websearch"])
    {
    }
}

internal class ListTemplatesCommand : ListAvailableTemplatesCommand
{
    public ListTemplatesCommand()
        : base(new Dictionary<string, string>
        {
            ["chatroom-websearch"] = "get-started template for chatroom web search",
        })
    {
    }
}
