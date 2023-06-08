using System.Reflection;
using System.Text.Json;

namespace R8.RedisHelper.Tests;

internal static class Utils
{
    public static bool IsTheSameAs(this object thisObject, object secondObject)
    {
        return JsonSerializer.Serialize(thisObject) == JsonSerializer.Serialize(secondObject);
    }

    public static void SetPrivateField<T>(this T instance, string fieldName, object value) where T : class
    {
        var fieldInfo = instance.GetType()
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

        fieldInfo.SetValue(instance, value);
    }
}