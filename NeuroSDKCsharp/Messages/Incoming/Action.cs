using NeuroSDKCsharp.Actions;
using NeuroSDKCsharp.Messages.API;
using NeuroSDKCsharp.Messages.Outgoing;
using NeuroSDKCsharp.Utilities;
using NeuroSDKCsharp.Websocket;
using Newtonsoft.Json.Linq;

namespace NeuroSDKCsharp.Messages.Incoming;

public class Action : IncomingMessageHandler<Action.ResultData>
{
    public class ResultData
    {
        public ResultData(string id) => Id = id;

        public string Id;
        public INeuroAction? Action;
        public object? Data;
    }

    public override bool CanHandle(string command) => command == "action";

    protected override ExecutionResult Validate(string command, IncomingData incomingData, out ResultData? resultData)
    {
        if (incomingData.Data == null)
        {
            resultData = null;
            return ExecutionResult.ServerFailure("The action failed as there is no data.");
        }

        string? actionId = incomingData.Data["id"]?.Value<string>();
        
        if (string.IsNullOrEmpty(actionId))
        {
            resultData = null;
            return ExecutionResult.ServerFailure("The action failed as there is not ID");
        }

        resultData = new ResultData(actionId);
        try
        {
            string? actionName = incomingData.Data?.Value<string>("name");
            string? actionStringifiedData = incomingData.Data?.Value<string>("data");

            if (string.IsNullOrEmpty(actionName))
            {
                resultData = null;
                return ExecutionResult.ServerFailure(Strings.ActionFailedNoName);
            }
            
            INeuroAction? registeredAction = NeuroActionHandler.GetRegistered(actionName);
            
            if (registeredAction == null)
            {
                if (NeuroActionHandler.IsRecentlyUnregistered(actionName))
                {
                    return ExecutionResult.Failure(Strings.ActionFailedUnregistered);
                }

                return ExecutionResult.Failure("action failed unknown action");
            }

            resultData.Action = registeredAction;
            
            if (!ActionData.TryParse(actionStringifiedData, out ActionData? actionData))
                return ExecutionResult.Failure("Action Failed Invalid JSON");
            
            ExecutionResult actionValidationResult = registeredAction.Validate(actionData!, out object? resultActionData);
            resultData.Data = resultActionData;
            
            return actionValidationResult;
        }
        catch (Exception e)
        {
            LogHolder.Error($"action failed caught {e}");

            return ExecutionResult.Failure("Action Failed Caught");
        }
    }

    protected override void ReportResult(ResultData? resultData, ExecutionResult executionResult)
    {
        if (resultData == null)
        {
            ExecutionResult.Failure("Action.ReportResult received null data.");
            return;
        }
        
        WebsocketHandler.Instance!.Send(new ActionResult(resultData.Id,executionResult));
    }

    protected override void Execute(ResultData? incomingData)
    {
        incomingData!.Action!.Execute(incomingData.Data);
    }
}