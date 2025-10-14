using Tokenez.Core.Syntax.Tokens.Base;

namespace Tokenez.Core.AST.Expressions
{
    /// <summary>
    ///     Represents a logical operation expression using AND/OR operators.
    ///     SQL-style: a > b AND c < d, x== y OR z != w
    /// </summary>
    public class LogicalExpression : Expression
    {
        public LogicalExpression(Expression left, Token operatorToken, Expression right)
        {
            StartToken = left.StartToken;
            Left = left;
            Operator = operatorToken;
            Right = right;
        }

        /// <summary>Left-hand side expression (condition)</summary>
        public Expression Left { get; set; }

        /// <summary>The logical operator (AND, OR)</summary>
        public Token Operator { get; set; }

        /// <summary>Right-hand side expression (condition)</summary>
        public Expression Right { get; set; }

        public override string ExpressionType { get; set; } = "LogicalOperation";

        public override string ToString()
        {
            return $"LogicalExpression({Left} {Operator} {Right})";
        }
    }
}