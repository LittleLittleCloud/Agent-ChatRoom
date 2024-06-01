using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using AutoGen.SemanticKernel;
using Azure.AI.OpenAI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;

namespace ChatRoom.BingSearch;

internal static class AgentFactory
{
    public static IAgent CreateBingSearchAgent(BingSearchConfiguration config)
    {
        var bingApiKey = config.BingApiKey;
        if (string.IsNullOrWhiteSpace(bingApiKey))
        {
            var defaultReply = "Bing API key not found. Please provide Bing API key in the configuration file or via env:BING_API_KEY";
            return new DefaultReplyAgent(config.Name, defaultReply);
        }

        var bingSearch = new BingConnector(bingApiKey);
        var webSearchPlugin = new WebSearchEnginePlugin(bingSearch);
        var kernel = new Kernel();
        var plugin = kernel.Plugins.AddFromObject(webSearchPlugin);
        var middleware = new KernelPluginMiddleware(kernel, plugin);
        OpenAIClient openaiClient;
        string deployModelName;
        if (config.LLMType == LLMType.AOAI)
        {
            var azureOpenAiEndpoint = config.AzureOpenAiEndpoint;
            var azureOpenAiKey = config.AzureOpenAiKey;
            var azureDeploymentName = config.AzureDeploymentName;

            if (string.IsNullOrWhiteSpace(azureOpenAiEndpoint) || string.IsNullOrWhiteSpace(azureOpenAiKey) || string.IsNullOrWhiteSpace(azureDeploymentName))
            {
                var defaultReply = "Azure OpenAI endpoint, key, or deployment name not found. Please provide Azure OpenAI endpoint, key, and deployment name in the configuration file or via env:AZURE_OPENAI_ENDPOINT, env:AZURE_OPENAI_API_KEY, env:AZURE_OPENAI_DEPLOY_NAME";
                return new DefaultReplyAgent(config.Name, defaultReply);
            }

            openaiClient = new OpenAIClient(new Uri(azureOpenAiEndpoint), new Azure.AzureKeyCredential(azureOpenAiKey));
            deployModelName = azureDeploymentName;
        }
        else if (config.LLMType == LLMType.OpenAI)
        {
            var openaiApiKey = config.OpenAiApiKey;
            var openaiModelId = config.OpenAiModelId;

            if (string.IsNullOrWhiteSpace(openaiApiKey))
            {
                var defaultReply = "OpenAI API key not found. Please provide OpenAI API key in the configuration file or via env:OPENAI_API_KEY";
                return new DefaultReplyAgent(config.Name, defaultReply);
            }

            openaiClient = new OpenAIClient(openaiApiKey);
            deployModelName = openaiModelId;
        }
        else
        {
            var defaultReply = "Language model type not found. Please provide language model type in the configuration file";

            return new DefaultReplyAgent(config.Name, defaultReply);
        }


        var agent = new OpenAIChatAgent(
            openAIClient: openaiClient,
            name: "bing-search",
            modelName: deployModelName,
            systemMessage: config.SystemMessage)
            .RegisterMessageConnector()
            .RegisterMiddleware(middleware)
            .RegisterMiddleware(async (msgs, option, innerAgent, ct) =>
            {
                try
                {
                    var reply = await innerAgent.GenerateReplyAsync(msgs, option, ct);
                    if (reply is ToolCallAggregateMessage)
                    {
                        return await innerAgent.GenerateReplyAsync(msgs.Append(reply), option, ct);
                    }

                    return reply;
                }
                catch (Exception ex)
                {
                    return new TextMessage(Role.Assistant, ex.Message);
                }
            })
            .RegisterPrintMessage();
            
        return agent;
    }
}
