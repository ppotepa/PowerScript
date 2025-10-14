using Tokenez.Core.AST.Expressions;

namespace Tokenez.Core.AST.Statements;

/// <summary>
///     Represents an IF conditional statement with optional ELSE block.
///     SQL-style syntax: IF condition { ... } or IF condition { ... } ELSE { ... }
///     Supports AND/OR operators and comparison operators (>, <, >=, <=, ==, !=)
/// </summary>
public class IfStatement(Expression condition, Scope thenScope, Scope? elseScope = null) : Statement
{
    public Expression Condition { get; } = condition;
    public Scope ThenScope { get; } = thenScope;
    public Scope? ElseScope { get; } = elseScope;

    public override string StatementType => "IF_CONDITIONAL";

    public override string ToString()
    {
        string result = $"IfStatement(Condition: {Condition}, ThenScope: {ThenScope.Name}";
        if (ElseScope != null)
        {
            result += $", ElseScope: {ElseScope.Name}";
        }

        return result + ")";
    }
}