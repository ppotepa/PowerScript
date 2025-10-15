using Tokenez.Common.Logging;
using Tokenez.Core.AST;

namespace Tokenez.Compiler.Functions;

/// <summary>
/// Manages function declarations and lookup.
/// Single Responsibility: Function registration and retrieval
/// </summary>
public class FunctionRegistry
{
    private readonly Dictionary<string, Declaration> _functions = new Dictionary<string, Declaration>(StringComparer.OrdinalIgnoreCase);

    public void RegisterFunction(Declaration functionDeclaration)
    {
        if (functionDeclaration == null)
        {
            throw new ArgumentNullException(nameof(functionDeclaration));
        }

        string functionName = GetFunctionName(functionDeclaration);

        if (_functions.ContainsKey(functionName))
        {
            LoggerService.Logger.Warning($"[FUNC] Function '{functionName}' is being redeclared");
        }

        _functions[functionName] = functionDeclaration;
        LoggerService.Logger.Debug($"[FUNC] Registered function: {functionName}");
    }

    public Declaration GetFunction(string functionName)
    {
        if (string.IsNullOrWhiteSpace(functionName))
        {
            throw new ArgumentException("Function name cannot be null or whitespace", nameof(functionName));
        }

        if (!_functions.TryGetValue(functionName, out Declaration? declaration))
        {
            throw new InvalidOperationException($"Function '{functionName}' is not declared");
        }

        return declaration;
    }

    public bool IsFunctionDeclared(string functionName)
    {
        if (string.IsNullOrWhiteSpace(functionName))
        {
            return false;
        }

        return _functions.ContainsKey(functionName);
    }

    public void Clear()
    {
        _functions.Clear();
        LoggerService.Logger.Debug("[FUNC] Function registry cleared");
    }

    public int GetFunctionCount()
    {
        return _functions.Count;
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
}
