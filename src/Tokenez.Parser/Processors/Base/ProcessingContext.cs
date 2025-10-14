using Tokenez.Core.AST;
using Tokenez.Core.Syntax.Tokens.Base;

namespace Tokenez.Parser.Processors.Base;

/// <summary>
///     Context information passed through the token processing pipeline.
///     Maintains state about the current scope, depth, and function context during processing.
/// </summary>
public class ProcessingContext(Scope currentScope, int depth)
{
    /// <summary>Current scope being built</summary>
    public Scope CurrentScope { get; set; } = currentScope;

    /// <summary>Nesting depth for logging purposes</summary>
    public int Depth { get; set; } = depth;

    /// <summary>Stack for managing nested token processing (future use)</summary>
    public Stack<Token> ProcessingStack { get; set; } = new();

    /// <summary>Tracks whether we're currently inside a function scope (for RETURN validation)</summary>
    public bool IsInsideFunction { get; set; }

    /// <summary>Tracks CYCLE loop nesting depth for auto-generated variable names (A, B, C, ...)</summary>
    public int CycleNestingDepth { get; set; }

    /// <summary>Tracks parenthesis depth (future use for complex expressions)</summary>
    public int ParenthesisDepth { get; set; }

    /// <summary>
    ///     Marks that we've entered a function scope.
    ///     This enables RETURN statement validation.
    /// </summary>
    public void EnterFunction()
    {
        IsInsideFunction = true;
    }

    /// <summary>
    ///     Marks that we've exited a function scope.
    /// </summary>
    public void ExitFunction()
    {
        IsInsideFunction = false;
    }

    /// <summary>
    ///     Creates a deep copy of the context for nested processing.
    /// </summary>
    public ProcessingContext Clone()
    {
        return new ProcessingContext(CurrentScope, Depth)
        {
            IsInsideFunction = IsInsideFunction,
            CycleNestingDepth = CycleNestingDepth,
            ParenthesisDepth = ParenthesisDepth,
            ProcessingStack = new Stack<Token>(ProcessingStack)
        };
    }
}