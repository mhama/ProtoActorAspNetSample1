using System.Threading.Channels;
using Microsoft.AspNetCore.SignalR;
using Proto;

public class ChatHub : Hub
{
    ActorSystem _system;
    private readonly IHubContext<ChatHub> _chatHubContext;
    CancellationTokenSource cts;

    PID UserActorPid
    {
        get => Context.Items["user-pid"] as PID;
        set => Context.Items["user-pid"] = value;
    }

    // Hubインスタンスは呼び出しごとに作り直される。（マジ？）
    public ChatHub(ActorSystem system, IHubContext<ChatHub> chatHubContext) {
        _system = system;
        _chatHubContext = chatHubContext;
        Console.WriteLine("ChatHub created.");
    }

    // 接続時に呼ばれる。接続ユーザーに対応するActorを作る
    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"Client {Context.ConnectionId} connected");
        var connectionId = Context.ConnectionId;
        UserActorPid = _system.Root.Spawn(
            Props.FromProducer(() => new ChatActor(connectionId, SendMessageFunc))
        );
        return Task.CompletedTask;
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        cts?.Cancel();
        return Task.CompletedTask;
    }

    // Actorにメッセージを飛ばす（非ストリーミング）
    public async Task SendMessage(string user, string message) 
    {
        _system.Root.Send(UserActorPid, new ChatMessage(user, message));
    }

    const int idleConnectionTimeoutMs = 30000;

    // Actorにメッセージを飛ばす（ストリーミング）
    // ストリーミングのほうがコネクションを維持してHubインスタンスも維持するので
    // メモリ負荷が低い
    public async Task SendMessageStream(ChannelReader<string> stream)
    {
        Console.WriteLine("SendMessageStream received.");
        cts = new CancellationTokenSource();
        cts.CancelAfter(idleConnectionTimeoutMs);
        try {
            // ストリーミングメッセージがあるまで待ち、受け取る。
            // タイムアウト時間がくるとキャンセルしてコネクションを閉じる。
            while (await stream.WaitToReadAsync(cts.Token))
            {
                while (stream.TryRead(out string message))
                {
                    _system.Root.Send(UserActorPid, new ChatMessage("streaming", message));
                    Console.WriteLine(message);
                }
                cts.CancelAfter(idleConnectionTimeoutMs);
            }
        }
        catch(OperationCanceledException e)
        {
            Console.WriteLine("connection canceled. reason:" + e.Message);
            Context.Abort();
        }
        finally
        {
            cts.Dispose();
        }
    }

    // ユーザーにメッセージを返すメソッド。Actorに与えて使ってもらう。
    public async Task SendMessageFunc(string connectionId, string user, string message) {
        await Task.Delay(100);
        Console.WriteLine($"Send message: {user}, {message}");
        await _chatHubContext.Clients.Client(connectionId).SendAsync("ReceiveMessage", user, message);
        //await _chatHubContext.Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}