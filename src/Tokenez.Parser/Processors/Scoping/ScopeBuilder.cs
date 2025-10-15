using Tokenez.Common.Logging;
using Tokenez.Core.AST;
using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Scoping;
using Tokenez.Parser.Processors.Base;

namespace Tokenez.Parser.Processors.Scoping;

/// <summary>
///     Builds scope hierarchies by processing tokens sequentially.
///     Uses a processor registry pattern to delegate token-specific logic to specialized processors.
/// </summary>
public class ScopeBuilder : IScopeBuilder
{
    private readonly ITokenProcessorRegistry _registry;

    public ScopeBuilder(ITokenProcessorRegistry registry)
    {
        _registry = registry;
    }

    /// <summary>
    ///     Builds a complete scope by processing tokens from start to end.
    ///     Each token is checked for a matching processor; if found, the processor handles it.
    /// </summary>
    public Scope BuildScope(Token startToken, Scope scope, int depth = 0)
    {
        ProcessingContext context = new(scope, depth);
        return BuildScope(startToken, scope, context);
    }

    /// <summary>
    ///     Builds a complete scope by processing tokens from start to end with an existing context.
    ///     This allows preserving context state like CycleNestingDepth across nested scopes.
    /// </summary>
    public Scope BuildScope(Token startToken, Scope scope, ProcessingContext context)
    {
        BuilderLogger.LogScopeStart(scope.ScopeName ?? "unknown", context.Depth);
        LoggerService.Logger.Debug(
            $"Entering BuildScope: {scope.ScopeName} at depth {context.Depth} with token {startToken?.GetType().Name} '{startToken?.RawToken?.Text}'");

        Token? currentToken = startToken;

        // Process all tokens sequentially until we hit a scope end token
        int safetyCounter = 0;
        while (currentToken is not null and not ScopeEndToken)
        {
            safetyCounter++;
            if (safetyCounter > 1000)
            {
                LoggerService.Logger.Error(
                    $"Exceeded 1000 iterations in BuildScope for scope {scope.ScopeName} at depth {context.Depth}. Possible endless loop.");
                break;
            }

            BuilderLogger.LogProcessing(
                currentToken.GetType().Name,
                currentToken.RawToken?.Text ?? string.Empty,
                context.Depth);
            LoggerService.Logger.Debug(
                $"Processing token: {currentToken.GetType().Name} '{currentToken.RawToken?.Text}' in scope {scope.ScopeName} at depth {context.Depth}");

            // Check if any processor can handle this token (even without expectations)
            // This allows processors like FunctionCallProcessor to handle identifiers followed by parentheses
            ITokenProcessor? processor = _registry.GetProcessor(currentToken);
            if (processor != null)
            {
                LoggerService.Logger.Debug(
                    $"Invoking processor {processor.GetType().Name} for token {currentToken.GetType().Name} '{currentToken.RawToken?.Text}'");
                // Process the token and get the next token to continue from
                TokenProcessingResult result = processor.Process(currentToken, context);

                // Update scope if processor modified it
                if (result.ModifiedScope != null)
                {
                    LoggerService.Logger.Debug(
                        $"Scope modified by processor. New scope: {result.ModifiedScope.ScopeName}");

                    // Create a nested context for the new scope
                    ProcessingContext nestedContext = context.Clone();
                    nestedContext.CurrentScope = result.ModifiedScope;

                    // If entering a function scope, mark the context
                    if (result.ModifiedScope.Type == ScopeType.Function)
                    {
                        nestedContext.EnterFunction();
                    }

                    // Recursively build the nested scope
                    BuildScope(result.NextToken, result.ModifiedScope, nestedContext);

                    // Find the end of the nested scope to continue processing
                    Token? scopeEndToken = result.NextToken;
                    int braceCount = 1;
                    while (scopeEndToken != null && braceCount > 0)
                    {
                        scopeEndToken = scopeEndToken.Next;
                        if (scopeEndToken is ScopeStartToken) braceCount++;
                        if (scopeEndToken is ScopeEndToken) braceCount--;
                    }
                    currentToken = scopeEndToken?.Next;
                    continue;
                }

                currentToken = result.NextToken;
                continue;
            }

            // No processor found, move to next token
            currentToken = currentToken.Next;
            LoggerService.Logger.Debug(
                $"Moving to next token: {currentToken?.GetType().Name} '{currentToken?.RawToken?.Text}'");
        }

        BuilderLogger.LogScopeComplete(scope.ScopeName ?? "unknown", context.Depth);
        LoggerService.Logger.Debug($"Exiting BuildScope: {scope.ScopeName} at depth {context.Depth}");

        return scope;
    }
}