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
        OpenAIClient? openaiClient = config.OpenAIConfiguration?.ToOpenAIClient();
        string? deployModelName = config.OpenAIConfiguration?.ModelId;
        
        if (openaiClient is null || deployModelName is null)
        {
            return new DefaultReplyAgent(
                config.Name,
                $"{config.Name} is not configured properly. Please check the configuration file.");
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
