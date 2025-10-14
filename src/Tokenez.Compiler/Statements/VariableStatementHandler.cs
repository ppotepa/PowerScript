using Tokenez.Common.Logging;
using Tokenez.Compiler.Core.Variables;
using Tokenez.Core.AST.Statements;

namespace Tokenez.Compiler.Statements;

/// <summary>
/// Handles variable declaration and assignment statements.
/// Single Responsibility: Variable statement processing
/// </summary>
public class VariableStatementHandler
{
    private readonly VariableRegistry _variableRegistry;
    private readonly Func<object, object> _evaluateExpression;

    public VariableStatementHandler(VariableRegistry variableRegistry, Func<object, object> evaluateExpression)
    {
        _variableRegistry = variableRegistry ?? throw new ArgumentNullException(nameof(variableRegistry));
        _evaluateExpression = evaluateExpression ?? throw new ArgumentNullException(nameof(evaluateExpression));
    }

    public void ExecuteVariableDeclaration(VariableDeclarationStatement variableStatement)
    {
        if (variableStatement == null)
        {
            throw new ArgumentNullException(nameof(variableStatement));
        }

        string variableName = GetVariableName(variableStatement);
        object value = _evaluateExpression(variableStatement.Expression);

        if (_variableRegistry.IsVariableDeclared(variableName))
        {
            _variableRegistry.UpdateVariable(variableName, value);
            LoggerService.Logger.Debug($"[EXEC] VAR UPDATE: {variableName} = {value}");
        }
        else
        {
            _variableRegistry.DeclareVariable(variableName, value);
            LoggerService.Logger.Debug($"[EXEC] VAR DECLARE: {variableName} = {value}");
        }
    }

    private static string GetVariableName(VariableDeclarationStatement variableStatement)
    {
        if (variableStatement.VarKeyword == null)
        {
            throw new InvalidOperationException("Variable statement has no VAR keyword.");
        }

        if (variableStatement.VarKeyword.RawToken == null)
        {
            throw new InvalidOperationException("VAR keyword has no raw token.");
        }

        string? variableName = variableStatement.VarKeyword.RawToken.Text;

        return string.IsNullOrWhiteSpace(variableName)
            ? throw new InvalidOperationException("Variable name is empty or whitespace.")
            : variableName;
    }
}
