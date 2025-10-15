using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Interfaces;
using PowerScript.Core.Syntax.Tokens.Keywords.Types;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Keywords;

/// <summary>
///     Token representing the VAR keyword.
///     Used to declare variables with optional type inference or explicit typing.
///     Examples:
///     - "VAR x = 10" - declares variable x with inferred type
///     - "VAR INT x = 10" - declares variable x with explicit INT type
/// </summary>
public class VarKeywordToken : Token, IKeyWordToken
{
    public VarKeywordToken()
    {
    }

    public VarKeywordToken(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After VAR, expect either a type token (INT) or an identifier (variable name)</summary>
    public override Type[] Expectations => [typeof(IdentifierToken), typeof(IntToken)];

    public override string KeyWord => "VAR";
}