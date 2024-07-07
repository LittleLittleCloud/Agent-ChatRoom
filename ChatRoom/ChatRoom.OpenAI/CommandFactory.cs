using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatRoom.SDK.Extension;
using Spectre.Console.Cli;

namespace ChatRoom.OpenAI;

internal class CommandFactory
{
    public static CommandApp Create()
    {
        var app = new CommandApp();

        app.Configure(config =>
        {
            config.AddCommand<OpenAICommand>("run")
                .WithDescription(OpenAICommand.Description)
                .WithExample(["run", "-c", "config.json"]);

            config.AddCommand<CreateConfigurationCommand>("create")
                .WithDescription("""
                Create and save a configuration file from given template.
                The following templates are available:
                - chatroom-openai: Create a configuration file for chat room with OpenAI agents.
                """)
                .WithExample(["create", "--template", "chatroom-openai"]);

            config.AddListTemplateCommand<ListTemplatesCommand>();
        });

        return app;
    }
}
