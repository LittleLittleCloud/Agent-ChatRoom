using ChatRoom.SDK;

namespace ChatRoom.OpenAI;

internal class OpenAICreateConfigurationFromTemplateCommand : CreateConfigurationFromTemplateCommand
{
    public OpenAICreateConfigurationFromTemplateCommand()
        : base("chatroom_openai_configuration_schema.json", ["chatroom-openai"])
    {
    }
}
