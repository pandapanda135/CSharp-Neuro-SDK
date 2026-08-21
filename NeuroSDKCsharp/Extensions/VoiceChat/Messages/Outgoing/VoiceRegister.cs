using NeuroSDKCsharp.Messages.API;
using Newtonsoft.Json;

namespace NeuroSDKCsharp.Extensions.VoiceChat.Messages.Outgoing;

public class VoiceRegister : OutgoingMessageHandler
{
	public VoiceRegister(Speaker[] speakers)
	{
		_speakers = speakers;
	}
	
	protected override string Command => "voice/speakers/register";

	[JsonProperty("speakers", Order = 0)]
	private readonly Speaker[] _speakers;
}