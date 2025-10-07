using ppotepa.tokenez.Tree.Expressions;
using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree.Statements
{
    /// <summary>
    /// Represents a RETURN statement.
    /// Can return a value from a function or be void (no value).
    /// - RETURN expr - returns a value
    /// - RETURN - void return (ReturnValue is null)
    /// Terminates function execution.
    /// </summary>
    public class ReturnStatement : Statement
    {
        /// <summary>
        /// The expression being returned.
        /// Null indicates a void return (function returns nothing).
        /// </summary>
        public Expression ReturnValue { get; set; }

        public override string StatementType => "RETURN";

        public ReturnStatement(Token returnToken, Expression returnValue)
        {
            StartToken = returnToken;
            ReturnValue = returnValue;
        }
    }
}
