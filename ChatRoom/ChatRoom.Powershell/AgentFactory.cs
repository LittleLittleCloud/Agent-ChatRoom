using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using Azure.AI.OpenAI;

namespace ChatRoom.Powershell;

internal static class AgentFactory
{
    public static IAgent CreatePwshDeveloperAgent(
        OpenAIClient client,
        string cwd,
        string name = "powershell",
        string modelName = "gpt-35-turbo-0125")
    {
        var agent = new OpenAIChatAgent(
            openAIClient: client,
            modelName: modelName,
            name: name,
            systemMessage: $"""
            You are a powershell developer. You need to convert the task assigned to you to a powershell script.
            
            If there is bug in the script, you need to fix it.

            The current working directory is {cwd}

            You need to write powershell script to resolve task. Put the script between ```pwsh and ```.
            The script should always write the result to the output stream using Write-Host command.

            e.g.
            ```pwsh
            # This is a powershell script
            Write-Host "Hello, World!"
            ```
            """)
            .RegisterMessageConnector()
            .RegisterPrintMessage();

        return agent;
    }
}
