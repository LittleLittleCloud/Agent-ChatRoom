using System.Text.Json;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using FluentAssertions;
using Json.Schema;
using Json.Schema.Generation;
using Xunit;

namespace ChatRoom.WebSearch.Tests
{
    public class ConfigurationTests
    {
        [Fact]
        [UseReporter(typeof(DiffReporter))]
        [UseApprovalSubdirectory("ApprovalTests")]
        public void VerifyConfigurationSchema()
        {
            var schema = new JsonSchemaBuilder()
                .FromType<WebSearchConfiguration>()
                .Build();

            var schemaFileName = "chatroom_web_search_configuration_schema.json";
            var schemaFilePath = Path.Join("template", "chatroom.websearch", schemaFileName);
            var schemaFile = File.ReadAllText(schemaFilePath);

            var json = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });

            Approvals.Verify(json);
            schemaFile.Should().BeEquivalentTo(json);
        }
    }
}
