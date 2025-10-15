namespace Tokenez.Compiler.Core.Variables;

/// <summary>
/// Manages variable storage and retrieval.
/// Single Responsibility: Variable lifecycle management
/// </summary>
public class VariableRegistry
{
    private readonly Dictionary<string, object> _variables;

    public VariableRegistry(Dictionary<string, object> variableStore)
    {
        _variables = variableStore ?? throw new ArgumentNullException(nameof(variableStore));
    }

    public void DeclareVariable(string name, object value)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Variable name cannot be null or empty.", nameof(name));
        }

        string upperName = name.ToUpperInvariant();
        _variables[upperName] = value;
    }

    public void UpdateVariable(string name, object value)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Variable name cannot be null or empty.", nameof(name));
        }

        string upperName = name.ToUpperInvariant();

        if (!_variables.ContainsKey(upperName))
        {
            throw new InvalidOperationException($"Variable '{name}' is not declared.");
        }

        _variables[upperName] = value;
    }

    public object GetVariable(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Variable name cannot be null or empty.", nameof(name));
        }

        string upperName = name.ToUpperInvariant();

        return _variables.TryGetValue(upperName, out object? value)
            ? value
            : throw new InvalidOperationException($"Variable '{name}' is not declared.");
    }

    public bool IsVariableDeclared(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        string upperName = name.ToUpperInvariant();
        return _variables.ContainsKey(upperName);
    }

    public void Clear()
    {
        _variables.Clear();
    }

    public void DeclareOrUpdateVariable(string name, object value)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Variable name cannot be null or empty.", nameof(name));
        }

        string upperName = name.ToUpperInvariant();
        _variables[upperName] = value;
    }
}
