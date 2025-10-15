using System.Linq.Expressions;
using Tokenez.Common.Logging;
using Tokenez.Compiler.Core;
using Tokenez.Core.AST;

namespace Tokenez.Compiler.Functions;

/// <summary>
/// Compiles PowerScript functions into Lambda expressions for execution.
/// Single Responsibility: Function compilation
/// </summary>
public class FunctionCompiler
{
    private readonly CompilerContext _context;
    private readonly Func<Scope, object?> _executeScope;

    public FunctionCompiler(CompilerContext context, Func<Scope, object?> executeScope)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _executeScope = executeScope ?? throw new ArgumentNullException(nameof(executeScope));
    }

    public Delegate CompileFunction(Declaration functionDeclaration)
    {
        if (functionDeclaration == null)
        {
            throw new ArgumentNullException(nameof(functionDeclaration));
        }

        string functionName = GetFunctionName(functionDeclaration);
        int parameterCount = GetParameterCount(functionDeclaration);

        LoggerService.Logger.Debug($"[FUNC] Compiling function: {functionName} with {parameterCount} parameters");

        if (parameterCount == 0)
        {
            return CompileFunctionWithNoParameters(functionDeclaration);
        }

        if (parameterCount == 1)
        {
            return CompileFunctionWithOneParameter(functionDeclaration);
        }

        if (parameterCount == 2)
        {
            return CompileFunctionWithTwoParameters(functionDeclaration);
        }

        if (parameterCount == 3)
        {
            return CompileFunctionWithThreeParameters(functionDeclaration);
        }

        throw new InvalidOperationException($"Functions with {parameterCount} parameters are not supported. Maximum is 3.");
    }

    private Func<object?> CompileFunctionWithNoParameters(Declaration functionDeclaration)
    {
        if (functionDeclaration is not FunctionDeclaration funcDecl)
        {
            throw new InvalidOperationException("Declaration is not a FunctionDeclaration");
        }

        return () =>
        {
            ResetContextForFunctionCall();
            return _executeScope(funcDecl.Scope);
        };
    }

    private Func<object, object?> CompileFunctionWithOneParameter(Declaration functionDeclaration)
    {
        if (functionDeclaration is not FunctionDeclaration funcDecl)
        {
            throw new InvalidOperationException("Declaration is not a FunctionDeclaration");
        }

        return (arg1) =>
        {
            ResetContextForFunctionCall();
            AssignParameterValues(funcDecl, new[] { arg1 });
            return _executeScope(funcDecl.Scope);
        };
    }

    private Func<object, object, object?> CompileFunctionWithTwoParameters(Declaration functionDeclaration)
    {
        if (functionDeclaration is not FunctionDeclaration funcDecl)
        {
            throw new InvalidOperationException("Declaration is not a FunctionDeclaration");
        }

        return (arg1, arg2) =>
        {
            ResetContextForFunctionCall();
            AssignParameterValues(funcDecl, new[] { arg1, arg2 });
            return _executeScope(funcDecl.Scope);
        };
    }

    private Func<object, object, object, object?> CompileFunctionWithThreeParameters(Declaration functionDeclaration)
    {
        if (functionDeclaration is not FunctionDeclaration funcDecl)
        {
            throw new InvalidOperationException("Declaration is not a FunctionDeclaration");
        }

        return (arg1, arg2, arg3) =>
        {
            ResetContextForFunctionCall();
            AssignParameterValues(funcDecl, new[] { arg1, arg2, arg3 });
            return _executeScope(funcDecl.Scope);
        };
    }

    private void ResetContextForFunctionCall()
    {
        _context.HasReturned = false;
        _context.LastReturnValue = null;
    }

    private void AssignParameterValues(FunctionDeclaration functionDeclaration, object[] arguments)
    {
        if (functionDeclaration.Parameters == null)
        {
            return;
        }

        List<string> parameterNames = GetParameterNames(functionDeclaration);

        if (parameterNames.Count != arguments.Length)
        {
            throw new InvalidOperationException(
                $"Parameter count mismatch: expected {parameterNames.Count}, got {arguments.Length}");
        }

        for (int i = 0; i < parameterNames.Count; i++)
        {
            string paramName = parameterNames[i];
            object paramValue = arguments[i];

            _context.Variables[paramName.ToUpperInvariant()] = paramValue;

            LoggerService.Logger.Debug($"[FUNC] Parameter {paramName} = {paramValue}");
        }
    }

    private static string GetFunctionName(Declaration declaration)
    {
        if (declaration.Identifier == null)
        {
            throw new InvalidOperationException("Function declaration has no identifier");
        }

        if (declaration.Identifier.RawToken == null)
        {
            throw new InvalidOperationException("Function identifier has no raw token");
        }

        string? functionName = declaration.Identifier.RawToken.Text;

        if (string.IsNullOrWhiteSpace(functionName))
        {
            throw new InvalidOperationException("Function name is empty or whitespace");
        }

        return functionName;
    }

    private static int GetParameterCount(Declaration declaration)
    {
        if (declaration is not FunctionDeclaration funcDecl)
        {
            return 0;
        }

        if (funcDecl.Parameters == null)
        {
            return 0;
        }

        return funcDecl.Parameters.Count;
    }

    private static List<string> GetParameterNames(FunctionDeclaration declaration)
    {
        List<string> names = new List<string>();

        if (declaration.Parameters == null)
        {
            return names;
        }

        foreach (var parameter in declaration.Parameters)
        {
            if (parameter == null)
            {
                continue;
            }

            if (parameter.Identifier == null)
            {
                continue;
            }

            if (parameter.Identifier.RawToken == null)
            {
                continue;
            }

            string? paramName = parameter.Identifier.RawToken.Text;

            if (string.IsNullOrWhiteSpace(paramName))
            {
                continue;
            }

            names.Add(paramName);
        }

        return names;
    }
}
