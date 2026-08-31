using NeuroSDKCsharp.Messages.API;
using NeuroSDKCsharp.Utilities;
using NeuroSDKCsharp.Websocket;
using Newtonsoft.Json.Linq;

namespace NeuroSDKCsharp.Extensions.VoiceChat.Messages.Incoming;

public class VoiceReady : IncomingMessageHandler<VoiceReady.ResultData>
{
	public sealed class ResultData
	{
		public ResultData(int sampleRate, int channels)
		{
			SampleRate = sampleRate;
			Channels = channels;
		}
		
		public int SampleRate { get; }
		public int Channels { get; }
	}
	public override bool CanHandle(string command) => command == "voice/ready";

	protected override ExecutionResult Validate(string command, IncomingData incomingData, out ResultData? resultData)
	{
		resultData = null;

		if (incomingData.Data is not JObject root) return ExecutionResult.Success();
		
		int sampleRate = root.Value<int>("sample_rate");
		int channels = root.Value<int>("channels");
		
		resultData = new ResultData(sampleRate, channels);
		return ExecutionResult.Success();
	}

	protected override void ReportResult(ResultData? resultData, ExecutionResult executionResult)
	{
	}

	protected override void Execute(ResultData? incomingData)
	{
		LogHolder.Info($"Server is ready for data: {incomingData?.Channels}   {incomingData?.SampleRate}");
	}
}