using ppotepa.tokenez.Tree.Builders.Interfaces;
using ppotepa.tokenez.Tree.Exceptions;
using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Keywords;
using ppotepa.tokenez.Tree.Tokens.Scoping;
using ppotepa.tokenez.Tree.Tokens.Identifiers;

namespace ppotepa.tokenez.Tree.Builders
{
    /// <summary>
    /// Processes scope blocks (code between { and }).
    /// Responsible for:
    /// - Processing all tokens within a scope
    /// - Validating that function scopes contain a RETURN statement
    /// - Managing function context entry/exit
    /// </summary>
    internal class ScopeProcessor : ITokenProcessor
    {
        private readonly TokenProcessorRegistry _registry;
        private readonly ExpectationValidator _validator;
        private readonly ScopeBuilder _scopeBuilder;

        public ScopeProcessor(TokenProcessorRegistry registry, ExpectationValidator validator, ScopeBuilder scopeBuilder)
        {
            _registry = registry;
            _validator = validator;
            _scopeBuilder = scopeBuilder;
        }

        public bool CanProcess(Token token)
        {
            return token is ScopeStartToken;
        }

        public TokenProcessingResult Process(Token token, ProcessingContext context)
        {
            var scopeStartToken = token as ScopeStartToken;

            // Check if this scope start follows a function declaration
            // If so, we need to find the function scope that was just created
            Scope targetScope = context.CurrentScope;

            // Look backwards to find the function that this scope belongs to
            var prevToken = scopeStartToken.Prev;
            while (prevToken != null && prevToken is not FunctionToken)
            {
                prevToken = prevToken.Prev;
            }

            if (prevToken is FunctionToken)
            {
                // Find the function name
                var functionNameToken = prevToken.Next as IdentifierToken;
                if (functionNameToken != null)
                {
                    var functionName = functionNameToken.RawToken.Text;
                    // Find the function's scope in the current scope's declarations
                    if (context.CurrentScope.Decarations.ContainsKey(functionName))
                    {
                        var declaration = context.CurrentScope.Decarations[functionName] as FunctionDeclaration;
                        if (declaration?.Scope != null)
                        {
                            targetScope = declaration.Scope;
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.WriteLine($"[DEBUG] ScopeProcessor: Switching to function scope '{targetScope.ScopeName}' for processing");
                            Console.ResetColor();
                        }
                    }
                }
            }

            BuilderLogger.LogScopeStart(targetScope.ScopeName, context.Depth);

            // Mark function context for RETURN validation
            if (targetScope.Type == ScopeType.Function)
            {
                context.EnterFunction();
            }

            var currentToken = scopeStartToken.Next;
            int scopeDepth = context.Depth + 1;

            // Create a new context for processing this scope, preserving important context state
            var scopeContext = context.Clone();
            scopeContext.CurrentScope = targetScope;
            scopeContext.Depth = scopeDepth;
            if (targetScope.Type == ScopeType.Function)
            {
                scopeContext.EnterFunction();
            }

            // Process all tokens until we hit the scope end token
            while (currentToken != null && currentToken is not ScopeEndToken)
            {
                BuilderLogger.LogProcessing(
                    currentToken.GetType().Name,
                    currentToken.RawToken?.Text,
                    scopeDepth);

                // Delegate token processing to registered processors
                var processor = _registry.GetProcessor(currentToken);
                if (processor != null)
                {
                    var result = processor.Process(currentToken, scopeContext);

                    // If the processor created a new scope (e.g., FunctionProcessor),
                    // recursively build that scope
                    if (result.ModifiedScope != null)
                    {
                        // Create a new context for the nested scope
                        var nestedContext = new ProcessingContext(result.ModifiedScope, scopeDepth);

                        // If the new scope is a function, mark it in the context
                        if (result.ModifiedScope.Type == ScopeType.Function)
                        {
                            nestedContext.EnterFunction();
                        }

                        // Build the nested scope recursively with the new context
                        _scopeBuilder.BuildScope(result.NextToken, result.ModifiedScope, scopeDepth);

                        // Find the scope end token to continue after the nested scope
                        var nestedToken = result.NextToken;
                        int braceCount = 1;
                        while (nestedToken != null && braceCount > 0)
                        {
                            nestedToken = nestedToken.Next;
                            if (nestedToken is ScopeStartToken) braceCount++;
                            if (nestedToken is ScopeEndToken) braceCount--;
                        }
                        currentToken = nestedToken?.Next;
                    }
                    else
                    {
                        currentToken = result.NextToken;
                    }
                    continue;
                }

                // No processor found, skip to next token
                currentToken = currentToken.Next;
            }

            // Enforce language rule: functions with return types must have a RETURN statement
            if (targetScope.Type == ScopeType.Function &&
                !targetScope.HasReturn &&
                targetScope.FunctionDeclaration?.ReturnType != null)
            {
                throw new MissingReturnStatementException(targetScope);
            }

            // Exit function context
            if (targetScope.Type == ScopeType.Function)
            {
                context.ExitFunction();
            }

            BuilderLogger.LogScopeComplete(targetScope.ScopeName, context.Depth);

            // Continue from token after scope end
            return TokenProcessingResult.Continue(currentToken?.Next);
        }
    }
}
