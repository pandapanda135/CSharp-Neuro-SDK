using NeuroSDKCsharp.Messages.Outgoing;
using NeuroSDKCsharp.Utilities;
using NeuroSDKCsharp.Websocket;

namespace NeuroSDKCsharp.Actions;

public sealed class ActionWindow
{
    #region Create

    private static bool _createdCorrectly;

    private ActionWindow()
    {
    }
    
    public static ActionWindow Create()
    {
        try
        {
            _createdCorrectly = true;
            ActionWindow actionWindow = new ActionWindow();
            return actionWindow;
        }
        catch (Exception e)
        {
            _createdCorrectly = false;
            LogHolder.Error($"Issue with creating action window: {e}");
            throw;
        }
    }
    // if not create correctly add your version of delete or dispose here
    public void Initialize()
    {
        if (!_createdCorrectly)
        {
            LogHolder.Error($"ActionWindow was not created correctly, you should use the Create method");
        }
    }

    #endregion
    
    #region State
    
    public enum State
    {
        Building,
        Registered,
        Forced,
        Ended,
    }

    public State CurrentState { get; private set; } = State.Building;

    private bool ValidateFrozen()
    {
        if (CurrentState != State.Building)
        {
            LogHolder.Error("Cannot change action window after it has been registered");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Sends an action register to the websocket. This will also make this window immutable
    /// </summary>
    public void Register()
    {
        if (CurrentState != State.Building)
        {
            LogHolder.Error("Cannot register action window more than once");
            return;
        }

        if (_actions.Count == 0)
        {
            LogHolder.Error("Cannot register an action window with no actions");
            return;
        }

        if (_contextMessage is not (null or ""))
        {
            Context.Send(_contextMessage,_contextSilent!.Value);
        }
        NeuroActionHandler.RegisterActions(_actions);

        CurrentState = State.Registered;
    }
    #endregion

    #region Context
    
    private string? _contextMessage;
    private bool? _contextSilent;

    /// <summary>
    /// Will send a Context message with the action register
    /// </summary>
    /// <returns>itself this is for chaining</returns>
    public ActionWindow SetContext(string message, bool silent = false)
    {
        if (!ValidateFrozen()) return this;

        _contextMessage = message;
        _contextSilent = silent;

        return this;
    }
    #endregion

    #region Actions
    
    private readonly List<INeuroAction> _actions = new();

    /// <summary>
    /// adds a new list of possible actions
    /// </summary>
    /// <returns>itself this is for chaining</returns>
    public ActionWindow AddAction(INeuroAction action)
    {
        if (!ValidateFrozen()) return this;

        if (action.ActionWindow != null)
        {
            if (action.ActionWindow != this)
            {
                LogHolder.Error($"Cannot add action {action.Name} to this action window as it is already in another action window");
            }

            return this;
        }

        if (!action.CanAddToActionWindow(this)) return this;

        if (_actions.Any(a => a.Name == action.Name))
        {
            LogHolder.Error($"Cannot add two action that have the same name. The name was {action.Name}");
            return this;
        }
        
        action.SetActionWindow(this);
        _actions.Add(action);

        return this;
    }
    
    #endregion

    #region Force
    
    private Func<bool>? _shouldForceFunc;
    private Func<string>? _forceQueryGetter;
    private Func<string?>? _forceStateGetter;
    private bool? _forceEphemeralContext;
    private ForcePriority _priority;
    
    /// <summary>
    /// Specify a condition under which the actions will be forced
    /// </summary>
    /// <param name="shouldForce">if this returns true, the actions will be forced</param>
    /// <param name="queryGetter">A getter for the query of the action force, invoked at force-time</param>
    /// <param name="stateGetter">A getter for the state of the action force, invoked at force-time</param>
    /// <param name="ephemeralContext">If true the query and state won't be remembered after the action force is finished</param>
    /// <param name="priority">Determines how urgently Neuro should respond to the action force when she is speaking, for specifics you can check the API spec.</param>
    /// <returns>itself for chaining</returns>
    public ActionWindow SetForce(Func<bool> shouldForce, Func<string> queryGetter, Func<string?> stateGetter,
        bool ephemeralContext = false, ForcePriority priority = ForcePriority.Low)
    {
        if (!ValidateFrozen()) return this;

        _shouldForceFunc = shouldForce;
        _forceQueryGetter = queryGetter;
        _forceStateGetter = stateGetter;
        _forceEphemeralContext = ephemeralContext;
        _priority = priority;

        return this;
    }
    
    /// <summary>
    /// Specify a condition under which the actions will be forced
    /// </summary>
    /// <param name="shouldForce">if this returns true, the actions will be forced</param>
    /// <param name="ephemeralContext">If true the query and state won't be remembered after the action force is finished</param>
    /// <param name="priority">Determines how urgently Neuro should respond to the action force when she is speaking, for specifics you can check the API spec.</param>
    /// <returns>itself for chaining</returns>
    public ActionWindow SetForce(Func<bool> shouldForce, string query, string? state, bool ephemeralContext = false, ForcePriority priority = ForcePriority.Low) =>
        SetForce(shouldForce, () => query, () => state, ephemeralContext, priority);
    
    /// <summary>
    /// specify a time in seconds after which the actions should be forced
    /// </summary>
    /// <param name="afterSeconds">Time till the action is forced</param>
    /// <param name="queryGetter">A getter for the query of the action force, invoked at force-time</param>
    /// <param name="stateGetter">A getter for the state of the action force, invoked at force-time</param>
    /// <param name="ephemeralContext">if true, the query and state won't be remembered after the action force is finished</param>
    /// <param name="priority">Determines how urgently Neuro should respond to the action force when she is speaking, for specifics you can check the API spec.</param>
    /// <returns>itself for chaining</returns>
    public ActionWindow SetForce(float afterSeconds, Func<string> queryGetter, Func<string?> stateGetter,
        bool ephemeralContext = false, ForcePriority priority = ForcePriority.Low)
    {
        float startTime = Time.ElapsedTime;
        
        return SetForce(ShouldForce, queryGetter, stateGetter, ephemeralContext, priority);

        bool ShouldForce()
        {
            return Time.ElapsedTime - startTime >= afterSeconds;
        }
    }
    
    /// <summary>
    /// specify a time in seconds after which the actions should be forced
    /// </summary>
    /// <param name="afterSeconds">Time till the action is forced</param>
    /// <param name="ephemeralContext">if true the query and state won't be remembered after the action force is finished</param>
    /// <param name="priority">Determines how urgently Neuro should respond to the action force when she is speaking, for specifics you can check the API spec.</param>
    /// <returns>itself for chaining</returns>
    public ActionWindow SetForce(float afterSeconds, string query, string? state, bool ephemeralContext = false, ForcePriority priority = ForcePriority.Low) =>
        SetForce(afterSeconds, () => query,() => state, ephemeralContext, priority);

    public void Force()
    {
        if (CurrentState != State.Registered) return;

        CurrentState = State.Forced;
        _shouldForceFunc = null;
        WebsocketHandler.Instance!.Send(new ActionsForce(_forceQueryGetter!(), _forceStateGetter!(), _forceEphemeralContext, _actions, _priority));
    }
    #endregion

    #region End
    
    private Func<bool>? _shouldEndFunc;

    /// <summary>
    /// Specify a condition under which the actions should be unregistered and this window closed
    /// </summary>
    /// <param name="shouldEnd">If this returns true, the actions will be unregistered</param>
    /// <returns>itself for chaining</returns>
    public ActionWindow SetEnd(Func<bool> shouldEnd)
    {
        if (!ValidateFrozen()) return this;

        _shouldEndFunc = shouldEnd;

        return this;
    }

    public ActionWindow SetEnd(float afterSeconds)
    {
        float startTime = Time.ElapsedTime;

        return SetEnd(ShouldEnd);

        bool ShouldEnd()
        {
            return Time.ElapsedTime - startTime >= afterSeconds;
        }
    }

    public void End()
    {
        if (CurrentState >= State.Ended) return;
        LogHolder.Info($"Ending action window");
        
        NeuroActionHandler.UnregisterActions(_actions);
        _shouldForceFunc = null;
        _shouldEndFunc = null;
        CurrentState = State.Ended;
    }
    #endregion

    #region Handling
    /// <summary>
    /// Run <see cref="ExecutionResult"/> through this ActionWindow, is called automatically in <see cref="BaseNeuroAction"/>.
    /// </summary>
    public ExecutionResult Result(ExecutionResult result)
    {
        if (CurrentState <= State.Building)
            throw new InvalidOperationException("Cannot handle a result before registering the action window");
        if (CurrentState >= State.Ended)
            throw new InvalidOperationException("Cannot handle a result after the Action window has ended");

        if (result.Successful) End();
        
        return result;
    }

    public void Update()
    {
        if (CurrentState != State.Registered) return;
    
        if (_shouldForceFunc != null && _shouldForceFunc())
        {
            Force();
        }
    
        if (_shouldEndFunc != null && _shouldEndFunc())
        {
            End();
        }
        Time.Update();
    }
    #endregion
}