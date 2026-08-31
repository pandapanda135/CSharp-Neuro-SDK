using NeuroSDKCsharp.Messages.API;
using NeuroSDKCsharp.Websocket;

namespace NeuroSDKCsharp.Extensions.VoiceChat.Messages.Incoming;

public class VoiceCancelled : IncomingMessageHandler
{
	public override bool CanHandle(string command) => command == "voice/cancelled";

	protected override ExecutionResult Validate(string command, IncomingData incomingData)
	{
		return ExecutionResult.Success();
	}

	protected override void ReportResult(ExecutionResult executionResult)
	{
	}

	protected override void Execute()
	{
		NeuroVoiceChat.Instance?.SpeechCancelled();
	}
}