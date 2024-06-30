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
            .FromType<ChatRoomServerConfiguration>()
            .Build();

        var json = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });

        var schemaFileName = "client_configuration_schema.json";
        var schemaFilePath = Path.Join("Schema", schemaFileName);
        var schemaFile = File.ReadAllText(schemaFilePath);

        Approvals.Verify(json);
        schemaFile.Should().BeEquivalentTo(json);
    }
}
