using NeuroSDKCsharp.Messages.API;
using NeuroSDKCsharp.Websocket;
using Newtonsoft.Json;

namespace NeuroSDKCsharp.Messages.Outgoing;

public sealed class ActionResult : OutgoingMessageHandler
{
    public ActionResult(string id,ExecutionResult result)
    {
        _id = id;
        _success = result.Successful;
        _message = result.Message;
    }
    
    protected override string Command => "action/result";
    
    [JsonProperty("id", Order = 0)]
    private string _id;

    [JsonProperty("success", Order = 10)]
    private bool _success;

    [JsonProperty("message", Order = 20)]
    private string? _message;
}