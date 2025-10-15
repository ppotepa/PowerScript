using PowerScript.Core.Syntax.Tokens.Base;

namespace PowerScript.Core.AST;

/// <summary>
///     Represents a function declaration.
///     Example: "FUNCTION add(a, b) { ... }" or "FUNCTION multiply(INT[8] A, INT[8] B)[INT[16]] { ... }"
/// </summary>
public class FunctionDeclaration(Token identifier) : Declaration(identifier)
{
    /// <summary>List of parameter declarations for this function</summary>
    public List<Declaration> Parameters { get; set; } = [];

    /// <summary>The scope (body) of this function</summary>
    public Scope Scope { get; set; } = default!;

    /// <summary>The return type token (e.g., INT), or null for void functions</summary>
    public Token? ReturnType { get; set; }

    /// <summary>Optional bit-width specification for the return type (e.g., INT[8], NUMBER[16])</summary>
    public int? ReturnTypeBitWidth { get; set; }
}