using Tokenez.Runtime.Interfaces;

namespace Tokenez.Runtime.Core;

/// <summary>
/// Implementation of execution context that maintains runtime state.
/// </summary>
public class ExecutionContext : IExecutionContext
{
    private readonly Dictionary<string, object?> _variables = new();
    private readonly Dictionary<string, object> _functions = new();

    /// <summary>
    /// Gets all variables in the current scope.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Variables => _variables;

    /// <summary>
    /// Gets all registered functions.
    /// </summary>
    public IReadOnlyDictionary<string, object> Functions => _functions;

    /// <summary>
    /// Gets a variable value.
    /// </summary>
    public object? GetVariable(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Variable name cannot be null or whitespace", nameof(name));
        }

        _variables.TryGetValue(name, out var value);
        return value;
    }

    /// <summary>
    /// Sets a variable value.
    /// </summary>
    public void SetVariable(string name, object? value)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Variable name cannot be null or whitespace", nameof(name));
        }

        _variables[name] = value;
    }

    /// <summary>
    /// Checks if a variable exists.
    /// </summary>
    public bool HasVariable(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        return _variables.ContainsKey(name);
    }

    /// <summary>
    /// Registers a function.
    /// </summary>
    public void RegisterFunction(string name, object function)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Function name cannot be null or whitespace", nameof(name));
        }

        if (function == null)
        {
            throw new ArgumentNullException(nameof(function));
        }

        _functions[name] = function;
    }

    /// <summary>
    /// Checks if a function is registered.
    /// </summary>
    public bool HasFunction(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        return _functions.ContainsKey(name);
    }

    /// <summary>
    /// Gets a registered function.
    /// </summary>
    public object? GetFunction(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        _functions.TryGetValue(name, out var function);
        return function;
    }

    /// <summary>
    /// Clears all variables and resets the context.
    /// </summary>
    public void Reset()
    {
        _variables.Clear();
        _functions.Clear();
    }
}