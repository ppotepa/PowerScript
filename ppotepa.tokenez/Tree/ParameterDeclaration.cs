using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree
{
    /// <summary>
    ///     Represents a parameter declaration in a function signature.
    ///     Example: In "FUNCTION add(NUMBER a, NUMBER b)", "NUMBER a" is a ParameterDeclaration.
    /// </summary>
    public class ParameterDeclaration(Token type, Token identifier) : Declaration(identifier)
    {
        /// <summary>The type token (e.g., NUMBER, STRING)</summary>
        public Token DeclarativeType { get; } = type;
    }
}