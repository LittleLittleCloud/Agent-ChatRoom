using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatRoom.SDK;
using Spectre.Console.Cli;

namespace ChatRoom.Powershell;

internal class CreateConfigurationCommand : CreateConfigurationFromTemplateCommand
{
    public CreateConfigurationCommand()
        :base("chatroom_powershell_configuration_schema.json", ["chatroom-powershell"])
    {
    }
}
