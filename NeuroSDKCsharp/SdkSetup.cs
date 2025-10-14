using Microsoft.Xna.Framework;
using NeuroSDKCsharp.Actions;
using NeuroSDKCsharp.Utilities;
using NeuroSDKCsharp.Websocket;

namespace NeuroSDKCsharp;

public static class SdkSetup
{
    public static void Initialize(Game gameClass,string game,string uriString)
    {
	    TaskDispatcher.Initialize();
        WebsocketHandler ws = new WebsocketHandler(gameClass,game,uriString);
        gameClass.Components.Add(ws);
	    
	    ExitApplicationEvent.Initialize();
	    ExitApplicationEvent.ApplicationExiting += NeuroActionHandler.OnApplicationQuit;
    }
}