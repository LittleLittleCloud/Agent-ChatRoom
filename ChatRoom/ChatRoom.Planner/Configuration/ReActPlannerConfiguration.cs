using System.Text.Json.Serialization;
using ChatRoom.SDK;
using Json.Schema.Generation;

namespace ChatRoom.Planner;

public class ReActPlannerConfiguration
{
    [JsonPropertyName("name")]
    [Description("The name of the agent. Default is 'react-planner'")]
    public string Name { get; set; } = "react-planner";

    [JsonPropertyName("system_message")]
    [Description($"System message, default is '{ReActPlanner.PlannerPrompt}'")]
    public string SystemMessage { get; set; } = ReActPlanner.PlannerPrompt;

    [JsonPropertyName("description")]
    [Description("Description of the agent. Default is 'A react planner agent, which can help other agents to plan actions.'")]
    public string Description { get; set; } = "A react planner agent, which can help other agents to plan actions.";

    [JsonPropertyName("openai_config")]
    [Description("OpenAI configuration for the agent. To get ideal result, it's recommended to use gpt-4o or above for this agent.")]
    public OpenAIClientConfiguration OpenAIConfiguration { get; set; } = new OpenAIClientConfiguration();
}
