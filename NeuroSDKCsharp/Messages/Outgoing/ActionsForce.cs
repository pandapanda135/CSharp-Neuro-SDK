using NeuroSDKCsharp.Actions;
using NeuroSDKCsharp.Messages.API;
using Newtonsoft.Json;

namespace NeuroSDKCsharp.Messages.Outgoing;

/// <summary>
/// Determines how urgently Neuro should respond to the action force when she is speaking.
/// </summary>
public enum ForcePriority
{
    /// <summary>
    /// This is the default, this waits for Neuro to finish speaking before responding
    /// </summary>
    Low,
    /// <summary>
    /// This will cause her to finish her utterance sooner.
    /// </summary>
    Medium,
    /// <summary>
    /// This prompts her to process the actions force immediately shortening her utterance and then responding.
    /// </summary>
    High,
    /// <summary>
    /// Will interrupt her speech and make her respond at once, use this with caution.
    /// </summary>
    Critical
}

public class ActionsForce : OutgoingMessageHandler
{
    protected override string Command => "actions/force";

    public ActionsForce(string query, string? state, bool? ephemeralContext, IEnumerable<INeuroAction> actions, ForcePriority priority = ForcePriority.Low)
    {
        _state = state;
        _query = query;
        _ephemeralContext = ephemeralContext;
        _actionNames = actions.Select(a => a.Name).ToArray();
        
        _priority = priority switch
        {
            ForcePriority.Low => "low",
            ForcePriority.Medium => "medium",
            ForcePriority.High => "high",
            ForcePriority.Critical => "critical",
            _ => "low"
        };
    }

    public ActionsForce(string query, string? state, bool? ephemeralContext, ForcePriority priority = ForcePriority.Low,
        params INeuroAction[] actions) : this(query, state, ephemeralContext, actions, priority) {}
    
    [JsonProperty("state", Order = 0)]
    private readonly string? _state;

    [JsonProperty("query", Order = 10)]
    private readonly string _query;

    [JsonProperty("ephemeral_context", Order = 20)]
    private readonly bool? _ephemeralContext;

    [JsonProperty("action_names", Order = 30)]
    private readonly string[] _actionNames;

    [JsonProperty("priority", Order = 40)]
    private readonly string? _priority;
}