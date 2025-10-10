namespace NeuroSDKCsharp.Utilities;

public static class ExitApplicationEvent
{
	private static bool _initialized;

	/// <summary>
	/// This gets ran before the application exits
	/// </summary>
	public static event EventHandler? ApplicationExiting;

	public static void Initialize()
	{
		if (_initialized) return;
		_initialized = true;

		AppDomain.CurrentDomain.ProcessExit += (_, _) => ApplicationExiting?.Invoke(null,EventArgs.Empty);
	}
}
