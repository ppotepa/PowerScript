using Tokenez.Core.AST.Expressions;
using Tokenez.Core.Syntax.Tokens.Identifiers;

namespace Tokenez.Core.AST.Statements;

/// <summary>
///     Represents an array element assignment statement.
///     Syntax: FLEX array[index] = value
///     Example: FLEX arr[0] = 10
/// </summary>
public class ArrayAssignmentStatement(
    IdentifierToken arrayIdentifier,
    Expression indexExpression,
    Expression valueExpression)
    : Statement
{
    public override string StatementType => "ARRAY_ASSIGNMENT";

    public IdentifierToken ArrayIdentifier { get; } = arrayIdentifier;
    public Expression IndexExpression { get; } = indexExpression;
    public Expression ValueExpression { get; } = valueExpression;
}