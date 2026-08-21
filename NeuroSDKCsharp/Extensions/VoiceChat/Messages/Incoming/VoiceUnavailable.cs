using NeuroSDKCsharp.Messages.API;
using NeuroSDKCsharp.Utilities;
using NeuroSDKCsharp.Websocket;
using Newtonsoft.Json.Linq;

namespace NeuroSDKCsharp.Extensions.VoiceChat.Messages.Incoming;

public class VoiceUnavailable : IncomingMessageHandler<VoiceUnavailable.ResultData>
{
	public sealed class ResultData(string reason)
	{
		public string Reason { get; } = reason;
	}

	public override bool CanHandle(string command) => command == "voice/unavailable";

	protected override ExecutionResult Validate(string command, IncomingData incomingData, out ResultData? resultData)
	{
		resultData = null;

		if (incomingData.Data is not JObject root) return ExecutionResult.Success();

		string reason = root.Value<string?>("reason") ?? string.Empty;
		
		resultData = new ResultData(reason);
		return  ExecutionResult.Success();
	}

	protected override void ReportResult(ResultData? resultData, ExecutionResult executionResult)
	{
	}

	protected override void Execute(ResultData? incomingData)
	{
		Logger.Error($"Issue with voice chat: {incomingData?.Reason}");
	}
}