using Tokenez.Core.Syntax.Tokens.Base;

namespace Tokenez.Core.AST.Expressions
{
    /// <summary>
    ///     Represents a function call expression.
    ///     Example: ADD(5, 3) or PRINT("Hello")
    /// </summary>
    public class FunctionCallExpression : Expression
    {
        public FunctionCallExpression()
        {
            ExpressionType = "FunctionCall";
            FunctionName = null!; // Will be set by initializer
        }

        public required Token FunctionName { get; set; }
        public List<Expression> Arguments { get; set; } = [];

        public override string ToString()
        {
            var args = string.Join(", ", Arguments.Select(a => a.ToString()));
            return $"{FunctionName.RawToken.Text}({args})";
        }
    }
}