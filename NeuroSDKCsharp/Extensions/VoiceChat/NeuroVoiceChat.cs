using System.Globalization;
using System.Net.WebSockets;
using Microsoft.Xna.Framework;
using NeuroSDKCsharp.Extensions.VoiceChat.Messages.Outgoing;
using NeuroSDKCsharp.Utilities;
using NeuroSDKCsharp.Websocket;

namespace NeuroSDKCsharp.Extensions.VoiceChat;

public class NeuroVoiceChat : BaseWebsocket<NeuroVoiceChat>
{
	public NeuroVoiceChat(Game game, string gameName, string? uriString) :
		base(game, gameName, uriString, [new VoiceStart()])
	{
		_instance = this;
	}

	public readonly List<Speaker> CurrentSpeakers = [];

	public event EventHandler<bool>? OnSpeakingStateChanged;
	
	public event EventHandler? OnSpeechCancelled;

	private const int SpeakerHeaderSize = 4;
	
	/// <summary>
	/// Sends the byte data of the audio received from the server. You can decode this to the Float32 pcm, with the DecodePcm method.  
	/// </summary>
	public event EventHandler<MemoryStream>? OnAudioReceived;

	public static float[]? DecodePcm(MemoryStream stream)
	{
		// Does not fit
		if (stream.Length % sizeof(float) != 0) return null;
			
		float[] samples = new float[stream.Length / sizeof(float)];
		Buffer.BlockCopy(stream.ToArray(), 0, samples, 0, (int)stream.Length);
		return samples;
	}

	public override void OnConnection(object? sender, EventArgs? e)
	{
	}

	public override void OnDisconnected(object? sender, EventArgs? e)
	{
	}

	public override void OnRecieveMessage(object? sender, RecievedMessageData? e)
	{
	}

	protected override Task HandleReceivedMessage(MemoryStream stream, WebSocketReceiveResult result)
	{
		switch (result.MessageType)
		{
			case WebSocketMessageType.Text:
				return base.HandleReceivedMessage(stream, result);
			case WebSocketMessageType.Binary:
				OnAudioReceived?.Invoke(this, stream);
				return Task.CompletedTask;
			// Should be handled by ReceiveMessage
			case WebSocketMessageType.Close:
			default:
				return Task.CompletedTask;
		}
	}

	protected override string GetWebsocketUrl()
	{
		string? newUrl;
		
		newUrl = Environment.GetEnvironmentVariable("NEURO_SDK_WS_URL", EnvironmentVariableTarget.Process) ??
		         Environment.GetEnvironmentVariable("NEURO_SDK_WS_URL", EnvironmentVariableTarget.User) ??
		         Environment.GetEnvironmentVariable("NEURO_SDK_WS_URL", EnvironmentVariableTarget.Machine);
		
		if (UriString != null && UriString.EndsWith("/game"))
		{
			newUrl = $"{UriString}/{GameName}/voice";
		}
		else if (UriString != null && !UriString.EndsWith("/voice"))
		{
			newUrl = $"{UriString}/game/{GameName}/voice";
		}
		else if (UriString != null && !UriString.EndsWith($"/{WebsocketHandler.Instance?.GameName}"))
		{
			newUrl = $"{UriString}/voice";
		}
		else
		{
			LogHolder.Error($"Could not find the correct way to format voice websocket URL.");
		}
		
		LogHolder.Info($"Final Voice URI string: {newUrl}");

		return newUrl ?? "";
	}

	public void SendVoiceAudio(Speaker speaker, byte[] voiceData)
	{
		// 4 is size of header
		byte[] header = new byte[SpeakerHeaderSize];
		header[0] = 0x1;
		header[1] = 0x0;
		header[2] = (byte)(speaker.Id & 0xFF);
		header[3] = (byte)((speaker.Id >> 8) & 0xFF);
		
		var data = header.Concat(voiceData).ToArray();
		_ = WebSocket?.SendAsync(data, WebSocketMessageType.Binary, false, CancellationToken.None);
	}

	public void RegisterSpeakers(Speaker[] speaker)
	{
		CurrentSpeakers.AddRange(speaker);
		Send(new VoiceRegister(speaker));
	}

	public void UnregisterSpeaker(Speaker[] speaker) => UnregisterSpeaker(speaker.Select(speaker1 => speaker1.Id).ToArray());

	public void UnregisterSpeaker(ushort[] speakerIds)
	{
		CurrentSpeakers.RemoveAll(speaker => speakerIds.Contains(speaker.Id));
		Send(new VoiceUnregister(speakerIds));
	}

	public void RenameSpeaker(ushort speakerId, string newName)
	{
		// Easier to do this than rename the variable as modifying structs is a pain.
		CurrentSpeakers.RemoveAll(speaker => speaker.Id == speakerId);
		Speaker newSpeaker = new Speaker(speakerId, newName);
		CurrentSpeakers.Add(newSpeaker);
		Send(new VoiceRegister([newSpeaker]));
	}

	public void SetSpeaking(bool speaking)
	{
		OnSpeakingStateChanged?.Invoke(this, speaking);
	}
	
	public void SpeechCancelled()
	{
		OnSpeechCancelled?.Invoke(this, EventArgs.Empty);
	}
}