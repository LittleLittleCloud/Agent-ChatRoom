﻿using System;
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
