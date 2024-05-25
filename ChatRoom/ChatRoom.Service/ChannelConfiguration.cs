using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Json.Schema.Generation;

namespace ChatRoom.Room;

public class ChannelConfiguration
{
    [Description("Wheather to use AOAI or not. Default is true")]
    [JsonPropertyName("use_aoai")]
    public bool UseAOAI { get; set; } = true;

    [Description("The OpenAI API key. Default is the value of the env:OPENAI_API_KEY")]
    [JsonPropertyName("openai_api_key")]
    public string? OpenAIApiKey { get; set; } = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

    [Description("The OpenAI model id. Default is 'gpt-3.5-turbo'")]
    [JsonPropertyName("openai_model_id")]
    public string? OpenAIModelId { get; set; } = "gpt-3.5-turbo";

    [Description("The Azure OpenAI endpoint. Default is the value of the env:AZURE_OPENAI_ENDPOINT")]
    [JsonPropertyName("azure_openai_endpoint")]
    public string? AzureOpenAIEndpoint { get; set; } = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");

    [Description("The Azure OpenAI key. Default is the value of the env:AZURE_OPENAI_KEY")]
    [JsonPropertyName("azure_openai_key")]
    public string? AzureOpenAIKey { get; set; } = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY");

    [Description("The Azure OpenAI deploy name. Default is the value of the env:AZURE_DEPLOYMENT_NAME")]
    [JsonPropertyName("azure_openai_deploy_name")]
    public string? AzureOpenAIDeployName { get; set; } = Environment.GetEnvironmentVariable("AZURE_DEPLOYMENT_NAME");
}

public class RoomConfiguration
{
    [Description("The name of the room. Default is 'room'")]
    [JsonPropertyName("room")]
    public string Room { get; set; } = "room";

    [Description("The port number where the room is hosted. Default is 30000")]
    [JsonPropertyName("port")]
    public int Port { get; set; } = 30000;
}
