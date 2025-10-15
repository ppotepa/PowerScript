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

        // Workaround: If the variable name starts and ends with quotes, it's actually a string literal
        // This happens due to parser quirk
        if (variableName.StartsWith("\"") && variableName.EndsWith("\""))
        {
            return variableName.Substring(1, variableName.Length - 2);
        }

        if (!_variableRegistry.IsVariableDeclared(variableName))
        {
            throw new InvalidOperationException($"Variable '{variableName}' is not declared.");
        }

        object value = _variableRegistry.GetVariable(variableName);

        // TODO: Array indexing support if needed
        // Current AST doesn't have ArrayIndex property

        return value;
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
