using AutoGen.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace ChatRoom.Powershell;

internal class PowershellRunnerAgent : IAgent
{
    public PowershellRunnerAgent(string name)
    {
        Name = name;
    }
    public string Name { get; }

    public Task<IMessage> GenerateReplyAsync(IEnumerable<IMessage> messages, GenerateReplyOptions? options = null, CancellationToken cancellationToken = default)
    {
        // get the last message
        var lastMessage = messages
                .TakeLast(2)
                .Where(m => m.GetContent() is string content && content.Contains("```pwsh"))
                .LastOrDefault() ?? null;

        if (lastMessage is null)
        {
            return Task.FromResult<IMessage>(new TextMessage(Role.Assistant, "No powershell script found", from: this.Name));
        }

        // retrieve the powershell script from the last message
        // the script will be placed between ```pwsh and ``` so we need to extract it
        var content = lastMessage.GetContent();
        if (content is string contentString)
        {
            var script = contentString
                .Split("```pwsh")
                .Last()
                .Split("```")
                .First()
                .Trim();

            var powershell = PowerShell.Create().AddScript(script);
            powershell.Invoke();
            if (powershell.HadErrors)
            {
                var errorMessage = powershell.Streams.Error.Select(e => e.ToString()).Aggregate((a, b) => $"{a}\n{b}");
                errorMessage = @$"[ERROR]
{errorMessage}";
                return Task.FromResult<IMessage>(new TextMessage(Role.Assistant, errorMessage, from: this.Name));
            }
            else
            {
                var information = powershell.Streams.Information;
                string successMessage = "script run succeed without any output";
                if (information is not null && information.Any())
                {
                    successMessage = information.Select(e => e.ToString()).Aggregate((a, b) => $"{a}\n{b}");
                }
                successMessage = @$"[SUCCESS]
{successMessage}";

                return Task.FromResult<IMessage>(new TextMessage(Role.Assistant, successMessage, from: this.Name));
            }
        }
        else
        {
            throw new ArgumentException("Invalid message content");
        }
    }
}
