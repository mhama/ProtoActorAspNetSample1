
using Proto;
using System;

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



public class ChatAggregatorActorPIDSelector {
    //public PID pid;

    public List<PID> aggregators;

    Random random = new Random();

    public ChatAggregatorActorPIDSelector() {
    }

    public PID NextPid() {
        int index = random.Next(0, aggregators.Count);
        return aggregators[index];
    }
}

record ChatAggregateRequest(PID sender, string user, string message);

record ChatAggregatedResult(string bulkMessage);

//
// actor to aggregate messages of users
//
public class ChatMessageAggregatorActor : IActor
{
    static int count = 0;
    int index;
    Dictionary<string, string> messagesDic = new Dictionary<string, string>();

    string dummyMessage = "";

    string currentFrameMessage = "";
    DateTime lastAggregatedTime = DateTime.Now;

    public ChatMessageAggregatorActor() {
        index = count++;
        Console.WriteLine($"ChatMessageAggregatorActor {index} created.");

        // create arbitrary size dummy string
        for(int i=0 ; i<400 ; i++) {
            dummyMessage += "DummyDummy.";
        }
    }

    public Task ReceiveAsync(IContext context)
    {
        try {
            Console.WriteLine($"ChatMessageAggregatorActor {index} : message" + context.Message);
            switch(context.Message) {
            case ChatAggregateRequest msg:
                messagesDic[msg.user] = msg.message;
                if ((DateTime.Now - lastAggregatedTime).TotalMilliseconds > 100.0) {
                    Console.WriteLine("aggregated.");
                    lastAggregatedTime = DateTime.Now;
                            aggregateCurrentFrameMessage();
                }
                //Console.WriteLine("allMessageText:" + allMessageText);
                context.Send(msg.sender, new ChatAggregatedResult(currentFrameMessage));
                break;
            }
        }
        catch(Exception e) {
            Console.WriteLine("Exception: " + e + e.StackTrace);
            throw e;
        }
        return Task.CompletedTask;
    }

    void aggregateCurrentFrameMessage() {
        currentFrameMessage = string.Join("\n", messagesDic.Select(pair => $"{pair.Key} : {pair.Value}"));
    }
}
