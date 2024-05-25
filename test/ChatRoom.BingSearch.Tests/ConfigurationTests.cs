using System.Text.Json;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using Json.Schema;
using Json.Schema.Generation;
using Xunit;

namespace ChatRoom.BingSearch.Tests
{
    public class ConfigurationTests
    {
        [Fact]
        [UseReporter(typeof(DiffReporter))]
        [UseApprovalSubdirectory("ApprovalTests")]
        public void VerifyConfigurationSchema()
        {
            var schema = new JsonSchemaBuilder()
                .FromType<BingSearchConfiguration>()
                .Build();

            var json = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });

            Approvals.Verify(json);
        }
    }
}
