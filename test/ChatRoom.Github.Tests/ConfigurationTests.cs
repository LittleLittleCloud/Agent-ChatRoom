﻿using System;
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

namespace ChatRoom.Github.Tests;

public class ConfigurationTests
{
    [Fact]
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovalTests")]
    public void VerifyConfigurationSchema()
    {
        var schema = new JsonSchemaBuilder()
            .FromType<ChatRoomGithubConfiguration>()
            .Build();
        var json = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
        Approvals.Verify(json);

        var schemaFileName = "chatroom_github_configuration_schema.json";
        var schemaFilePath = Path.Join("template", "chatroom.github", schemaFileName);
        var schemaFile = File.ReadAllText(schemaFilePath);
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
        availableTemplates.Should().BeEquivalentTo(["chatroom-github"]);

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
