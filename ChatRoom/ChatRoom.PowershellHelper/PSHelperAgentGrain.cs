using AutoGen.Core;
using Azure.AI.OpenAI;

namespace ChatRoom.PowershellHelper;

[GenerateSerializer]
public class PSHelperAgentGrain : Grain, IAgentGrain
{
    private IAgent? _psHelper;
    private string _name = "powershell";

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
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
        _psHelper = AgentFactory.CreatePwshDeveloperAgent(openaiClient, Environment.CurrentDirectory, name: _name, modelName: deployModelName);

        return base.OnActivateAsync(cancellationToken);
    }

    public Task<IMessage> GenerateReplyAsync(IEnumerable<IMessage> messages, GenerateReplyOptions? options = null, CancellationToken cancellationToken = default)
    {
        return _psHelper?.GenerateReplyAsync(messages, options, cancellationToken) ?? throw new InvalidOperationException("Agent is not initialized.");
    }

    public async Task<ChatMsg> GenerateReplyAsync(ChatMsg[] messages)
    {
        // convert ChatMsg to TextMessage
        var textMessages = messages.Select(msg => new TextMessage(Role.Assistant, msg.Text, from: msg.From)).ToArray();
        var reply = await _psHelper!.GenerateReplyAsync(textMessages);

        if (reply.GetContent() is string content)
        {
            return new ChatMsg(_name, content);
        }

        throw new InvalidOperationException("Invalid reply content.");
    }

    public Task<string> GetName() => Task.FromResult(_name);

    public Task<string> GetDescription() => Task.FromResult($"{_name} is a helper agent for Powershell developers.");
}
