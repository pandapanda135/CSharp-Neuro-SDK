using NeuroSDKCsharp.Messages.API;
using NeuroSDKCsharp.Websocket;
using Newtonsoft.Json.Linq;

namespace NeuroSDKCsharp.Messages.Incoming;

public class Startup : IncomingMessageHandler<Startup.ResultData>
{
	public class ResultData
	{
		public ResultData(string characterId, string displayName)
		{
			CharacterId = characterId;
			DisplayName = displayName;
		}

		public string CharacterId { get; }
		public string DisplayName { get; }
	}

	public override bool CanHandle(string command) => command == "startup";

	protected override ExecutionResult Validate(string command, IncomingData incomingData, out ResultData? resultData)
	{
		resultData = null;
		if (incomingData.Data == null)
		{
			return ExecutionResult.Failure("Did not receive incoming data for Startup command.");
		}

		JObject? session = incomingData.Data.Value<JObject>("session");
		if (session == null)
		{
			return ExecutionResult.Failure("Could not get session from data.");
		}

		// IDK why we do this rather than fail and send error but unity SDK does this, so I guess it's the intended method.
		string characterId = session.Value<string>("characterId") ?? "";
		if (characterId.Length == 0) return ExecutionResult.Success();
		
		string displayName = session.Value<string>("displayName") ?? characterId;
		resultData = new ResultData(characterId, displayName.Length == 0 ? characterId : displayName);
		return ExecutionResult.Success();
	}

	protected override void ReportResult(ResultData? resultData, ExecutionResult executionResult)
	{
	}

	protected override void Execute(ResultData? incomingData)
	{
		if (incomingData == null) return;

		WebsocketHandler.Instance?.SetCharacterMetadata(
			new CharacterMetadata(incomingData.CharacterId, incomingData.DisplayName));
	}
}