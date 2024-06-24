using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using Azure.AI.OpenAI;

namespace weather_agent;

public partial class WeatherFunction
{
    [Function]
    public async Task<string> GetWeatherAsync(string city)
    {
        return await Task.FromResult($"The weather in {city} is sunny.");
    }
}

public static class WeatherAgentFactory
{
    public static IAgent CreateAgent(string name, string modelName)
    {
        var openAIApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? throw new Exception("OPENAI_API_KEY is not set.");
        var openAIClient = new OpenAIClient(openAIApiKey);
        var weatherFunction = new WeatherFunction();
        var functionMiddleware = new FunctionCallMiddleware(
            functions: [weatherFunction.GetWeatherAsyncFunctionContract],
            functionMap: new Dictionary<string, Func<string, Task<string>>>
            {
                { weatherFunction.GetWeatherAsyncFunctionContract.Name!, weatherFunction.GetWeatherAsyncWrapper }
            });

        return new OpenAIChatAgent(
            openAIClient: openAIClient,
            name: name,
            modelName: modelName)
            .RegisterMessageConnector()
            .RegisterMiddleware(functionMiddleware)
            .RegisterPrintMessage();
    }
}
