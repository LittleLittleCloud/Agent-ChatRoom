using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ChatRoom.SDK;

internal class TemplateCommandSettings : CommandSettings
{
    [Description("the name of the configuration file to be created. Default is config.json")]
    [CommandOption("-o|--output <OUTPUT>")]
    public string Output { get; set; } = "config.json";

    [CommandOption("-t|--template <TEMPLATE>")]
    public string? Template { get; init; }
}

internal abstract class CreateConfigurationFromTemplateCommand : AsyncCommand<TemplateCommandSettings>
{
    private readonly string[] availableTemplates = ["chatroom-openai"];
    private readonly string schemaPath;

    public CreateConfigurationFromTemplateCommand(string schemaPath, string[] availableTemplates)
    {
        this.schemaPath = schemaPath;
        this.availableTemplates = availableTemplates;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, TemplateCommandSettings settings)
    {
        if (settings.Template is null)
        {
            AnsiConsole.MarkupLine("[red]Error:[/] Template is required.");
            return 1;
        }

        if (!availableTemplates.Contains(settings.Template))
        {
            AnsiConsole.MarkupLine("[red]Error:[/] Template not found.");
            return 1;
        }

        var template = await GetEmbeddedResourceContentAsync($"{settings.Template}.json");
        var schema = await GetEmbeddedResourceContentAsync(schemaPath);

        // save the template to the output file
        await File.WriteAllTextAsync(settings.Output, template);

        // save the schema to the output file
        await File.WriteAllTextAsync(schemaPath, schema);

        return 0;
    }

    internal async Task<string> GetEmbeddedResourceContentAsync(string resource)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(str => str.EndsWith(resource));
        if (resourceName is null)
        {
            throw new ArgumentException($"Resource {resource} not found");
        }

        await using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            throw new ArgumentException($"Resource {resource} not found");
        }

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}
