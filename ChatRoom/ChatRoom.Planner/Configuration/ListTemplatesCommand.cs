using ChatRoom.SDK;

namespace ChatRoom.Planner;

internal class ListTemplatesCommand : ListAvailableTemplatesCommand
{
    public ListTemplatesCommand()
        : base(new Dictionary<string, string>
        {
            ["chatroom-planner"] = "get-started template for chatroom planner",
        })
    {
    }
}
