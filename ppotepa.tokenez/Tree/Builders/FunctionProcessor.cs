using ppotepa.tokenez.Tree;
using ppotepa.tokenez.Tree.Exceptions;
using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Delimiters;
using ppotepa.tokenez.Tree.Tokens.Identifiers;

namespace ppotepa.tokenez.Tree.Builders
{
    /// <summary>
    /// Responsible for processing function declarations and their parameters
    /// </summary>
    internal class FunctionProcessor
    {
        private readonly ParameterProcessor _parameterProcessor;

        public FunctionProcessor(ParameterProcessor parameterProcessor)
        {
            _parameterProcessor = parameterProcessor;
        }

        public ProcessFunctionResult ProcessFunction(Token functionToken, Scope parentScope, int depth)
        {
            BuilderLogger.LogFunctionFound(depth);
            var functionScope = new Scope { OuterScope = parentScope };

            if (functionToken.Next is not IdentifierToken functionNameToken)
            {
                throw new UnexpectedTokenException(functionToken.Next, typeof(IdentifierToken));
            }

            var functionName = functionNameToken.RawToken.Text;
            BuilderLogger.LogFunctionName(functionName, depth);

            var declaration = new FunctionDeclaration(functionNameToken);
            parentScope.Decarations.Add(functionName, declaration);

            Token currentToken = functionNameToken;

            if (currentToken.Next is not ParenthesisOpen)
            {
                return new ProcessFunctionResult
                {
                    NextToken = currentToken.Next,
                    ParenthesisDepthChange = 0
                };
            }

            currentToken = currentToken.Next;
            BuilderLogger.LogParametersStarting(depth);

            var (parameters, nextToken) = _parameterProcessor.ProcessParameters(currentToken.Next);

            return new ProcessFunctionResult
            {
                NextToken = nextToken,
                ParenthesisDepthChange = 0, // Open and close balanced
                Parameters = parameters
            };
        }
    }

    internal class ProcessFunctionResult
    {
        public Token NextToken { get; init; }
        public int ParenthesisDepthChange { get; init; }
        public FunctionParametersToken Parameters { get; init; }
    }
}
