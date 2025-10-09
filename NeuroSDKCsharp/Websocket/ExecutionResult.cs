namespace NeuroSDKCsharp.Websocket;

public class ExecutionResult
{
    public bool Successful;
    public string Message;

    public ExecutionResult(bool successful,string? message)
    {
        if (string.IsNullOrEmpty(message))
        {
            message = "";
        }

        Successful = successful;
        Message = message;
    }

    public static ExecutionResult Success(string? message = null)
    {
        return new ExecutionResult(true, message);
    }

    public static ExecutionResult Failure(string? message)
    {
        return new ExecutionResult(false, message);
    }
    
    // TODO: implement Strings to show who's fault it is 
    public static ExecutionResult ServerFailure(string? message)
    {
        return Failure(message + " Server issue");
    }
    
    public static ExecutionResult ModFailure(string? message)
    {
        return Failure(message + " Mod failure");
    }
}