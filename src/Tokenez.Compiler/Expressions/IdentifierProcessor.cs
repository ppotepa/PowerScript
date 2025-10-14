using Tokenez.Compiler.Core.Variables;
using Tokenez.Core.AST.Expressions;

namespace Tokenez.Compiler.Expressions;

/// <summary>
/// Processes identifier expressions (variable references).
/// Single Responsibility: Variable name resolution and array indexing
/// </summary>
public class IdentifierProcessor
{
    private readonly VariableRegistry _variableRegistry;
    private readonly ExpressionEvaluator _expressionEvaluator;

    public IdentifierProcessor(VariableRegistry variableRegistry, ExpressionEvaluator expressionEvaluator)
    {
        _variableRegistry = variableRegistry ?? throw new ArgumentNullException(nameof(variableRegistry));
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
    }

    public object GetIdentifierValue(IdentifierExpression identifier)
    {
        if (identifier == null)
        {
            throw new ArgumentNullException(nameof(identifier));
        }

        string variableName = GetVariableName(identifier);

        if (!_variableRegistry.IsVariableDeclared(variableName))
        {
            throw new InvalidOperationException($"Variable '{variableName}' is not declared.");
        }

        object value = _variableRegistry.GetVariable(variableName);

        return identifier.ArrayIndex != null ? _expressionEvaluator.GetArrayElement(value, identifier.ArrayIndex) : value;
    }

    private static string GetVariableName(IdentifierExpression identifier)
    {
        if (identifier.Identifier != null)
        {
            string? name = identifier.Identifier.RawToken?.Text;

            return string.IsNullOrWhiteSpace(name) ? throw new InvalidOperationException("Identifier has no valid name.") : name;
        }

        throw new InvalidOperationException("Identifier expression has no identifier token.");
    }
}
