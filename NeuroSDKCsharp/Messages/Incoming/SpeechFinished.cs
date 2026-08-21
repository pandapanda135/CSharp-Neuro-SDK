
using NeuroSDKCsharp.Messages.API;
using NeuroSDKCsharp.Websocket;
using Newtonsoft.Json.Linq;

namespace NeuroSDKCsharp.Messages.Incoming;

// ReSharper disable once UnusedType.Global
public sealed class SpeechFinished : IncomingMessageHandler<SpeechFinished.ParsedData>
{
	public sealed class ParsedData
	{
		public ParsedData(bool isFinal, bool? cancelled, string? reason)
		{
			IsFinal = isFinal;
			Cancelled = cancelled;
			Reason = reason;
		}

		public bool IsFinal { get; }
		public bool? Cancelled { get; }
		public string? Reason { get; }
	}

	public override bool CanHandle(string command) => command == "speech_finished";

	protected override ExecutionResult Validate(string command, IncomingData messageData, out ParsedData? parsedData)
	{
		parsedData = null;

		if (messageData.Data is not JObject root || root["data"] is not JObject data)
			return ExecutionResult.Success();

		bool isFinal = data.Value<bool>("isFinal");
		bool? cancelled = data.Value<bool?>("cancelled");
		string? reason = data.Value<string?>("reason");

		parsedData = new ParsedData(isFinal, cancelled, reason);
		return ExecutionResult.Success();
	}

	protected override void ReportResult(ParsedData? parsedData, ExecutionResult result)
	{
	}

	protected override void Execute(ParsedData? parsedData)
	{
		if (parsedData == null) return;
		WebsocketHandler.Instance?.SetSpeechFinished(
			new SpeechFinishedResult(parsedData.IsFinal, parsedData.Cancelled, parsedData.Reason)
		);
	}
}
