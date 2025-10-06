using ppotepa.tokenez.Tree;
using ppotepa.tokenez.Tree.Builders.Interfaces;
using ppotepa.tokenez.Tree.Exceptions;
using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Delimiters;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Keywords;

namespace ppotepa.tokenez.Tree.Builders
{
    internal class FunctionProcessor : ITokenProcessor
    {
        private readonly ParameterProcessor _parameterProcessor;
        private readonly ExpectationValidator _validator;

        public FunctionProcessor(ParameterProcessor parameterProcessor, ExpectationValidator validator)
        {
            _parameterProcessor = parameterProcessor;
            _validator = validator;
        }

        public bool CanProcess(Token token)
        {
            return token is FunctionToken;
        }

        public TokenProcessingResult Process(Token token, ProcessingContext context)
        {
            var functionToken = token as FunctionToken;
            BuilderLogger.LogFunctionFound(context.Depth);

            var functionScope = new Scope
            {
                OuterScope = context.CurrentScope,
                Type = ScopeType.Function
            };

            if (functionToken.Next is not IdentifierToken functionNameToken)
            {
                throw new UnexpectedTokenException(functionToken.Next, typeof(IdentifierToken));
            }

            var functionName = functionNameToken.RawToken.Text;
            BuilderLogger.LogFunctionName(functionName, context.Depth);

            var declaration = new FunctionDeclaration(functionNameToken);
            context.CurrentScope.Decarations.Add(functionName, declaration);
            functionScope.ScopeName = functionName;

            Token currentToken = functionNameToken;

            if (currentToken.Next is not ParenthesisOpen)
            {
                throw new UnexpectedTokenException(currentToken.Next, typeof(ParenthesisOpen));
            }

            currentToken = currentToken.Next;
            BuilderLogger.LogParametersStarting(context.Depth);

            var (parameters, nextToken) = _parameterProcessor.ProcessParameters(currentToken.Next);

            return TokenProcessingResult.ContinueWithScope(nextToken, functionScope);
        }
    }
}
