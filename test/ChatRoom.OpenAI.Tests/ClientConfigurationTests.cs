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
using ChatRoom.SDK;
using FluentAssertions;
using Json.Schema;
using Json.Schema.Generation;
using Moq;
using Xunit;

namespace ChatRoom.OpenAI.Tests;

public class ClientConfigurationTests
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };
    
    [Fact]
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovalTests")]
    public void VerifyConfigurationSchema()
    {
        var schema = new JsonSchemaBuilder()
            .FromType<Configuration>()
            .Build();

        var schemaFileName = "chatroom_openai_configuration_schema.json";
        var schemaFilePath = Path.Join("template", "chatroom.openai", schemaFileName);
        var schemaFile = File.ReadAllText(schemaFilePath);

        var json = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
        Approvals.Verify(json);
        schemaFile.Should().BeEquivalentTo(json);
    }

    [Fact]
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovalTests")]
    public void ConfigurationLoadTest()
    {
        var config = new OpenAIAgentConfiguration
        {
            LLMConfiguration = new OpenAIClientConfiguration
            {
                LLMType = LLMType.AOAI,
                OpenAIApiKey = "test",
                OpenAIModelId = "test",
                AzureOpenAIEndpoint = "test",
                AzureOpenAIKey = "test",
                AzureOpenAIDeployName = "test",
                ThirdPartyLLMEndpoint = "test",
                ThirdPartyLLMKey = "test"
            },
            SystemMessage = "test",
            Description = "test",
            Name = "test"
        };

        var json = JsonSerializer.Serialize(config, _jsonSerializerOptions);
        Approvals.Verify(json);
        config = JsonSerializer.Deserialize<OpenAIAgentConfiguration>(json)!;

        json.Should().BeEquivalentTo(JsonSerializer.Serialize(config, _jsonSerializerOptions));
    }
}
