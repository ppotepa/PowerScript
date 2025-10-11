using ppotepa.tokenez.Tree.Tokens.Values;

namespace ppotepa.tokenez.Tree.Expressions
{
    /// <summary>
    /// Represents an array creation expression.
    /// Syntax: CHAIN <size>
    /// Example: FLEX arr = CHAIN 10
    /// </summary>
    public class ArrayCreationExpression : Expression
    {
        public ValueToken SizeToken { get; }

        public ArrayCreationExpression(ValueToken sizeToken)
        {
            SizeToken = sizeToken;
        }
    }
}
