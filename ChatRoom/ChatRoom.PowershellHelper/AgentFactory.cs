using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using Azure.AI.OpenAI;

namespace ChatRoom.PowershellHelper;

internal static class AgentFactory
{
    public static IAgent CreateManagerAgent(OpenAIClient client, string name = "Manager", string modelName = "gpt-35-turbo-0125")
    {
        var planner = new OpenAIChatAgent(
            openAIClient: client,
            name: name,
            modelName: modelName,
            systemMessage: """
            You are a manager of an software development team. If someone raise a powershell related question or task, you need to assign the task to the powershell developer in your team.
            Otherwise, you say "Not a powershell question".

            here's your team member:
            - powershell developer: A developer who is expert in powershell

            The task you create should be a json object with the following properties:
            - name: The name of the task
            - description: A description of the task
            - to: The member who you want to assign the task to. Can be empty if the task is not relevant to any of the team member

            here are some examples of tasks:
            {
                "name": "Not a powershell question",
                "description": "The question or task is not relevant to powershell",
                "to": null
            }
            {
                "name": "Shut down the server",
                "description": "Create a powershell script to shut down the server",
                "to": "powershell developer"
            }
            """,
            responseFormat: ChatCompletionsResponseFormat.JsonObject)
            .RegisterMessageConnector()
            .RegisterPrintMessage();

        return planner;
    }

    public static IAgent CreatePwshDeveloperAgent(
        OpenAIClient client,
        string cwd,
        string name = "powershell developer",
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
