using PowerScript.Core.Syntax.Tokens.Base;

namespace PowerScript.Core.AST;

/// <summary>
///     Represents a parameter declaration in a function signature.
///     Example: In "FUNCTION add(NUMBER a, NUMBER b)", "NUMBER a" is a ParameterDeclaration.
///     Supports bit-width specifications: "FUNCTION process(INT[8] byte, NUMBER[16] value)"
/// </summary>
public class ParameterDeclaration(Token type, Token identifier, int? bitWidth = null) : Declaration(identifier)
{
    /// <summary>The type token (e.g., NUMBER, STRING, INT)</summary>
    public Token DeclarativeType { get; } = type;

    /// <summary>Optional bit-width specification for numeric types (e.g., INT[8], NUMBER[16])</summary>
    public int? BitWidth { get; } = bitWidth;
}