using NeuroSDKCsharp.Messages.API;
using NeuroSDKCsharp.Messages.Outgoing;

namespace NeuroSDKCsharp.Websocket;

public class MessageQueue
{
    public MessageQueue(List<OutgoingMessageHandler> messages)
    {
        Messages =  new List<OutgoingMessageHandler>(messages);
    }
    
    protected readonly List<OutgoingMessageHandler> Messages;

    public virtual int Count
    {
        get
        {
            lock (Messages)
            {
                return Messages.Count;
            }
        }
    }

    public virtual void Enqueue(OutgoingMessageHandler message)
    {
        lock (Messages)
        {
            foreach (OutgoingMessageHandler existingMessage in Messages)
            {
                if (existingMessage.Merge(message)) return;
            }
            
            Messages.Add(message);
        }
    }
    
    /// <summary>
    /// Remove first element of queue 
    /// </summary>
    /// <returns>The first element of the queue that has been removed</returns>
    public virtual OutgoingMessageHandler? Dequeue()
    {
        lock (Messages)
        {
            if (Messages.Count == 0) return null;

            OutgoingMessageHandler message = Messages[0];
            Messages.RemoveAt(0);

            return message;
        }
    }
}