using ChatRoom.SDK;

namespace ChatRoom.OpenAI;

internal class CreateConfigurationCommand : CreateConfigurationFromTemplateCommand
{
    public CreateConfigurationCommand()
        : base("chatroom_openai_configuration_schema.json", ["chatroom-openai"])
    {
    }
}

internal class ListTemplatesCommand : ListAvailableTemplatesCommand
{
    public ListTemplatesCommand()
        : base(new Dictionary<string, string>
        {
            ["chatroom-openai"] = "get-started template for chatroom openai",
        })
    {
    }
}
