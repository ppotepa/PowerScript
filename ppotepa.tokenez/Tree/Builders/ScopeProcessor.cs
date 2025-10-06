using ppotepa.tokenez.Tree.Builders.Interfaces;
using ppotepa.tokenez.Tree.Exceptions;
using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Keywords;
using ppotepa.tokenez.Tree.Tokens.Scoping;

namespace ppotepa.tokenez.Tree.Builders
{
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

            if (context.CurrentScope.Type == ScopeType.Function)
            {
                context.EnterFunction();
            }

            var currentToken = scopeStartToken.Next;
            int scopeDepth = context.Depth + 1;

            while (currentToken != null && currentToken is not ScopeEndToken)
            {
                BuilderLogger.LogProcessing(
                    currentToken.GetType().Name,
                    currentToken.RawToken?.Text,
                    scopeDepth);

                var processor = _registry.GetProcessor(currentToken);
                if (processor != null)
                {
                    var result = processor.Process(currentToken, context);
                    currentToken = result.NextToken;
                    continue;
                }

                currentToken = currentToken.Next;
            }

            if (context.CurrentScope.Type == ScopeType.Function && !context.CurrentScope.HasReturn)
            {
                throw new MissingReturnStatementException(context.CurrentScope);
            }

            if (context.CurrentScope.Type == ScopeType.Function)
            {
                context.ExitFunction();
            }

            BuilderLogger.LogScopeComplete(context.CurrentScope.ScopeName, context.Depth);

            return TokenProcessingResult.Continue(currentToken?.Next);
        }
    }
}
