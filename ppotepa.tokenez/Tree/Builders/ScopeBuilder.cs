using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree.Builders
{
    /// <summary>
    ///     Builds scope hierarchies by processing tokens sequentially.
    ///     Uses a processor registry pattern to delegate token-specific logic to specialized processors.
    /// </summary>
    internal class ScopeBuilder(TokenProcessorRegistry registry, ExpectationValidator validator)
    {
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
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(
                $"[DEBUG] Entering BuildScope: {scope.ScopeName} at depth {context.Depth} with token {startToken?.GetType().Name} '{startToken?.RawToken?.Text}'");
            Console.ResetColor();

            Token? currentToken = startToken;

            // Process all tokens sequentially
            int safetyCounter = 0;
            while (currentToken is not null)
            {
                safetyCounter++;
                if (safetyCounter > 1000)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(
                        $"[ERROR] Exceeded 1000 iterations in BuildScope for scope {scope.ScopeName} at depth {context.Depth}. Possible endless loop.");
                    Console.ResetColor();
                    break;
                }

                BuilderLogger.LogProcessing(
                    currentToken.GetType().Name,
                    currentToken.RawToken?.Text ?? string.Empty,
                    context.Depth);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(
                    $"[DEBUG] Processing token: {currentToken.GetType().Name} '{currentToken.RawToken?.Text}' in scope {scope.ScopeName} at depth {context.Depth}");
                Console.ResetColor();

                // Check if any processor can handle this token (even without expectations)
                // This allows processors like FunctionCallProcessor to handle identifiers followed by parentheses
                Interfaces.ITokenProcessor? processor = registry.GetProcessor(currentToken);
                if (processor != null)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(
                        $"[DEBUG] Invoking processor {processor.GetType().Name} for token {currentToken.GetType().Name} '{currentToken.RawToken?.Text}'");
                    Console.ResetColor();
                    // Process the token and get the next token to continue from
                    TokenProcessingResult result = processor.Process(currentToken, context);

                    // Optionally validate that the next token meets expectations (only for tokens that have them)
                    if (result.ShouldValidateExpectations && HasExpectations(currentToken))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine(
                            $"[DEBUG] Validating expectations after processing {currentToken.GetType().Name} '{currentToken.RawToken?.Text}'");
                        Console.ResetColor();
                        validator.ValidateNext(currentToken);
                    }

                    // Update scope if processor modified it
                    if (result.ModifiedScope != null)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine(
                            $"[DEBUG] Scope modified by processor. New scope: {result.ModifiedScope.ScopeName}");
                        Console.ResetColor();
                        context.CurrentScope = result.ModifiedScope;
                    }

                    currentToken = result.NextToken;
                    continue;
                }

                // No processor found, move to next token
                currentToken = currentToken.Next;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(
                    $"[DEBUG] Moving to next token: {currentToken?.GetType().Name} '{currentToken?.RawToken?.Text}'");
                Console.ResetColor();
            }

            BuilderLogger.LogScopeComplete(scope.ScopeName ?? "unknown", context.Depth);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[DEBUG] Exiting BuildScope: {scope.ScopeName} at depth {context.Depth}");
            Console.ResetColor();

            return scope;
        }

        /// <summary>
        ///     Checks if a token has any expectations defined.
        ///     Tokens without expectations are typically identifiers or values that don't require special processing.
        /// </summary>
        private static bool HasExpectations(Token token)
        {
            return token.Expectations.Length != 0;
        }
    }
}