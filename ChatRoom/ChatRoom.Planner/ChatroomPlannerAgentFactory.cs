using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using ChatRoom.SDK.Extension;

namespace ChatRoom.Planner;

internal class ChatroomPlannerAgentFactory
{
    public static IAgent CreateReactPlanner(ReActPlannerConfiguration configuration)
    {
        var openaiClient = configuration.OpenAIConfiguration.ToOpenAIClient();
        var modelName = configuration.OpenAIConfiguration.ModelId;

        if (openaiClient is null || modelName is null)
        {
            return new DefaultReplyAgent(
                configuration.Name,
                $"{configuration.Name} is not configured properly. Please check the configuration file.");
        }

        var openaiAgent = new OpenAIChatAgent(
            openaiClient.GetChatClient(modelName),
            configuration.Name,
            configuration.SystemMessage)
            .RegisterMessageConnector()
            .RegisterPrintMessage()
            .ReturnErrorMessageWhenExceptionThrown();

        return new ReActPlanner(openaiAgent);
    }
}
