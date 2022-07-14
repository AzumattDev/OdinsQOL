using System;

namespace CraftyBoxes.Compatibility;

public class WILCompat
{
    protected static T InvokeMethod<T>(Type type, object instance, string methodName, object[] parameter)
    {
        return ((T)type.GetMethod(methodName)?.Invoke(instance, parameter)!)!;
    }

    protected static T? GetField<T>(Type type, object instance, string fieldName)
    {
        return (T)type.GetField(fieldName)?.GetValue(instance)!;
    }
}