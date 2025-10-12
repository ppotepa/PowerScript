using ppotepa.tokenez.Tree.Expressions;

namespace ppotepa.tokenez.Tree.Statements
{
    /// <summary>
    ///     Represents a PRINT statement that outputs text or expression values to the console.
    ///     Example: PRINT "Hello World" or PRINT ADD(5, 3)
    /// </summary>
    public class PrintStatement : Statement
    {
        public PrintStatement(Expression expression)
        {
            Expression = expression;
        }

        /// <summary>The expression to print (can be string literal, function call, variable, etc.)</summary>
        public Expression Expression { get; set; }

        public override string StatementType => "PRINT";

        public override string ToString()
        {
            return $"PRINT {Expression}";
        }
    }
}