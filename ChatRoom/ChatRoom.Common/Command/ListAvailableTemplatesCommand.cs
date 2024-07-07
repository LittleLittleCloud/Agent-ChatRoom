using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ChatRoom.SDK;

internal abstract class ListAvailableTemplatesCommand : AsyncCommand
{
    private readonly Dictionary<string, string> _templates;

    public ListAvailableTemplatesCommand(Dictionary<string, string> templates) => _templates = templates;

    public override async Task<int> ExecuteAsync(CommandContext context)
    {
        AnsiConsole.MarkupLine("[yellow]Available templates:[/]");

        foreach (var (key, value) in _templates)
        {
            AnsiConsole.MarkupLine($"[yellow]{key}[/]: {value}");
        }

        return 0;
    }

    internal Dictionary<string, string> AvailableTemplates => _templates;
}
