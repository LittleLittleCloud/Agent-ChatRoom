using System.ComponentModel;
using System.Text.Json;
using ChatRoom.SDK;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;
using Spectre.Console.Cli;
namespace ChatRoom.OpenAI;
internal class OpenAICommand : AsyncCommand<ChatRoomAgentClientCommandSettings>
{
    private IHost? _host = null;
    private bool _deployed = false;

    public static string Description { get; } = """
        OpenAI agent for chat room.

        The agent will use OpenAI API to generate response for the input message.

        To use the agent, you need to provide a configuration file.
        A configuration file is a json file with the following schema:
        - https://raw.githubusercontent.com/LittleLittleCloud/Agent-ChatRoom/main/schema/chatroom_openai_configuration_schema.json
        """;

    public override async Task<int> ExecuteAsync(CommandContext context, ChatRoomAgentClientCommandSettings setting)
    {
        var config = setting.ConfigFile is not null
            ? JsonSerializer.Deserialize<Configuration>(File.ReadAllText(setting.ConfigFile))!
            : new Configuration();

        return await ExecuteAsync(config);
    }

    internal IServiceProvider? ServiceProvider => _host?.Services;

    internal async Task StopAsync()
    {

       if (_host is not null)
        {
            await _host.StopAsync();
        }
    }

    internal async Task DeployAsync()
    {
        while (true)
        {
            if (_host is null)
            {
                await Task.Delay(1000);
                continue;
            }

            if (_deployed)
            {
                break;
            }

            await Task.Delay(1000);
        }
    }

    internal async Task<int> ExecuteAsync(Configuration config)
    {
        // verify if the name of agents is duplicated
        var agentNames = config.Agents.Select(agent => agent.Name);
        if (agentNames.Distinct().Count() != agentNames.Count())
        {
            AnsiConsole.MarkupLine("[red]The name of agents can't be duplicated.[/]");
            return 1;
        }

        _host = Host.CreateDefaultBuilder()
            .UseChatRoom(roomName: config.RoomConfig.Room ?? "room", port: config.RoomConfig.Port)
            .Build();
        await _host.StartAsync();
        var chatRoomClient = _host.Services.GetRequiredService<ChatPlatformClient>();

        foreach (var agentConfig in config.Agents)
        {
            var agentFactory = new OpenAIAgentFactory(agentConfig);

            await chatRoomClient.RegisterAutoGenAgentAsync(agentFactory.CreateAgent(), agentConfig.Description);
        }

        _deployed = true;
        await _host.WaitForShutdownAsync();

        return 0;
    }
}
