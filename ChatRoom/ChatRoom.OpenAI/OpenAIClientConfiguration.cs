using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Azure.Core.Pipeline;
using Json.Schema.Generation;

namespace ChatRoom.OpenAI;


[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LLMType
{
    [Description("Azure OpenAI")]
    AOAI = 0, // Azure OpenAI

    [Description("OpenAI")]
    OpenAI = 1, // OpenAI

    [Description("Third-party LLM provider, like ollama or lm studio")]
    ThirdParty = 2, // Third-party LLM provider
}

public class OpenAIClientConfiguration
{
    [Description("LLM provider, default is Azure OpenAI")]
    [JsonPropertyName("llm_type")]
    public LLMType LLMType { get; set; } = LLMType.AOAI;

    [Description("OpenAI API key, default is $env:OPENAI_API_KEY")]
    [JsonPropertyName("openai_api_key")]
    public string? OpenAIApiKey { get; set; } = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

    [Description("OpenAI model ID, default is gpt-3.5-turbo")]
    [JsonPropertyName("openai_model_id")]
    public string? OpenAIModelId { get; set; } = "gpt-3.5-turbo";

    [Description("Azure OpenAI endpoint, default is $env:AZURE_OPENAI_ENDPOINT")]
    [JsonPropertyName("azure_openai_endpoint")]
    public string? AzureOpenAIEndpoint { get; set; } = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");

    [Description("Azure OpenAI key, default is $env:AZURE_OPENAI_API_KEY")]
    [JsonPropertyName("azure_openai_key")]
    public string? AzureOpenAIKey { get; set; } = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");

    [Description("Azure OpenAI deploy name, default is $env:AZURE_OPENAI_DEPLOY_NAME")]
    [JsonPropertyName("azure_openai_deploy_name")]
    public string? AzureOpenAIDeployName { get; set; } = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOY_NAME");

    [Description("Third-party LLM provider endpoint, default is $env:THIRD_PARTY_LLM_ENDPOINT")]
    [JsonPropertyName("third_party_llm_endpoint")]
    public string? ThirdPartyLLMEndpoint { get; set; } = Environment.GetEnvironmentVariable("THIRD_PARTY_LLM_ENDPOINT");

    [Description("Third-party LLM provider key, default is $env:THIRD_PARTY_LLM_KEY")]
    [JsonPropertyName("third_party_llm_key")]
    public string? ThirdPartyLLMKey { get; set; } = Environment.GetEnvironmentVariable("THIRD_PARTY_LLM_KEY");

    [Description("Third-party LLM provider model ID, default is $env:THIRD_PARTY_LLM_MODEL_ID")]
    [JsonPropertyName("third_party_llm_model_id")]
    public string? ThirdPartyLLMModelId { get; set; } = Environment.GetEnvironmentVariable("THIRD_PARTY_LLM_MODEL_ID");

    public string? ModelId
    {
        get
        {
            return LLMType switch
            {
                LLMType.AOAI => AzureOpenAIDeployName,
                LLMType.OpenAI => OpenAIModelId,
                LLMType.ThirdParty => ThirdPartyLLMModelId,
                _ => null,
            };
        }
    }

    public OpenAIClient? ToOpenAIClient()
    {
        if (LLMType == LLMType.AOAI)
        {
            if (AzureOpenAIEndpoint is string
                && AzureOpenAIKey is string
                && AzureOpenAIDeployName is string)
            {
                return new OpenAIClient(new Uri(AzureOpenAIEndpoint), new Azure.AzureKeyCredential(AzureOpenAIKey));
            }
            else
            {
                return null;
            }
        }
        else if (LLMType == LLMType.OpenAI)
        {
            if (OpenAIApiKey is string && OpenAIModelId is string)
            {
                return new OpenAIClient(OpenAIApiKey);
            }
            else
            {
                return null;
            }
        }
        else
        {
            if (ThirdPartyLLMEndpoint is string
                && ThirdPartyLLMModelId is string)
            {
                var httpClient = new HttpClient()
                {
                    BaseAddress = new Uri(ThirdPartyLLMEndpoint),
                    DefaultRequestHeaders =
                    {
                        { "x-api-key", ThirdPartyLLMKey },
                        { "api-key", ThirdPartyLLMKey },
                        { "Authorization", $"Bearer {ThirdPartyLLMKey}" }
                    },
                };

                var openaiClientOption = new OpenAIClientOptions()
                {
                    Transport = new HttpClientTransport(httpClient)
                };

                return new OpenAIClient(ThirdPartyLLMModelId, openaiClientOption);
            }
            else
            {
                return null;
            }
        }
    }
}
