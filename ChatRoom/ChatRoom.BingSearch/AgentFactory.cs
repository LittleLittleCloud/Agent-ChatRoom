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
using ChatRoom.SDK.Extension;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web.Google;

namespace ChatRoom.WebSearch;

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
            name: config.Name,
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
            .RegisterPrintMessage()
            .ReturnErrorMessageWhenExceptionThrown();
            
        return agent;
    }

    public static IAgent CreateGoogleSearchAgent(GoogleSearchConfiguration config)
    {
        var googleApiKey = config.GoogleApiKey;
        if (string.IsNullOrWhiteSpace(googleApiKey))
        {
            var defaultReply = "Google API key not found. Please provide Google API key in the configuration file or via env:GOOGLE_API_KEY";
            return new DefaultReplyAgent(config.Name, defaultReply);
        }

        if (string.IsNullOrWhiteSpace(config.GoogleCustomSearchEngineId))
        {
            var defaultReply = "Google custom search engine id not found. Please provide Google custom search engine id in the configuration file or via env:GOOGLE_CUSTOM_SEARCH_ENGINE_ID";
            return new DefaultReplyAgent(config.Name, defaultReply);
        }

        var bingSearch = new GoogleConnector(googleApiKey, config.GoogleCustomSearchEngineId);
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
            name: config.Name,
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
