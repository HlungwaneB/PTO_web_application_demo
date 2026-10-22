using System.Text.Json;

namespace PTO_web_application.Extensions;

public static class SessionExtensions
{
    public static void SetObject<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonSerializer.Serialize(value));
    }

    public static T? GetObject<T>(this ISession session, string key)
    {
        var data = session.GetString(key);
        return data == null ? default : JsonSerializer.Deserialize<T>(data);
    }
}