using ppotepa.tokenez.Tree.Expressions;
using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree
{
    /// <summary>
    ///     Represents a variable declaration.
    ///     Example: "VAR x = 10" or "VAR INT x = 10"
    /// </summary>
    public class VariableDeclaration : Declaration
    {
        public VariableDeclaration(Token identifier) : base(identifier)
        {
        }

        public VariableDeclaration(Token type, Token identifier) : base(identifier)
        {
            DeclarativeType = type;
        }

        /// <summary>The type token (e.g., INT), or null for inferred types</summary>
        public Token? DeclarativeType { get; }

        /// <summary>The initial value expression assigned to this variable</summary>
        public Expression? InitialValue { get; set; }
    }
}