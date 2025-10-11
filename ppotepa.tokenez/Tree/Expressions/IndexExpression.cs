using ppotepa.tokenez.Tree.Tokens.Identifiers;

namespace ppotepa.tokenez.Tree.Expressions
{
    /// <summary>
    /// Represents an array/collection index access expression.
    /// Example: numbers[5], array[i+1]
    /// </summary>
    public class IndexExpression : Expression
    {
        /// <summary>
        /// The array/collection being indexed
        /// </summary>
        public required IdentifierToken ArrayIdentifier { get; init; }

        /// <summary>
        /// The index expression (can be a literal, variable, or complex expression)
        /// </summary>
        public required Expression Index { get; init; }

        public override string ToString()
        {
            return $"{ArrayIdentifier.RawToken.Text}[{Index}]";
        }
    }
}
