using Tokenez.Core.Syntax.Tokens.Base;

namespace Tokenez.Core.AST;

/// <summary>
///     Base class for all declarations (functions, parameters, variables).
///     Declarations define new named entities that can be referenced later.
/// </summary>
public abstract class Declaration(Token identifier)
{
    /// <summary>The identifier token containing the name of this declaration</summary>
    public Token Identifier { get; set; } = identifier;
}