using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ApprovalTests;
using ApprovalTests.Approvers;
using ApprovalTests.Core;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using ChatRoom.OpenAI;
using FluentAssertions;
using Json.Schema;
using Json.Schema.Generation;
using Moq;
using Xunit;

namespace ChatRoom.OpenAI.Tests;

public class ClientConfigurationTests
{
    [Fact]
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovalTests")]
    public void VerifyConfigurationSchema()
    {
        var schema = new JsonSchemaBuilder()
            .FromType<OpenAIAgentConfiguration>()
            .Build();

        var schemaFileName = "chatroom_openai_configuration_schema.json";
        var schemaFilePath = Path.Join("Schema", schemaFileName);
        var schemaFile = File.ReadAllText(schemaFilePath);

        var json = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
        Approvals.Verify(json);
        schemaFile.Should().BeEquivalentTo(json);
    }
}
