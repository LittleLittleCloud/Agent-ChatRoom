using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using ApprovalTests;
using Json.Schema;
using Xunit;
using Json.Schema.Generation;

namespace ChatRoom.Powershell.Tests;

public class ConfigurationTest
{
    [Fact]
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovalTests")]
    public void VerifyConfigurationSchema()
    {
        var schema = new JsonSchemaBuilder()
            .FromType<PowershellConfiguration>()
            .Build();

        var json = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });

        Approvals.Verify(json);
    }
}
