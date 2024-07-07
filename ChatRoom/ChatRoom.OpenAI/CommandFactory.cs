using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                .WithDescription(OpenAICommand.Description);

            config.AddCommand<OpenAICreateConfigurationFromTemplateCommand>("create")
                .WithDescription("""
                Create and save a configuration file from given template.
                The following templates are available:
                - chatroom-openai: Create a configuration file for chat room with OpenAI agents.
                """);
            config.AddExample(["run", "-c", "config.json"]);
            config.AddExample(["create", "--template", "chatroom-openai"]);
        });
        return app;
    }
}
