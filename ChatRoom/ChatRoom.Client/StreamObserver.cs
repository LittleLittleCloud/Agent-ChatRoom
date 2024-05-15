using Orleans.Streams;
using Spectre.Console;

namespace ChatRoom;

public sealed class StreamObserver : IAsyncObserver<ChatMsg>
{
    private readonly string _roomName;

    public StreamObserver(string roomName) => _roomName = roomName;

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
            item.Created.LocalDateTime, _roomName, item.From, text);

        return Task.CompletedTask;
    }
}
