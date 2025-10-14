using Tokenez.Core.Syntax.Tokens.Values;

namespace Tokenez.Core.AST.Expressions
{
    /// <summary>
    ///     Represents a literal value expression (constant value like number or string).
    ///     Examples: 42, "hello", true
    /// </summary>
    public class LiteralExpression : Expression
    {
        public LiteralExpression(ValueToken value)
        {
            StartToken = value;
            Value = value;
        }

        /// <summary>The value token containing the literal</summary>
        public ValueToken Value { get; set; }

        public override string ExpressionType { get; set; } = "Literal";
    }
}