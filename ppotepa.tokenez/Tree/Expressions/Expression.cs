using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree.Expressions
{
    /// <summary>
    ///     Base class for all expressions in the language.
    ///     Expressions evaluate to values (literals, identifiers, operations, function calls, etc.).
    /// </summary>
    public abstract class Expression
    {
        /// <summary>The token that starts this expression</summary>
        public Token? StartToken { get; set; }

        /// <summary>String identifier for the expression type</summary>
        public virtual string ExpressionType { get; set; } = "Expression";
    }
}