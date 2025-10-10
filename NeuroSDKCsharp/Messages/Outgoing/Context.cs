using NeuroSDKCsharp.Messages.API;
using NeuroSDKCsharp.Websocket;
using Newtonsoft.Json;

namespace NeuroSDKCsharp.Messages.Outgoing;

public sealed class Context : OutgoingMessageHandler
{
    public Context(string message, bool silent)
    {
        _message = message;
        _silent = silent;
    }
    
    protected override string Command => "context";

    [JsonProperty("message", Order = 0)]
    private string _message;

    [JsonProperty("silent",Order = 10)]
    private bool _silent;

    public static void Send(string message, bool silent = false) =>
        WebsocketHandler.Instance!.Send(new Context(message, silent));
}
