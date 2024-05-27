using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using Spectre.Console.Cli;
using Spectre.Console.Testing;
using Xunit;

namespace ChatRoom.OpenAI.Tests;

public class OpenAICommandTests
{
    [Fact]
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovalTests")]
    public async Task ItShowHelperTextTestAsync()
    {
        var app = new CommandAppTester();
        app.SetDefaultCommand<OpenAICommand>(OpenAICommand.Description);
        var result = await app.RunAsync("--help");

        Approvals.Verify(result.Output);
    }
}
