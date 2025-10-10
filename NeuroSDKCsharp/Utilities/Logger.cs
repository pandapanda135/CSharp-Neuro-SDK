namespace NeuroSDKCsharp.Utilities;

/// <summary>
/// This is intended as a replacement to Debug.Log from unity, if where ever you are implementing has its own logging
/// methods you should replace these.
/// </summary>
internal static class Logger
{
	internal enum LogLevels
	{
		Info,
		Warning,
		Error
	}
	private static readonly Dictionary<LogLevels, ConsoleColor> LevelColours = new()
	{
		{ LogLevels.Info, ConsoleColor.White },
		{ LogLevels.Warning, ConsoleColor.Yellow },
		{ LogLevels.Error, ConsoleColor.Red },
	};

	internal static void Log(string message, LogLevels level)
	{
		LogSettings(level,message,true);
	}
	
	internal static void Info(string message)
	{
		LogSettings(LogLevels.Info,$"INFO: {message}", true);
	}

	internal static void Warning(string message)
	{
		LogSettings(LogLevels.Warning,$"WARNING: {message}", true);
	}

	internal static void Error(string message)
	{
		LogSettings(LogLevels.Error,$"ERROR: {message}", true);
	}
	private static void LogSettings(LogLevels level,string message, bool includeTimestamp)
	{
		DateTime timeNow = DateTime.Now;
		_ = includeTimestamp ? message = $"{timeNow.Hour}:{timeNow.Minute}:{timeNow.Second}: {message}" : message;
		
		Console.ForegroundColor = LevelColours[level];
		Console.WriteLine($"{message}");
		Console.ResetColor();
	}
}