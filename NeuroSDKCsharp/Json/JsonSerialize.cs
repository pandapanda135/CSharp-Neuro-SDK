using NeuroSDKCsharp.Utilities;
using Newtonsoft.Json;

namespace NeuroSDKCsharp.Json;

internal static class JsonSerialize
{
    public static string Serialize(object? value)
    {
        Logger.Info($"inside serialize value: {value}");
        return JsonConvert.SerializeObject(value, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        });
    }
}