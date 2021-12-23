
using Proto;

record ChatMessage(string user, string message);

public delegate Task SendMessageFuncType(string connectionId, string user, string message);

public class ChatActor : IActor
{
    private string _connectionId; // SignalR送信用コネクションID
    SendMessageFuncType _sendMessageFunc;

    public ChatActor(string connectionId, SendMessageFuncType sendMessageFunc) {
        _connectionId = connectionId;
        _sendMessageFunc = sendMessageFunc;
    }
    
    public Task ReceiveAsync(IContext context)
    {
        Console.WriteLine("ChatActor: message" + context.Message);
        switch(context.Message) {
        case ChatMessage msg:
            _sendMessageFunc?.Invoke(_connectionId, msg.user, msg.message + " by Proto.Actor");
            break;
        }
        return Task.CompletedTask;
    }
}

/*
record ChatListMessage(string user, string message);

public class ChatListActor : IActor
{
    public Task ReceiveAsync(IContext context)
    {
        Console.WriteLine("ChatListActor: message" + context.Message);
        switch(context.Message) {
        case ChatListMessage msg:
            // signalRのhubに投げる？
            break;
        }
        return Task.CompletedTask;
    }
}
*/