using PowerScript.Runtime.Interfaces;

namespace PowerScript.Runtime.Core;

/// <summary>
/// Implementation of execution context that maintains runtime state with scope support.
/// </summary>
public class ExecutionContext : IExecutionContext
{
    private readonly Stack<Dictionary<string, object?>> _scopeStack = new();
    private readonly Dictionary<string, object> _functions = new();

    public ExecutionContext()
    {
        // Initialize with global scope
        _scopeStack.Push(new Dictionary<string, object?>());
    }

    /// <summary>
    /// Gets all variables in the current scope (flattened view).
    /// </summary>
    public IReadOnlyDictionary<string, object?> Variables
    {
        get
        {
            var result = new Dictionary<string, object?>();
            // Merge all scopes from bottom to top (global to current)
            foreach (var scope in _scopeStack.Reverse())
            {
                foreach (var kvp in scope)
                {
                    result[kvp.Key] = kvp.Value;
                }
            }
            return result;
        }
    }

    /// <summary>
    /// Gets all registered functions.
    /// </summary>
    public IReadOnlyDictionary<string, object> Functions => _functions;

    /// <summary>
    /// Gets a variable value by searching from current scope up to global scope.
    /// </summary>
    public object? GetVariable(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Variable name cannot be null or whitespace", nameof(name));
        }

        // Search from current scope up to global scope
        foreach (var scope in _scopeStack)
        {
            if (scope.TryGetValue(name, out var value))
            {
                return value;
            }
        }

        return null;
    }

    /// <summary>
    /// Sets a variable value. If variable exists in any parent scope, updates it there.
    /// Otherwise creates it in the current scope.
    /// Use this for assignments: x = 5
    /// </summary>
    public void SetVariable(string name, object? value)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Variable name cannot be null or whitespace", nameof(name));
        }

        // Search from current scope up to find existing variable
        foreach (var scope in _scopeStack)
        {
            if (scope.ContainsKey(name))
            {
                scope[name] = value;
                return;
            }
        }

        // Variable doesn't exist, create in current scope
        _scopeStack.Peek()[name] = value;
    }

    /// <summary>
    /// Declares a new variable in the current scope, even if it exists in parent scopes.
    /// Use this for type declarations: INT x = 5
    /// </summary>
    public void DeclareVariable(string name, object? value)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Variable name cannot be null or whitespace", nameof(name));
        }

        // Always create/update in current scope (allows shadowing)
        _scopeStack.Peek()[name] = value;
    }

    /// <summary>
    /// Checks if a variable exists in any scope.
    /// </summary>
    public bool HasVariable(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        foreach (var scope in _scopeStack)
        {
            if (scope.ContainsKey(name))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Pushes a new scope onto the stack (for IF blocks).
    /// </summary>
    public void PushScope()
    {
        _scopeStack.Push(new Dictionary<string, object?>());
    }

    /// <summary>
    /// Pops the current scope from the stack (when exiting IF block).
    /// Never pops the global scope.
    /// </summary>
    public void PopScope()
    {
        if (_scopeStack.Count > 1)
        {
            _scopeStack.Pop();
        }
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
        _scopeStack.Clear();
        _scopeStack.Push(new Dictionary<string, object?>()); // Re-initialize global scope
        _functions.Clear();
    }
}