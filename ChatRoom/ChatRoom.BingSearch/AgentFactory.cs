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
    public static IAgent CreateBingSearchAgent()
    {
        var bingApiKey = Environment.GetEnvironmentVariable("BING_API_KEY") ?? throw new InvalidOperationException("BING_API_KEY environment variable is not set.");
        var bingSearch = new BingConnector(bingApiKey);
        var webSearchPlugin = new WebSearchEnginePlugin(bingSearch);
        var kernel = new Kernel();
        var plugin = kernel.Plugins.AddFromObject(webSearchPlugin);
        var middleware = new KernelPluginMiddleware(kernel, plugin);

        // create agents
        var AZURE_OPENAI_ENDPOINT = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
        var AZURE_OPENAI_KEY = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
        var AZURE_DEPLOYMENT_NAME = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOY_NAME");
        var OPENAI_API_KEY = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        var OPENAI_MODEL_ID = Environment.GetEnvironmentVariable("OPENAI_MODEL_ID") ?? "gpt-3.5-turbo-0125";

        OpenAIClient openaiClient;
        bool useAzure = false;
        if (AZURE_OPENAI_ENDPOINT is string && AZURE_OPENAI_KEY is string && AZURE_DEPLOYMENT_NAME is string)
        {
            openaiClient = new OpenAIClient(new Uri(AZURE_OPENAI_ENDPOINT), new Azure.AzureKeyCredential(AZURE_OPENAI_KEY));
            useAzure = true;
        }
        else if (OPENAI_API_KEY is string)
        {
            openaiClient = new OpenAIClient(OPENAI_API_KEY);
        }
        else
        {
            throw new ArgumentException("Please provide either (AZURE_OPENAI_ENDPOINT, AZURE_OPENAI_KEY, AZURE_DEPLOYMENT_NAME) or OPENAI_API_KEY");
        }

        var deployModelName = useAzure ? AZURE_DEPLOYMENT_NAME! : OPENAI_MODEL_ID;

        var agent = new OpenAIChatAgent(
            openAIClient: openaiClient,
            name: "bing-search",
            modelName: deployModelName,
            systemMessage: "You are a Bing search agent. You can search the web using Bing search engine.")
            .RegisterMessageConnector()
            .RegisterMiddleware(middleware)
            .RegisterMiddleware(async (msgs, option, innerAgent, ct) =>
            {
                try
                {
                    var reply = await innerAgent.GenerateReplyAsync(msgs, option, ct);
                    if (reply is AggregateMessage<ToolCallMessage, ToolCallResultMessage>)
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
