using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using ApprovalTests;
using ChatRoom.OpenAI;
using FluentAssertions;
using Json.Schema;
using Xunit;
using Json.Schema.Generation;

namespace ChatRoom.Planner.Tests;

public class ConfigurationTests
{
    [Fact]
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovalTests")]
    public void VerifyConfigurationSchema()
    {
        var schema = new JsonSchemaBuilder()
            .FromType<ChatRoomPlannerConfiguration>()
            .Build();
        var json = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
        Approvals.Verify(json);

        var schemaFileName = "chatroom_planner_configuration_schema.json";
        var schemaFilePath = Path.Join("template", "chatroom.planner", schemaFileName);
        var schemaFileContent = File.ReadAllText(schemaFilePath);
        var schemaFile = JsonSerializer.Deserialize<JsonSchema>(schemaFileContent);
        schemaFileContent = JsonSerializer.Serialize(schemaFile, new JsonSerializerOptions { WriteIndented = true });
        schemaFileContent.Should().BeEquivalentTo(json);

        var command = new CreateConfigurationCommand();
        schemaFileContent = command.GetSchemaContent();
        schemaFile = JsonSerializer.Deserialize<JsonSchema>(schemaFileContent);
        schemaFileContent = JsonSerializer.Serialize(schemaFile, new JsonSerializerOptions { WriteIndented = true });
        schemaFileContent.Should().BeEquivalentTo(json);
    }

    [Fact]
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovalTests")]
    public void VerifyAvailableTemplates()
    {
        var command = new CreateConfigurationCommand();
        var availableTemplates = command.AvailableTemplates;
        availableTemplates.Should().BeEquivalentTo(["chatroom-planner"]);

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
