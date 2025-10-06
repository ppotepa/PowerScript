using ppotepa.tokenez.Tree.Expressions;
using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree.Statements
{
    /// <summary>
    /// Represents a RETURN statement.
    /// Returns a value from a function and terminates function execution.
    /// </summary>
    public class ReturnStatement : Statement
    {
        /// <summary>The expression being returned</summary>
        public Expression ReturnValue { get; set; }

        public override string StatementType => "RETURN";

        public ReturnStatement(Token returnToken, Expression returnValue)
        {
            StartToken = returnToken;
            ReturnValue = returnValue;
        }
    }
}
