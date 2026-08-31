using NeuroSDKCsharp.Messages.API;
using Newtonsoft.Json;

namespace NeuroSDKCsharp.Extensions.VoiceChat.Messages.Outgoing;

public class VoiceUnregister : OutgoingMessageHandler
{
	public VoiceUnregister(ushort[] ids)
	{
		Ids = ids;
	}
	
	protected override string Command => "voice/speakers/unregister";

	[JsonProperty("Ids", Order = 0)]
	private readonly ushort[] Ids;
}