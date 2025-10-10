using NeuroSDKCsharp.Messages.Outgoing;
using NeuroSDKCsharp.Utilities;
using NeuroSDKCsharp.Websocket;

namespace NeuroSDKCsharp.Actions;

public static class NeuroActionHandler
{
    private static List<INeuroAction> _currentRegisteredActions = new();
    private static readonly List<INeuroAction> DyingActions = new();

    public static INeuroAction? GetRegistered(string name) =>
        _currentRegisteredActions.FirstOrDefault(a => a.Name == name);

    public static bool IsRecentlyUnregistered(string name) => DyingActions.Any(a => a.Name == name);

    public static void RegisterActions(IReadOnlyCollection<INeuroAction> newActions)
    {
        _currentRegisteredActions.RemoveAll(
            oldAction => newActions.Any(newAction => oldAction.Name == newAction.Name));
        DyingActions.RemoveAll(oldAction => newActions.Any(newAction => oldAction.Name == newAction.Name));
        _currentRegisteredActions.AddRange(newActions);
        WebsocketHandler.Instance!.Send(new ActionsRegister(newActions));
    }

    public static void RegisterActions(params INeuroAction[] newActions)
        => RegisterActions((IReadOnlyCollection<INeuroAction>)newActions);

    public static void UnregisterActions(IEnumerable<string> removeActionsList)
    {
        INeuroAction[] actionsToRemove = _currentRegisteredActions.Where(oldAction =>
            removeActionsList.Any(removeAction => oldAction.Name == removeAction)).ToArray();

        _currentRegisteredActions.RemoveAll(actionsToRemove.Contains);
        DyingActions.AddRange(actionsToRemove);
        RemoveActions();
        
        WebsocketHandler.Instance!.Send(new ActionsUnregister(removeActionsList));
        
        return;

        void RemoveActions() // unity sdk waits for 10 seconds, I'm not too sure why.
        {
            DyingActions.RemoveAll(actionsToRemove.Contains);
        }
    }

    public static void UnregisterActions(IEnumerable<INeuroAction> removeActionsList) =>
        UnregisterActions(removeActionsList.Select(a => a.Name));

    public static void UnregisterActions(params INeuroAction[] removeActinsList) =>
        UnregisterActions((IReadOnlyCollection<INeuroAction>)removeActinsList);

    public static void UnregisterActions(params string[] removeActionsNamesList) =>
        UnregisterActions((IReadOnlyCollection<string>)removeActionsNamesList);

    public static void ResendRegisteredActions()
    {
        WebsocketHandler.Instance!.Send(new ActionsRegister(_currentRegisteredActions));
    }
    
    public static void OnApplicationQuit(object? obj, EventArgs args)
    {
        ExitApplicationEvent.ApplicationExiting += OnApplicationQuit;
        Task.Run(async () =>
        {
            await WebsocketHandler.Instance!.SendImmediate(new ActionsUnregister(_currentRegisteredActions));
            _currentRegisteredActions = null!;
        });
    }
}