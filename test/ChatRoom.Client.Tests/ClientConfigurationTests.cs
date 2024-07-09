using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using ChatRoom.SDK;
using FluentAssertions;
using Json.Schema;
using Json.Schema.Generation;
using Xunit;

namespace ChatRoom.Client.Tests;

public class ClientConfigurationTests
{
    [Fact]
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovalTests")]
    public void VerifyConfigurationSchema()
    {
        var schema = new JsonSchemaBuilder()
            .FromType<ChatRoomClientConfiguration>()
            .Build();

        var json = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });

        var schemaFileName = "chatroom_configuration_schema.json";
        var schemaFilePath = Path.Join("template", "chatroom", schemaFileName);
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
        availableTemplates.Should().BeEquivalentTo(["chatroom_empty", "chatroom_openai", "chatroom_powershell", "chatroom_github", "chatroom_websearch", "chatroom_all_in_one"]);

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
