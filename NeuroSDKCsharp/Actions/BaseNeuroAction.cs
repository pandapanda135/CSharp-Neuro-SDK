using NeuroSDKCsharp.Json;
using NeuroSDKCsharp.Utilities;
using NeuroSDKCsharp.Websocket;

namespace NeuroSDKCsharp.Actions;

public abstract class BaseNeuroAction : INeuroAction
{
    public ActionWindow? ActionWindow { get; private set; }

    protected BaseNeuroAction()
    {
        ActionWindow = null;
    }

    public abstract string Name { get; }
    protected abstract string Description { get; }
    protected abstract JsonSchema? Schema { get; }

    public virtual bool CanAddToActionWindow(ActionWindow actionWindow) => true;

    ExecutionResult INeuroAction.Validate(ActionData actionData, out object? resultData)
    {
        ExecutionResult result = Validate(actionData, out resultData);

        if (ActionWindow != null)
        {
            return ActionWindow.Result(result);
        }

        return result;
    }

    void INeuroAction.Execute(object? data) => Execute(data);
    
    public virtual WSAction GetWsAction()
    {
        return new WSAction(Name, Description, Schema);
    }

    protected abstract ExecutionResult Validate(ActionData actionData, out object? resultData);
    protected abstract void Execute(object? data);

    void INeuroAction.SetActionWindow(ActionWindow actionWindow)
    {
        if (ActionWindow != null)
        {
            if (ActionWindow != actionWindow)
            {
                Logger.Error("Cannot set the action window for this action as it is already set");
            }
            
            return;
        }

        ActionWindow = actionWindow;
    }
}