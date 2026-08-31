using NeuroSDKCsharp.Messages.API;
using NeuroSDKCsharp.Websocket;
using Newtonsoft.Json;

namespace NeuroSDKCsharp.Extensions.VoiceChat.Messages.Outgoing;

public class VoiceStart : OutgoingMessageHandler
{
	protected override string Command => "voice/start";
}