using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Base;

/// <summary>
///     Base class for all tokens in the tokenizer.
///     Tokens are linked in a doubly-linked list and can specify expected following tokens.
///     Each token wraps a RawToken (text + position) and provides semantic meaning.
/// </summary>
public abstract class Token
{
    private Type _type = null!;

    protected Token()
    {
        RawToken = null!;
        // Tree = null!; // Commented out to avoid circular dependency with Parser
        Next = null!;
        Prev = null!;
    }

    protected Token(RawToken rawToken)
    {
        RawToken = rawToken;
        // Tree = null!; // Commented out to avoid circular dependency with Parser
        Next = null!;
        Prev = null!;
    }

    public Type Type
    {
        get
        {
            _type ??= GetType();
            return _type;
        }
    }

    /// <summary>Position index in the token sequence</summary>
    public int Index { get; set; }

    /// <summary>Next token in the linked list</summary>
    public Token Next { get; set; }

    /// <summary>Previous token in the linked list</summary>
    public Token Prev { get; set; }

    /// <summary>
    ///     Array of token types that can follow this token.
    ///     Used by ExpectationValidator for syntax checking.
    /// </summary>
    public abstract Type[] Expectations { get; }

    /// <summary>Whether this token requires a delimiter (e.g., '(' needs ')')</summary>
    public virtual bool HasDelimiter => false;

    /// <summary>The delimiter keyword (e.g., "END", ")", "}")</summary>
    public virtual string Delimiter => "END";

    /// <summary>The underlying raw token (text + position)</summary>
    public RawToken RawToken { get; }

    // Commented out to avoid circular dependency with Parser layer
    // /// <summary>Reference to the token tree (for lookups)</summary>
    // public TokenTree Tree { get; }

    /// <summary>Keyword text for keyword tokens (FUNCTION, RETURN, etc.)</summary>
    public virtual string KeyWord => string.Empty;

    /// <summary>
    ///     Gets n previous tokens in the linked list.
    ///     Used for error context display.
    /// </summary>
    public Token[] GetPrevious(int n)
    {
        List<Token> tokens = [];
        Token current = Prev;
        for (int i = 0; i < n && current != null; i++)
        {
            tokens.Add(current);
            current = current.Prev;
        }

        return tokens.ToArray();
    }

    /// <summary>
    ///     Gets n next tokens in the linked list.
    ///     Used for error context display and lookahead.
    /// </summary>
    public Token[] GetNext(int n)
    {
        List<Token> tokens = [];
        Token current = Next;
        for (int i = 0; i < n && current != null; i++)
        {
            tokens.Add(current);
            current = current.Next;
        }

        return tokens.ToArray();
    }
}