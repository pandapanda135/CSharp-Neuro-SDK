namespace NeuroSDKCsharp.Utilities;

public abstract class BaseLogger
{
	public enum LogLevels
	{
		Info,
		Warning,
		Error
	}

	public static BaseLogger? Instance;

	public abstract void Log(string message, LogLevels level);

	public abstract void Info(string message);

	public abstract void Warning(string message);
	
	public abstract void Error(string message);

	public abstract void LogSettings(LogLevels level, string message, bool includeTimestamp);
}