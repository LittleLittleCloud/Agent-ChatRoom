using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using ApprovalTests;
using Spectre.Console.Testing;
using Xunit;

namespace ChatRoom.Client.Tests;

public class ChatRoomClientCommandTests
{
    [Fact]
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovalTests")]
    public async Task ItShowHelperTextTestAsync()
    {
        var app = new CommandAppTester();
        app.SetDefaultCommand<ChatRoomClientCommand>(ChatRoomClientCommand.Description);

        var result = await app.RunAsync("--help");

        Approvals.Verify(result.Output);
    }
}
