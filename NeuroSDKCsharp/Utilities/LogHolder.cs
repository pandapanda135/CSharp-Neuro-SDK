namespace NeuroSDKCsharp.Utilities;

public static class LogHolder
{
	public static BaseLogger? Logger;
	
	public static void SetLogger(BaseLogger logger)
	{
		Logger = logger;
	}

	public static void Log(string message, BaseLogger.LogLevels level)
	{
		Logger?.Log(message, level);
	}
	
	public static void Info(string message)
	{
		Logger?.Info(message);
	}
	
	public static void Warning(string message)
	{
		Logger?.Warning(message);
	}
	
	public static void Error(string message)
	{
		Logger?.Error(message);
	}
	
}