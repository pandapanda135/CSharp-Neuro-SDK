using NeuroSDKCsharp.Actions;
using NeuroSDKCsharp.Extensions.VoiceChat;
using NeuroSDKCsharp.Utilities;
using NeuroSDKCsharp.Websocket;

namespace NeuroSDKCsharp;

public static class SdkSetup
{
	/// <summary>
	/// Initialise the SDK, this will start the connection and send the startup message 
	/// </summary>
	/// <param name="game">The name of the current game, this will be sent in the startup message.</param>
	/// <param name="uriString">
	/// The uri to start the connection on, if this is null or empty the system's environment variables will be used.
	/// According to the Neuro-SDK documentation, using the environment variables is preferred.
	/// </param>
	public static void Initialize(string game, string uriString)
	{
		if (LogHolder.Logger is null)
		{
			LogHolder.SetLogger(new ConsoleLogger());
		}
		
		TaskDispatcher.Initialize();
		_ = new WebsocketHandler(game, uriString);

		ExitApplicationEvent.Initialize();
		ExitApplicationEvent.ApplicationExiting += NeuroActionHandler.OnApplicationQuit;
	}

	public static void ConnectVoiceChat(string uriString = "")
	{
		if (WebsocketHandler.Instance is null)
		{
			throw new Exception("Websocket handler not initialized");
		}
		
		WebsocketHandler instance = WebsocketHandler.Instance;
		_ = new NeuroVoiceChat(instance.GameName, uriString);
	}

	/// <summary>
	/// Replace the default Console.WriteLine based logging approach with your own. This should be called before initializing the base SDK via Initialize.  
	/// </summary>
	public static void InitializeLogger(BaseLogger logger)
	{
		LogHolder.Logger = logger;
	}
}