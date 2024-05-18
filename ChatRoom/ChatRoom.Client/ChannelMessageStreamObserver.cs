using Orleans.Streams;
using Spectre.Console;

namespace ChatRoom;

public sealed class ChannelMessageStreamObserver : IAsyncObserver<ChatMsg>
{
    private readonly string _channelName;

    public ChannelMessageStreamObserver(string channelName) => _channelName = channelName;

    public Task OnCompletedAsync() => Task.CompletedTask;

    public Task OnErrorAsync(Exception ex)
    {
        AnsiConsole.WriteException(ex);

        return Task.CompletedTask;
    }

    public Task OnNextAsync(ChatMsg item, StreamSequenceToken? token = null)
    {
        var text = item.Text;
        text = text.Replace("[", "[[");
        text = text.Replace("]", "]]");
        AnsiConsole.MarkupLine(
            "[[[dim]{0}[/]]][[{1}]] [bold yellow]{2}:[/] {3}",
            item.Created.LocalDateTime, _channelName, item.From!, text);

        return Task.CompletedTask;
    }
}
