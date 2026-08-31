using Newtonsoft.Json;

namespace NeuroSDKCsharp.Extensions.VoiceChat;

public struct Speaker
{
	public Speaker(ushort id, string name)
	{
		Id = id;
		Name = name;
	}

	[JsonProperty("id", Order = 0)]
	public readonly ushort Id;
	[JsonProperty("name", Order = 10)]
	public string Name;
}