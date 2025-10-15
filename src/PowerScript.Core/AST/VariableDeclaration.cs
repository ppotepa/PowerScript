using PowerScript.Core.AST.Expressions;
using PowerScript.Core.Syntax.Tokens.Base;

namespace PowerScript.Core.AST;

/// <summary>
///     Represents a variable declaration.
///     Example: "VAR x = 10" or "INT x = 10" or "INT[8] small = 100"
/// </summary>
public class VariableDeclaration : Declaration
{
    public VariableDeclaration(Token identifier) : base(identifier)
    {
    }

    public VariableDeclaration(Token type, Token identifier, int? bitWidth = null) : base(identifier)
    {
        DeclarativeType = type;
        BitWidth = bitWidth;
    }

    /// <summary>The type token (e.g., INT), or null for inferred types</summary>
    public Token? DeclarativeType { get; }

    /// <summary>Optional bit-width specification for numeric types (e.g., INT[8] has BitWidth = 8)</summary>
    public int? BitWidth { get; }

    /// <summary>The initial value expression assigned to this variable</summary>
    public Expression? InitialValue { get; set; }
}