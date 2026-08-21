using System.Dynamic;
using NeuroSDKCsharp.Messages.API;
using NeuroSDKCsharp.Websocket;
using Newtonsoft.Json.Linq;

namespace NeuroSDKCsharp.Extensions.VoiceChat.Messages.Incoming;

public class VoiceSpeaking : IncomingMessageHandler<VoiceSpeaking.ResultData>
{
	public sealed class ResultData(bool speaking)
	{
		public bool Speaking = speaking;
	}

	public override bool CanHandle(string command) => command == "voice/speaking";

	protected override ExecutionResult Validate(string command, IncomingData incomingData, out ResultData? resultData)
	{
		resultData = null;

		if (incomingData.Data is not JObject root) return ExecutionResult.Success();
		
		resultData = new ResultData(root.Value<bool>("speaking"));
		return ExecutionResult.Success();
	}

	protected override void ReportResult(ResultData? resultData, ExecutionResult executionResult)
	{
	}

	protected override void Execute(ResultData? incomingData)
	{
		if (incomingData == null) return;
		NeuroVoiceChat.Instance?.SetSpeaking(incomingData.Speaking);
	}
}