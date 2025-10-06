using ppotepa.tokenez.Tree;
using ppotepa.tokenez.Tree.Exceptions;
using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Delimiters;

namespace ppotepa.tokenez.Tree.Builders
{
    internal class ScopeBuilder
    {
        private readonly TokenProcessorRegistry _registry;
        private readonly ExpectationValidator _validator;

        public ScopeBuilder(TokenProcessorRegistry registry, ExpectationValidator validator)
        {
            _registry = registry;
            _validator = validator;
        }

        public Scope BuildScope(Token startToken, Scope scope, int depth = 0)
        {
            BuilderLogger.LogScopeStart(scope.ScopeName, depth);

            var context = new ProcessingContext(scope, depth);
            var currentToken = startToken;

            while (currentToken is not null)
            {
                BuilderLogger.LogProcessing(
                    currentToken.GetType().Name,
                    currentToken.RawToken?.Text,
                    depth);

                if (HasExpectations(currentToken))
                {
                    var processor = _registry.GetProcessor(currentToken);
                    if (processor != null)
                    {
                        var result = processor.Process(currentToken, context);

                        if (result.ShouldValidateExpectations)
                        {
                            _validator.ValidateNext(currentToken);
                        }

                        if (result.ModifiedScope != null)
                        {
                            context.CurrentScope = result.ModifiedScope;
                        }

                        currentToken = result.NextToken;
                        continue;
                    }
                }

                currentToken = currentToken.Next;
            }

            BuilderLogger.LogScopeComplete(scope.ScopeName, depth);

            return scope;
        }

        private static bool HasExpectations(Token token) => token.Expectations.Length != 0;
    }
}
