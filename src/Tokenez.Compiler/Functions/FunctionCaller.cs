using Tokenez.Common.Logging;
using Tokenez.Compiler.Core;
using Tokenez.Core.AST;
using Tokenez.Core.AST.Expressions;

namespace Tokenez.Compiler.Functions;

/// <summary>
/// Handles function invocation including recursion tracking.
/// Single Responsibility: Function call execution
/// </summary>
public class FunctionCaller
{
    private readonly CompilerContext _context;
    private readonly FunctionRegistry _functionRegistry;
    private readonly FunctionCompiler _functionCompiler;
    private readonly Func<object, object> _evaluateExpression;
    private readonly Dictionary<string, Delegate> _compiledFunctions = new Dictionary<string, Delegate>(StringComparer.OrdinalIgnoreCase);

    public FunctionCaller(
        CompilerContext context,
        FunctionRegistry functionRegistry,
        FunctionCompiler functionCompiler,
        Func<object, object> evaluateExpression)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _functionRegistry = functionRegistry ?? throw new ArgumentNullException(nameof(functionRegistry));
        _functionCompiler = functionCompiler ?? throw new ArgumentNullException(nameof(functionCompiler));
        _evaluateExpression = evaluateExpression ?? throw new ArgumentNullException(nameof(evaluateExpression));
    }

    public object CallFunction(FunctionCallExpression functionCall)
    {
        if (functionCall == null)
        {
            throw new ArgumentNullException(nameof(functionCall));
        }

        string functionName = GetFunctionName(functionCall);

        if (!_functionRegistry.IsFunctionDeclared(functionName))
        {
            throw new InvalidOperationException($"Function '{functionName}' is not declared");
        }

        _context.IncrementRecursion(functionName);

        try
        {
            object result = ExecuteFunctionCall(functionName, functionCall);
            LoggerService.Logger.Debug($"[FUNC] Function {functionName} returned: {result}");
            return result;
        }
        finally
        {
            _context.DecrementRecursion();
        }
    }

    private object ExecuteFunctionCall(string functionName, FunctionCallExpression functionCall)
    {
        Delegate compiledFunction = GetOrCompileFunction(functionName);
        LoggerService.Logger.Debug($"[FUNC] Compiled function type: {compiledFunction.GetType().Name}");

        object[] arguments = EvaluateArguments(functionCall);

        int argumentCount = arguments.Length;
        LoggerService.Logger.Debug($"[FUNC] Evaluated {argumentCount} arguments for function '{functionName}'");

        if (argumentCount == 0)
        {
            return InvokeFunctionWithNoArguments(compiledFunction);
        }

        if (argumentCount == 1)
        {
            return InvokeFunctionWithOneArgument(compiledFunction, arguments);
        }

        if (argumentCount == 2)
        {
            return InvokeFunctionWithTwoArguments(compiledFunction, arguments);
        }

        if (argumentCount == 3)
        {
            return InvokeFunctionWithThreeArguments(compiledFunction, arguments);
        }

        throw new InvalidOperationException($"Functions with {argumentCount} arguments are not supported");
    }

    private Delegate GetOrCompileFunction(string functionName)
    {
        if (_compiledFunctions.TryGetValue(functionName, out Delegate? cached))
        {
            return cached;
        }

        Declaration functionDeclaration = _functionRegistry.GetFunction(functionName);
        Delegate compiled = _functionCompiler.CompileFunction(functionDeclaration);

        _compiledFunctions[functionName] = compiled;

        return compiled;
    }

    private object[] EvaluateArguments(FunctionCallExpression functionCall)
    {
        if (functionCall.Arguments == null)
        {
            LoggerService.Logger.Debug("[FUNC] Arguments is null, returning empty array");
            return Array.Empty<object>();
        }

        if (functionCall.Arguments.Count == 0)
        {
            LoggerService.Logger.Debug("[FUNC] Arguments.Count is 0, returning empty array");
            return Array.Empty<object>();
        }

        LoggerService.Logger.Debug($"[FUNC] Evaluating {functionCall.Arguments.Count} argument(s)");

        object[] evaluated = new object[functionCall.Arguments.Count];

        for (int i = 0; i < functionCall.Arguments.Count; i++)
        {
            evaluated[i] = _evaluateExpression(functionCall.Arguments[i]);
            LoggerService.Logger.Debug($"[FUNC] Argument {i}: {evaluated[i]}");
        }

        return evaluated;
    }

    private static object InvokeFunctionWithNoArguments(Delegate function)
    {
        if (function is Func<object?> func0)
        {
            object? result = func0();
            return result ?? new object();
        }

        // Fallback: Try dynamic invocation
        try
        {
            object? result = function.DynamicInvoke();
            return result ?? new object();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Function signature does not match expected delegate type. Expected Func<object?>, got {function.GetType().Name}", ex);
        }
    }

    private static object InvokeFunctionWithOneArgument(Delegate function, object[] arguments)
    {
        if (function is Func<object, object?> func1)
        {
            object? result = func1(arguments[0]);
            return result ?? new object();
        }

        // Fallback: Try dynamic invocation
        try
        {
            object? result = function.DynamicInvoke(arguments[0]);
            return result ?? new object();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Function signature does not match expected delegate type. Expected Func<object, object?>, got {function.GetType().Name}", ex);
        }
    }

    private static object InvokeFunctionWithTwoArguments(Delegate function, object[] arguments)
    {
        if (function is Func<object, object, object?> func2)
        {
            object? result = func2(arguments[0], arguments[1]);
            return result ?? new object();
        }

        // Fallback: Try dynamic invocation
        try
        {
            object? result = function.DynamicInvoke(arguments[0], arguments[1]);
            return result ?? new object();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Function signature does not match expected delegate type. Expected Func<object, object, object?>, got {function.GetType().Name}", ex);
        }
    }

    private static object InvokeFunctionWithThreeArguments(Delegate function, object[] arguments)
    {
        if (function is Func<object, object, object, object?> func3)
        {
            object? result = func3(arguments[0], arguments[1], arguments[2]);
            return result ?? new object();
        }

        // Fallback: Try dynamic invocation
        try
        {
            object? result = function.DynamicInvoke(arguments[0], arguments[1], arguments[2]);
            return result ?? new object();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Function signature does not match expected delegate type. Expected Func<object, object, object, object?>, got {function.GetType().Name}", ex);
        }
    }

    private static string GetFunctionName(FunctionCallExpression functionCall)
    {
        if (functionCall.FunctionName == null)
        {
            throw new InvalidOperationException("Function call has no function name");
        }

        if (functionCall.FunctionName.RawToken == null)
        {
            throw new InvalidOperationException("Function name has no raw token");
        }

        string? functionName = functionCall.FunctionName.RawToken.Text;

        if (string.IsNullOrWhiteSpace(functionName))
        {
            throw new InvalidOperationException("Function name is empty or whitespace");
        }

        return functionName;
    }

    public void ClearCompiledFunctions()
    {
        _compiledFunctions.Clear();
        LoggerService.Logger.Debug("[FUNC] Compiled functions cache cleared");
    }
}
