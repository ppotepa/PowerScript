using System.Reflection;
using Tokenez.Common.Logging;
using Tokenez.Core.AST.Statements;

namespace Tokenez.Compiler.Integration;

/// <summary>
/// Handles .NET method invocation (NET::Type::Method calls).
/// Single Responsibility: .NET method call execution
/// </summary>
public class NetMethodCallHandler
{
    private readonly Func<object, object> _evaluateExpression;

    public NetMethodCallHandler(Func<object, object> evaluateExpression)
    {
        _evaluateExpression = evaluateExpression ?? throw new ArgumentNullException(nameof(evaluateExpression));
    }

    public object ExecuteNetMethodCall(NetMethodCallStatement netCall)
    {
        if (netCall == null)
        {
            throw new ArgumentNullException(nameof(netCall));
        }

        string fullPath = netCall.FullMethodPath;

        LoggerService.Logger.Debug($"[NET] Calling {fullPath}");

        // Parse the full path into type and method
        int lastDot = fullPath.LastIndexOf('.');

        if (lastDot < 0)
        {
            throw new InvalidOperationException($"Invalid NET method path: {fullPath}. Expected format: Type.Method");
        }

        string typeName = fullPath.Substring(0, lastDot);
        string methodName = fullPath.Substring(lastDot + 1);

        Type type = ResolveType(typeName);
        object[] arguments = EvaluateArguments(netCall);

        object result = InvokeMethod(type, methodName, arguments);

        LoggerService.Logger.Debug($"[NET] {fullPath} returned: {result}");

        return result;
    }

    private object[] EvaluateArguments(NetMethodCallStatement netCall)
    {
        if (netCall.Arguments == null)
        {
            return Array.Empty<object>();
        }

        if (netCall.Arguments.Count == 0)
        {
            return Array.Empty<object>();
        }

        object[] evaluated = new object[netCall.Arguments.Count];

        for (int i = 0; i < netCall.Arguments.Count; i++)
        {
            evaluated[i] = _evaluateExpression(netCall.Arguments[i]);
        }

        return evaluated;
    }

    private static Type ResolveType(string typeName)
    {
        Type? type = Type.GetType(typeName);

        if (type != null)
        {
            return type;
        }

        type = FindTypeInLoadedAssemblies(typeName);

        if (type != null)
        {
            return type;
        }

        throw new InvalidOperationException($"Type '{typeName}' not found");
    }

    private static Type? FindTypeInLoadedAssemblies(string typeName)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (Assembly assembly in assemblies)
        {
            Type? type = assembly.GetType(typeName);

            if (type != null)
            {
                return type;
            }
        }

        return null;
    }

    private static object InvokeMethod(Type type, string methodName, object[] arguments)
    {
        MethodInfo? method = FindMethod(type, methodName, arguments);

        if (method == null)
        {
            string signature = GetMethodSignature(methodName, arguments);
            throw new InvalidOperationException($"Method '{signature}' not found on type '{type.FullName}'");
        }

        if (!method.IsStatic)
        {
            throw new InvalidOperationException($"Method '{methodName}' on type '{type.FullName}' is not static. Only static methods are supported.");
        }

        try
        {
            object? result = method.Invoke(null, arguments);
            return result ?? new object();
        }
        catch (TargetInvocationException ex)
        {
            if (ex.InnerException != null)
            {
                throw new InvalidOperationException($"Error invoking {type.FullName}::{methodName}: {ex.InnerException.Message}", ex.InnerException);
            }

            throw;
        }
    }

    private static MethodInfo? FindMethod(Type type, string methodName, object[] arguments)
    {
        MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);

        foreach (MethodInfo method in methods)
        {
            if (!string.Equals(method.Name, methodName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            ParameterInfo[] parameters = method.GetParameters();

            if (parameters.Length != arguments.Length)
            {
                continue;
            }

            if (AreArgumentsCompatible(parameters, arguments))
            {
                return method;
            }
        }

        return null;
    }

    private static bool AreArgumentsCompatible(ParameterInfo[] parameters, object[] arguments)
    {
        for (int i = 0; i < parameters.Length; i++)
        {
            Type parameterType = parameters[i].ParameterType;
            object argument = arguments[i];

            if (argument == null)
            {
                if (parameterType.IsValueType && Nullable.GetUnderlyingType(parameterType) == null)
                {
                    return false;
                }
                continue;
            }

            Type argumentType = argument.GetType();

            if (!parameterType.IsAssignableFrom(argumentType))
            {
                return false;
            }
        }

        return true;
    }

    private static string GetMethodSignature(string methodName, object[] arguments)
    {
        string[] typeNames = new string[arguments.Length];

        for (int i = 0; i < arguments.Length; i++)
        {
            if (arguments[i] == null)
            {
                typeNames[i] = "null";
            }
            else
            {
                typeNames[i] = arguments[i].GetType().Name;
            }
        }

        return $"{methodName}({string.Join(", ", typeNames)})";
    }
}
