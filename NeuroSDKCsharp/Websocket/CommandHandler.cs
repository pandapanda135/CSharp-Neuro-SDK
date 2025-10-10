using NeuroSDKCsharp.Messages.API;
using NeuroSDKCsharp.Utilities;

namespace NeuroSDKCsharp.Websocket;

public sealed class CommandHandler
{
    private readonly List<IIncomingMessageHandler?> _handlers = new();

    private void Start() // this will need to be called manually as there is nothing similar
    {
        _handlers.AddRange(ReflectionHelpers.GetAllInDomain<IIncomingMessageHandler>());
    }

    public void Handle(string command, IncomingData data)
    {
        if (_handlers.Count == 0) Start();
        
        try
        {
            foreach (var handler in _handlers.OfType<IIncomingMessageHandler>().Where(handler => handler.CanHandle(command)))
            {
                ExecutionResult validationResult;
                object? resultData;
                try
                {
                    validationResult = handler.Validate(command, data, out resultData);
                }
                catch (Exception e)
                {
                    validationResult = ExecutionResult.Failure($"Issue with message handling {e.Message}");
                    Logger.Error($"Error in command handling: {e}");
                    resultData = null;  
                }

                if (!validationResult.Successful)
                {
                    Logger.Warning("Unsuccessful execution result when handling message");
                }
            
                handler.ReportResult(resultData, validationResult);

                if (validationResult.Successful)
                {
                    Logger.Info($"CommandHandler validation successful");
                    handler.Execute(resultData);
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Issue in Handle\n  {e}");
            throw;
        }
        
    }
}