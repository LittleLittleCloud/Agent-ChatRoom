using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Json.Schema.Generation;

namespace ChatRoom.Powershell;

public enum LLMType
{
    [Description("Azure OpenAI")]
    AOAI = 0, // Azure OpenAI

    [Description("OpenAI")]
    OpenAI = 1 // OpenAI
}

public class PowershellGPTConfiguration
{
    [Description("Name of the powershell gpt agent, default is 'ps-gpt'")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = "ps-gpt";

    [Description("Current working directory, default is current directory of where the program is running")]
    public string CurrentWorkingDirectory { get; set; } = Directory.GetCurrentDirectory();

    [Description("System message, default is '$\"\"\"\r\n            You are a powershell developer. You need to convert the task assigned to you to a powershell script.\r\n            \r\n            If there is bug in the script, you need to fix it.\r\n\r\n            The current working directory is {cwd}\r\n\r\n            You need to write powershell script to resolve task. Put the script between ```pwsh and ```.\r\n            The script should always write the result to the output stream using Write-Host command.\r\n\r\n            e.g.\r\n            ```pwsh\r\n            # This is a powershell script\r\n            Write-Host \"Hello, World!\"\r\n            ```\r\n            \"\"\"'")]
    [JsonPropertyName("system_message")]
    public string SystemMessage { get; set; } = $"""
            You are a powershell developer. You need to convert the task assigned to you to a powershell script.
            
            If there is bug in the script, you need to fix it.

            The current working directory is {Directory.GetCurrentDirectory()}

            You need to write powershell script to resolve task. Put the script between ```pwsh and ```.
            The script should always write the result to the output stream using Write-Host command.

            e.g.
            ```pwsh
            # This is a powershell script
            Write-Host "Hello, World!"
            ```
            """;

    [Description("Agent description, default is 'I am PowerShell GPT, I am good at writing powershell scripts.'")]
    [JsonPropertyName("description")]
    public string Description { get; set; } = "I am PowerShell GPT, I am good at writing powershell scripts.";

    [Description("Language model type, will use Azure OpenAI if not provided")]
    [JsonPropertyName("llm_type")]
    public LLMType LLMType { get; set; } = LLMType.AOAI;

    [JsonPropertyName("azure_openai_endpoint")]
    [Description("Azure OpenAI endpoint, will use $env:AZURE_OPENAI_ENDPOINT if not provided")]
    public string? AzureOpenAiEndpoint { get; set; } = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");

    [JsonPropertyName("azure_openai_key")]
    [Description("Azure OpenAI key, will use $env:AZURE_OPENAI_API_KEY if not provided")]
    public string? AzureOpenAiKey { get; set; } = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");

    [JsonPropertyName("azure_deployment_name")]
    [Description("Azure OpenAI deployment name, will use $env:AZURE_OPENAI_DEPLOY_NAME if not provided")]
    public string? AzureDeploymentName { get; set; } = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOY_NAME");


    [JsonPropertyName("openai_api_key")]
    [Description("OpenAI API key, will use $env:OPENAI_API_KEY if not provided")]
    public string? OpenAiApiKey { get; set; } = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

    [JsonPropertyName("openai_model_id")]
    [Description("OpenAI model ID, will use gpt-3.5-turbo if not provided")]
    public string OpenAiModelId { get; set; } = "gpt-3.5-turbo";
}

public class PowershellRunnerConfiguration
{
    [JsonPropertyName("name")]
    [Description("Name of the powershell runner agent, default is 'ps-runner'")]
    public string Name { get; set; } = "ps-runner";

    [JsonPropertyName("description")]
    [Description("Agent description of the powershell runner agent, default is 'A powershell script runner'")]
    public string Description { get; set; } = "A powershell script runner";

    [JsonPropertyName("last_n_message")]
    [Description("Number of last message to look for powershell script, default is 10")]
    public int LastNMessage { get; set; } = 10;
}

public class PowershellConfiguration
{
    [JsonPropertyName("runner")]
    [Description("Powershell runner configuration")]
    public PowershellRunnerConfiguration Runner { get; set; } = new PowershellRunnerConfiguration();

    [JsonPropertyName("gpt")]
    [Description("Powershell GPT configuration")]
    public PowershellGPTConfiguration GPT { get; set; } = new PowershellGPTConfiguration();
}
