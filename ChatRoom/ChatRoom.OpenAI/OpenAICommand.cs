using System.ComponentModel;
using System.Text.Json;
using ChatRoom.SDK;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;
namespace ChatRoom.OpenAI;

public class OpenAICommandSettings : ChatRoomAgentClientCommandSettings
{
    [Description("Configuration file, schema: https://raw.githubusercontent.com/LittleLittleCloud/Agent-ChatRoom/main/schema/chatroom_openai_configuration_schema.json")]
    public override string? ConfigFile { get; init; }
}
public class OpenAICommand : AsyncCommand<OpenAICommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, OpenAICommandSettings setting)
    {
        var config = setting.ConfigFile is not null
            ? JsonSerializer.Deserialize<OpenAIAgentConfiguration>(File.ReadAllText(setting.ConfigFile))!
            : new OpenAIAgentConfiguration();

        var agentFactory = new OpenAIAgentFactory(config);

        var host = Host.CreateDefaultBuilder()
            .AddAgentAsync(async (_) => agentFactory.CreateAgent(), config.Description)
            .UseChatRoom(roomName: setting.Room ?? "room", port: setting.Port ?? 30000)
            .Build();

        await host.StartAsync();
        await host.WaitForAgentsJoinRoomAsync();
        await host.WaitForShutdownAsync();

        return 0;
    }
}
