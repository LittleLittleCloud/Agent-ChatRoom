using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGen.Core;
using ChatRoom.SDK;
using FluentAssertions;
using Xunit;

namespace ChatRoom.OpenAI.Tests;

public class OpenAIAgentFactoryTests
{
    [Fact]
    public async Task ItReturnDefaultReplyAgentWhenConfigurationIsNotValid()
    {
        var configuration = new OpenAIAgentConfiguration();
        configuration.LLMConfiguration.AzureOpenAIKey = null;
        var agentFactory = new OpenAIAgentFactory(configuration);
        var agent = agentFactory.CreateAgent();
        agent.Should().BeOfType<DefaultReplyAgent>();
    }

    [Fact]
    public async Task ItReturnErrorMessageWhenMeetException()
    {
        var configuration = new OpenAIAgentConfiguration();
        configuration.LLMConfiguration.LLMType = LLMType.OpenAI;
        configuration.LLMConfiguration.OpenAIApiKey = "invalid key";
        var agentFactory = new OpenAIAgentFactory(configuration);
        var agent = agentFactory.CreateAgent();

        var reply = await agent.SendAsync("hey");
        // should get 401 error
        reply.GetContent().Should().Contain("401");
    }
}
