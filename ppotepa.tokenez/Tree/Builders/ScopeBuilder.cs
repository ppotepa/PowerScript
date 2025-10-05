using ppotepa.tokenez.Tree;
using ppotepa.tokenez.Tree.Exceptions;
using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Delimiters;
using ppotepa.tokenez.Tree.Tokens.Keywords;

namespace ppotepa.tokenez.Tree.Builders
{
    /// <summary>
    /// Responsible for building scope hierarchies from tokens
    /// </summary>
    internal class ScopeBuilder
    {
        private readonly FunctionProcessor _functionProcessor;

        public ScopeBuilder(FunctionProcessor functionProcessor)
        {
            _functionProcessor = functionProcessor;
        }

        public Scope BuildScope(Token startToken, Scope scope, int depth = 0)
        {
            BuilderLogger.LogScopeStart(scope.ScopeName, depth);

            var currentToken = startToken;
            var parenthesisDepth = 0;

            while (currentToken is not null)
            {
                BuilderLogger.LogProcessing(
                    currentToken.GetType().Name,
                    currentToken.RawToken?.Text,
                    depth);

                if (HasExpectations(currentToken))
                {
                    var nextToken = ProcessTokenWithExpectations(currentToken, scope, depth, ref parenthesisDepth);
                    if (nextToken != null)
                    {
                        currentToken = nextToken;
                        continue;
                    }
                }

                currentToken = currentToken.Next;
            }

            ValidateParenthesisBalance(currentToken, depth, parenthesisDepth);
            BuilderLogger.LogScopeComplete(scope.ScopeName, depth);

            return scope;
        }

        private Token ProcessTokenWithExpectations(Token token, Scope scope, int depth, ref int parenthesisDepth)
        {
            if (token is FunctionToken)
            {
                var result = _functionProcessor.ProcessFunction(token, scope, depth);
                parenthesisDepth += result.ParenthesisDepthChange;
                return result.NextToken;
            }

            return null;
        }

        private static bool HasExpectations(Token token) => token.Expectations.Length != 0;

        private static void ValidateParenthesisBalance(Token lastToken, int depth, int parenthesisDepth)
        {
            if (depth == 0 && parenthesisDepth > 0)
            {
                throw new UnexpectedTokenException(
                    lastToken,
                    "Unmatched parenthesis detected",
                    typeof(ParenthesisClosed));
            }
        }
    }
}
