using Microsoft.Xna.Framework;
using NeuroSDKCsharp.Messages.Outgoing;

namespace NeuroSDKCsharp.Websocket;

public class CharacterMetadata(string characterInfo, string displayName)
{
    public string CharacterInfo = characterInfo;
    public string DisplayName = displayName;
}

public sealed class SpeechFinishedResult(bool isFinal, bool? cancelled, string? reason)
{
    public bool IsFinal = isFinal;
    public bool? Cancelled = cancelled;
    public string? Reason = reason;
}

public class WebsocketHandler : BaseWebsocket<WebsocketHandler>
{
    public WebsocketHandler(Game game, string gameName, string? uriString) : base(game, gameName, uriString,
        [new Startup()])
    {
        _instance = this;
    }

    public CharacterMetadata? Character { get; private set; }

    public event EventHandler<CharacterMetadata>? OnCharacterChanged;
    
    public SpeechFinishedResult? SpeechFinished { get; private set; }
    
    /// <summary>
    /// When the server sends a speech_finished action, this will be invoked. You can use this to 
    /// </summary>
    public event EventHandler<SpeechFinishedResult>? OnSpeechFinished;

    public override void OnConnection(object? sender, EventArgs? e)
    {
    }

    public override void OnDisconnected(object? sender, EventArgs? e)
    {
    }

    public override void OnRecieveMessage(object? sender, RecievedMessageData? e)
    {
    }

    public void SetCharacterMetadata(CharacterMetadata metadata)
    {
        Character = metadata;
        OnCharacterChanged?.Invoke(this, Character);
    }
    
    public void SetSpeechFinished(SpeechFinishedResult result)
    {
        SpeechFinished = result;
        OnSpeechFinished?.Invoke(this, SpeechFinished);
    }
}