using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Parser.Lexer;

/// <summary>
/// Base class for lexical contexts that control token classification behavior.
/// Contexts are maintained in a stack to track the current parsing state and
/// determine whether custom syntax pattern keywords should be recognized.
/// </summary>
public abstract class LexicalContext
{
    /// <summary>
    /// Determines if custom syntax pattern keywords are allowed in this context.
    /// </summary>
    /// <param name="text">The text being classified</param>
    /// <returns>True if custom keywords are allowed, false otherwise</returns>
    public abstract bool AllowsCustomKeyword(string text);

    /// <summary>
    /// Gets a descriptive name for this context (for debugging/logging)
    /// </summary>
    public virtual string ContextName => GetType().Name;

    /// <summary>
    /// Determines if this context should be automatically popped after processing a token.
    /// Some contexts (like MemberAccessContext) are ephemeral and only apply to the next token.
    /// </summary>
    public virtual bool IsEphemeral => false;
}

/// <summary>
/// Default root context - allows custom keywords at the statement level.
/// This is the initial context before any special constructs are entered.
/// </summary>
public class RootContext : LexicalContext
{
    public override bool AllowsCustomKeyword(string text) => true;
}

/// <summary>
/// Context after member access operators (., ->, #).
/// Custom keywords are NEVER allowed - must be identifiers (property/method names).
/// Example: #str->Length (Length must be an identifier, not a custom keyword)
/// </summary>
public class MemberAccessContext : LexicalContext
{
    public override bool AllowsCustomKeyword(string text) => false;
    public override bool IsEphemeral => true; // Only applies to next token
}

/// <summary>
/// Context inside expressions (after =, (, [, comma, operators).
/// Custom keywords generally not allowed unless starting a new statement-like pattern.
/// Example: FLEX x = value (value must be identifier/literal, not custom keyword)
/// </summary>
public class ExpressionContext : LexicalContext
{
    public override bool AllowsCustomKeyword(string text) => false;
}

/// <summary>
/// Context at statement boundaries where custom syntax patterns are expected.
/// This is where patterns like "TAKE $count FROM $array" should be recognized.
/// </summary>
public class StatementContext : LexicalContext
{
    public override bool AllowsCustomKeyword(string text) => true;
}

/// <summary>
/// Context inside type annotations/constraints.
/// Custom keywords not allowed - must be type identifiers.
/// Example: FUNCTION foo(INT x) - INT must be a type token, not custom keyword
/// </summary>
public class TypeAnnotationContext : LexicalContext
{
    public override bool AllowsCustomKeyword(string text) => false;
    public override bool IsEphemeral => true; // Only for the type identifier
}

/// <summary>
/// Context inside array literals.
/// Custom keywords not allowed - elements must be identifiers, literals, or expressions.
/// Example: [1, 2, 3] - no custom keywords inside brackets
/// </summary>
public class ArrayLiteralContext : LexicalContext
{
    public override bool AllowsCustomKeyword(string text) => false;
}

/// <summary>
/// Context inside function/method call arguments.
/// Custom keywords not allowed unless they start a nested statement pattern.
/// Example: PRINT(value) - value must be identifier, not custom keyword
/// </summary>
public class FunctionCallContext : LexicalContext
{
    public override bool AllowsCustomKeyword(string text) => false;
}

/// <summary>
/// Context inside function body blocks.
/// Custom keywords allowed for statement-level patterns.
/// Example: FUNCTION foo() { TAKE 5 FROM array } - TAKE pattern allowed here
/// </summary>
public class BlockContext : LexicalContext
{
    public override bool AllowsCustomKeyword(string text) => true;
}

/// <summary>
/// Context after return statements.
/// Custom keywords not allowed - must return expressions/values.
/// Example: RETURN value (value must be identifier/expression, not custom keyword)
/// </summary>
public class ReturnContext : LexicalContext
{
    public override bool AllowsCustomKeyword(string text) => false;
    public override bool IsEphemeral => true; // Only for the return value expression
}

/// <summary>
/// Context inside custom syntax blocks (![...]).
/// Custom keywords ARE allowed - this is where pattern transformations happen.
/// Example: ![MAX OF numbers] - MAX and OF are custom keywords
/// </summary>
public class CustomSyntaxBlockContext : LexicalContext
{
    public override bool AllowsCustomKeyword(string text) => true;
}
