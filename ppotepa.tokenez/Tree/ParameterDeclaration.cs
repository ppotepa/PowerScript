using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree
{
    /// <summary>
    ///     Represents a parameter declaration in a function signature.
    ///     Example: In "FUNCTION add(NUMBER a, NUMBER b)", "NUMBER a" is a ParameterDeclaration.
    /// </summary>
    public class ParameterDeclaration : Declaration
    {
        public ParameterDeclaration(Token type, Token identifier) : base(identifier)
        {
            DeclarativeType = type;
        }

        /// <summary>The type token (e.g., NUMBER, STRING)</summary>
        public Token DeclarativeType { get; }
    }
}