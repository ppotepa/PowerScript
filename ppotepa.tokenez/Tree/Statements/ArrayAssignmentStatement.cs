using ppotepa.tokenez.Tree.Expressions;
using ppotepa.tokenez.Tree.Tokens.Identifiers;

namespace ppotepa.tokenez.Tree.Statements
{
    /// <summary>
    /// Represents an array element assignment statement.
    /// Syntax: FLEX array[index] = value
    /// Example: FLEX arr[0] = 10
    /// </summary>
    public class ArrayAssignmentStatement : Statement
    {
        public override string StatementType => "ARRAY_ASSIGNMENT";

        public IdentifierToken ArrayIdentifier { get; }
        public Expression IndexExpression { get; }
        public Expression ValueExpression { get; }

        public ArrayAssignmentStatement(IdentifierToken arrayIdentifier, Expression indexExpression, Expression valueExpression)
        {
            ArrayIdentifier = arrayIdentifier;
            IndexExpression = indexExpression;
            ValueExpression = valueExpression;
        }
    }
}
