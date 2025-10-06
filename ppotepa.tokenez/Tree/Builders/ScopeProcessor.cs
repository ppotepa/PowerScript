using ppotepa.tokenez.Tree.Builders.Interfaces;
using ppotepa.tokenez.Tree.Exceptions;
using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Keywords;
using ppotepa.tokenez.Tree.Tokens.Scoping;

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

        public ScopeProcessor(TokenProcessorRegistry registry, ExpectationValidator validator)
        {
            _registry = registry;
            _validator = validator;
        }

        public bool CanProcess(Token token)
        {
            return token is ScopeStartToken;
        }

        public TokenProcessingResult Process(Token token, ProcessingContext context)
        {
            var scopeStartToken = token as ScopeStartToken;
            BuilderLogger.LogScopeStart(context.CurrentScope.ScopeName, context.Depth);

            // Mark function context for RETURN validation
            if (context.CurrentScope.Type == ScopeType.Function)
            {
                context.EnterFunction();
            }

            var currentToken = scopeStartToken.Next;
            int scopeDepth = context.Depth + 1;

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
                    var result = processor.Process(currentToken, context);
                    currentToken = result.NextToken;
                    continue;
                }

                // No processor found, skip to next token
                currentToken = currentToken.Next;
            }

            // Enforce language rule: every function must have a RETURN statement
            if (context.CurrentScope.Type == ScopeType.Function && !context.CurrentScope.HasReturn)
            {
                throw new MissingReturnStatementException(context.CurrentScope);
            }

            // Exit function context
            if (context.CurrentScope.Type == ScopeType.Function)
            {
                context.ExitFunction();
            }

            BuilderLogger.LogScopeComplete(context.CurrentScope.ScopeName, context.Depth);

            // Continue from token after scope end
            return TokenProcessingResult.Continue(currentToken?.Next);
        }
    }
}
