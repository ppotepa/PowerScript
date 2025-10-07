using ppotepa.tokenez.Tree;
using ppotepa.tokenez.Tree.Exceptions;
using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Delimiters;

namespace ppotepa.tokenez.Tree.Builders
{
    /// <summary>
    /// Builds scope hierarchies by processing tokens sequentially.
    /// Uses a processor registry pattern to delegate token-specific logic to specialized processors.
    /// </summary>
    internal class ScopeBuilder
    {
        private readonly TokenProcessorRegistry _registry;
        private readonly ExpectationValidator _validator;

        public ScopeBuilder(TokenProcessorRegistry registry, ExpectationValidator validator)
        {
            _registry = registry;
            _validator = validator;
        }

        /// <summary>
        /// Builds a complete scope by processing tokens from start to end.
        /// Each token is checked for a matching processor; if found, the processor handles it.
        /// </summary>
        public Scope BuildScope(Token startToken, Scope scope, int depth = 0)
        {
            BuilderLogger.LogScopeStart(scope.ScopeName, depth);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[DEBUG] Entering BuildScope: {scope.ScopeName} at depth {depth} with token {startToken?.GetType().Name} '{startToken?.RawToken?.Text}'");
            Console.ResetColor();

            var context = new ProcessingContext(scope, depth);
            var currentToken = startToken;

            // Process all tokens sequentially
            int safetyCounter = 0;
            while (currentToken is not null)
            {
                safetyCounter++;
                if (safetyCounter > 1000)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[ERROR] Exceeded 1000 iterations in BuildScope for scope {scope.ScopeName} at depth {depth}. Possible endless loop.");
                    Console.ResetColor();
                    break;
                }
                BuilderLogger.LogProcessing(
                    currentToken.GetType().Name,
                    currentToken.RawToken?.Text,
                    depth);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"[DEBUG] Processing token: {currentToken.GetType().Name} '{currentToken.RawToken?.Text}' in scope {scope.ScopeName} at depth {depth}");
                Console.ResetColor();

                // Only process tokens that have declared expectations
                if (HasExpectations(currentToken))
                {
                    // Find a processor that can handle this token type
                    var processor = _registry.GetProcessor(currentToken);
                    if (processor != null)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine($"[DEBUG] Invoking processor {processor.GetType().Name} for token {currentToken.GetType().Name} '{currentToken.RawToken?.Text}'");
                        Console.ResetColor();
                        // Process the token and get the next token to continue from
                        var result = processor.Process(currentToken, context);

                        // Optionally validate that the next token meets expectations
                        if (result.ShouldValidateExpectations)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.WriteLine($"[DEBUG] Validating expectations after processing {currentToken.GetType().Name} '{currentToken.RawToken?.Text}'");
                            Console.ResetColor();
                            _validator.ValidateNext(currentToken);
                        }

                        // Update scope if processor modified it
                        if (result.ModifiedScope != null)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.WriteLine($"[DEBUG] Scope modified by processor. New scope: {result.ModifiedScope.ScopeName}");
                            Console.ResetColor();
                            context.CurrentScope = result.ModifiedScope;
                        }

                        currentToken = result.NextToken;
                        continue;
                    }
                }

                // No processor found, move to next token
                currentToken = currentToken.Next;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"[DEBUG] Moving to next token: {currentToken?.GetType().Name} '{currentToken?.RawToken?.Text}'");
                Console.ResetColor();
            }

            BuilderLogger.LogScopeComplete(scope.ScopeName, depth);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[DEBUG] Exiting BuildScope: {scope.ScopeName} at depth {depth}");
            Console.ResetColor();

            return scope;
        }

        /// <summary>
        /// Checks if a token has any expectations defined.
        /// Tokens without expectations are typically identifiers or values that don't require special processing.
        /// </summary>
        private static bool HasExpectations(Token token) => token.Expectations.Length != 0;
    }
}
