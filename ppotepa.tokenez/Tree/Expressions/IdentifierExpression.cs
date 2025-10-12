using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Identifiers;

namespace ppotepa.tokenez.Tree.Expressions
{
    /// <summary>
    ///     Represents an identifier expression (variable or parameter reference).
    ///     References a value by name (e.g., variable 'x' or parameter 'count').
    /// </summary>
    public class IdentifierExpression : Expression
    {
        public IdentifierExpression(IdentifierToken identifier)
        {
            StartToken = identifier;
            Identifier = identifier;
        }

        /// <summary>The identifier token containing the name</summary>
        public IdentifierToken Identifier { get; set; }

        public override string ExpressionType { get; set; } = "Identifier";
    }
}