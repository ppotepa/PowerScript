using PowerScript.Core.AST.Expressions;

namespace PowerScript.Core.AST.Statements;

/// <summary>
///     Represents an array element assignment statement.
///     Syntax: FLEX array[index] = value
///     Example: FLEX arr[0] = 10, FLEX matrix[0][1] = 5 (2D)
/// </summary>
public class ArrayAssignmentStatement(
    IndexExpression indexExpression,
    Expression valueExpression)
    : Statement
{
    public override string StatementType => "ARRAY_ASSIGNMENT";

    public IndexExpression IndexExpression { get; } = indexExpression;
    public Expression ValueExpression { get; } = valueExpression;
}