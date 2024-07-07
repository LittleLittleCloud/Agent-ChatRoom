using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using ApprovalTests;
using Json.Schema;
using Xunit;
using Json.Schema.Generation;
using FluentAssertions;

namespace ChatRoom.Powershell.Tests;

public class ConfigurationTest
{
    [Fact]
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovalTests")]
    public void VerifyConfigurationSchema()
    {
        var schema = new JsonSchemaBuilder()
            .FromType<PowershellConfiguration>()
            .Build();

        var json = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
        var schemaFileName = "chatroom_powershell_configuration_schema.json";
        var schemaFilePath = Path.Join("template", "chatroom.powershell", schemaFileName);
        var schemaFile = File.ReadAllText(schemaFilePath);

        Approvals.Verify(json);
        schemaFile.Should().BeEquivalentTo(json);

        var command = new CreateConfigurationCommand();
        var schemaContent = command.GetSchemaContent();
        schemaContent.Should().BeEquivalentTo(json);
    }


    [Fact]
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovalTests")]
    public void VerifyAvailableTemplates()
    {
        var command = new CreateConfigurationCommand();
        var availableTemplates = command.AvailableTemplates;
        availableTemplates.Should().BeEquivalentTo(["chatroom-powershell"]);

        var listTemplatesCommand = new ListTemplatesCommand();
        listTemplatesCommand.AvailableTemplates.Keys.Should().BeEquivalentTo(availableTemplates);

        var templates = new List<string>();
        foreach (var template in availableTemplates)
        {
            var templateContent = command.GetTemplateContent(template);
            templates.Add(templateContent);
        }

        Approvals.VerifyAll("templates", templates, "templates");
    }
}
