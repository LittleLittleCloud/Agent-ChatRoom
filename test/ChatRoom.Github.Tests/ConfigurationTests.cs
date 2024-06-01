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

namespace ChatRoom.Github.Tests;

public class ConfigurationTests
{
    [Fact]
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovalTests")]
    public void VerifyConfigurationSchema()
    {
        var schema = new JsonSchemaBuilder()
            .FromType<GithubConfiguration>()
            .Build();
        var json = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
        Approvals.Verify(json);

        var schemaFileName = "chatroom_github_configuration_schema.json";
        var schemaFilePath = Path.Join("Schema", schemaFileName);
        var schemaFile = File.ReadAllText(schemaFilePath);
        schemaFile.Should().BeEquivalentTo(json);
    }
}
