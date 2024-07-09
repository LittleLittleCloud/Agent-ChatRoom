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
                .FromType<ChatRoomWebSearchConfiguration>()
                .Build();

            var schemaFileName = "chatroom_web_search_configuration_schema.json";
            var schemaFilePath = Path.Join("template", "chatroom.websearch", schemaFileName);
            var schemaFile = File.ReadAllText(schemaFilePath);

            var json = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });

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
            availableTemplates.Should().BeEquivalentTo(["chatroom-websearch"]);

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
}
