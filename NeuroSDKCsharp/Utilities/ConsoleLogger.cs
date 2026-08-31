using System.Runtime.CompilerServices;

namespace NeuroSDKCsharp.Utilities;

/// <summary>
/// This is intended as a replacement to Debug.Log from unity, if where ever you are implementing has its own logging
/// methods you should replace these.
/// </summary>
public class ConsoleLogger : BaseLogger
{
	private static readonly Dictionary<LogLevels, ConsoleColor> LevelColours = new()
	{
		{ LogLevels.Info, ConsoleColor.White },
		{ LogLevels.Warning, ConsoleColor.Yellow },
		{ LogLevels.Error, ConsoleColor.Red },
	};

	public ConsoleLogger() : base()
	{
		Instance = this;
	}

	public override void Log(string message, BaseLogger.LogLevels level)
	{
		LogSettings(level,message,true);
	}
	
	public override void Info(string message)
	{
		LogSettings(LogLevels.Info,$"INFO: {message}", true);
	}

	public override void Warning(string message)
	{
		LogSettings(LogLevels.Warning,$"WARNING: {message}", true);
	}

	public override void Error(string message)
	{
		LogSettings(LogLevels.Error,$"ERROR: {message}", true);
	}
	public override void LogSettings(BaseLogger.LogLevels level,string message, bool includeTimestamp)
	{
		DateTime timeNow = DateTime.Now;
		_ = includeTimestamp ? message = $"{timeNow.Hour}:{timeNow.Minute}:{timeNow.Second}: {message}" : message;
		
		Console.ForegroundColor = LevelColours[level];
		Console.WriteLine($"{message}");
		Console.ResetColor();
	}
}