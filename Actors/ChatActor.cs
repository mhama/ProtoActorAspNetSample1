
using Proto;

record ChatMessage(string user, string message);

public delegate Task SendMessageFuncType(string connectionId, string user, string message);

public class ChatActor : IActor
{
    private string _connectionId; // SignalR送信用コネクションID
    SendMessageFuncType _sendMessageFunc;

    PID _aggregatorPID;

    public ChatActor(string connectionId, SendMessageFuncType sendMessageFunc, PID aggregatorPID) {
        _connectionId = connectionId;
        _sendMessageFunc = sendMessageFunc;
        _aggregatorPID = aggregatorPID;
    }
    
    public Task ReceiveAsync(IContext context)
    {
        switch(context.Message) {
        case ChatMessage msg:
            Console.WriteLine("ChatActor: received message" + context.Message);
            context.Send(_aggregatorPID, new ChatAggregateRequest(context.Self, msg.user, msg.message));
            break;
        case ChatAggregatedResult msg:
            _sendMessageFunc?.Invoke(_connectionId, "aggregated", msg.bulkMessage);
            break;
       default:
            Console.WriteLine("ChatActor: other message:" + context.Message);
            break;
        }
        return Task.CompletedTask;
    }
}



public class ChatAggregatorActorPID {
    public PID pid;
}

record ChatAggregateRequest(PID sender, string user, string message);

record ChatAggregatedResult(string bulkMessage);

//
// actor to aggregate messages of users
//
public class ChatMessageAggregatorActor : IActor
{
    Dictionary<string, string> messagesDic = new Dictionary<string, string>();

    public ChatMessageAggregatorActor() {
        Console.WriteLine("ChatMessageAggregatorActor created.");
    }

    public Task ReceiveAsync(IContext context)
    {
        try {
            Console.WriteLine("ChatListActor: message" + context.Message);
            switch(context.Message) {
            case ChatAggregateRequest msg:
                messagesDic[msg.user] = msg.message;
                var allMessageText = string.Join("\n", messagesDic.Select(pair => $"{pair.Key} : {pair.Value}"));
                //Console.WriteLine("allMessageText:" + allMessageText);
                context.Send(msg.sender, new ChatAggregatedResult(allMessageText));
                break;
            }
        }
        catch(Exception e) {
            Console.WriteLine("Exception: " + e + e.StackTrace);
            throw e;
        }
        return Task.CompletedTask;
    }
}
