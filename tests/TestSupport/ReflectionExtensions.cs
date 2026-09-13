using System.Reflection;

namespace Leagues.Tests.TestSupport;

/// <summary>
/// Reflection helpers used to observe/drive private state on view models so that
/// internal branching logic can be exercised without loosening production access modifiers
/// beyond what's already needed for the object graph to function.
/// </summary>
internal static class ReflectionExtensions
{
    public static void SetPrivateField(this object target, string fieldName, object? value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new MissingFieldException(target.GetType().FullName, fieldName);
        field.SetValue(target, value);
    }

    public static T GetPrivateField<T>(this object target, string fieldName)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new MissingFieldException(target.GetType().FullName, fieldName);
        return (T)field.GetValue(target)!;
    }

    public static void SetPrivateProperty(this object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new MissingMemberException(target.GetType().FullName, propertyName);
        property.SetValue(target, value);
    }

    public static void InvokePrivateMethod(this object target, string methodName, params object?[] arguments)
    {
        var method = target.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new MissingMethodException(target.GetType().FullName, methodName);
        method.Invoke(target, arguments);
    }
}
