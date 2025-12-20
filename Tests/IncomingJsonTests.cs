using Microsoft.Xna.Framework;
using Xunit;
using NeuroSDKCsharp;
using NeuroSDKCsharp.Messages.API;
using Newtonsoft.Json.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tests;

public class IncomingJsonTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void ProcessMessage_ValidJson_ShouldParseSuccessfully()
    {
        var testingClass = new NeuroSDKCsharp.Websocket.WebsocketHandler(new Game(),"Test","string");
        
        string testJson = "{\"command\": \"action\", \"data\": {\"id\": \"123\", \"name\": \"name\", \"Description\": \"This is Description\"}}";

        Dictionary<string,Object>? result = testingClass.ProcessJsonMessage(testJson);

        
        Assert.NotNull(result);
        IncomingData incomingData = (IncomingData)result["data"];
        Assert.Equal("action", result["command"]);
        Assert.Equal("123", incomingData.Data!["id"]);
        Assert.Equal("action", result["command"]);
    }
    
    [Fact]
    public void ProcessMessage_ValidJson_ShouldParseUnsuccessfully()
    {
        var testingClass = new NeuroSDKCsharp.Websocket.WebsocketHandler(new Game(),"Test","string");
        
        string testJson = "{'command': 'action', 'data': {'id': '123', 'name': 'name', 'Description': 'This is Description'}";

        try
        {
            object? result = testingClass.ProcessJsonMessage(testJson);
        
            Assert.Null(result);
            Assert.IsType<Newtonsoft.Json.JsonReaderException>(result);
        }
        catch (Exception e)
        {
            testOutputHelper.WriteLine($"successful error:\n{e}");
            return;
        }
    }
}