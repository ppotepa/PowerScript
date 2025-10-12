using ppotepa.tokenez.Tree.Expressions;

namespace ppotepa.tokenez.Tree.Statements
{
    /// <summary>
    ///     Represents an IF conditional statement with optional ELSE block.
    ///     SQL-style syntax: IF condition { ... } or IF condition { ... } ELSE { ... }
    ///     Supports AND/OR operators and comparison operators (>, <, >=, <=, ==, !=)
    /// </summary>
    public class IfStatement : Statement
    {
        public IfStatement(Expression condition, Scope thenScope, Scope? elseScope = null)
        {
            Condition = condition;
            ThenScope = thenScope;
            ElseScope = elseScope;
        }

        public Expression Condition { get; }
        public Scope ThenScope { get; }
        public Scope? ElseScope { get; }

        public override string StatementType => "IF_CONDITIONAL";

        public override string ToString()
        {
            var result = $"IfStatement(Condition: {Condition}, ThenScope: {ThenScope.Name}";
            if (ElseScope != null) result += $", ElseScope: {ElseScope.Name}";
            return result + ")";
        }
    }
}