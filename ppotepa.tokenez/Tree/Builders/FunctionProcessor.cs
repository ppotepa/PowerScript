using ppotepa.tokenez.Tree;
using ppotepa.tokenez.Tree.Builders.Interfaces;
using ppotepa.tokenez.Tree.Exceptions;
using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Delimiters;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Keywords;

namespace ppotepa.tokenez.Tree.Builders
{
    /// <summary>
    /// Processes FUNCTION keyword tokens.
    /// Responsible for:
    /// - Extracting function name
    /// - Processing parameter list
    /// - Creating function scope
    /// - Registering function declaration in parent scope
    /// </summary>
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

            // Create a new scope for this function
            var functionScope = new Scope
            {
                OuterScope = context.CurrentScope,
                Type = ScopeType.Function  // Mark as function scope to enforce RETURN requirement
            };

            // Function must be followed by an identifier (function name)
            if (functionToken.Next is not IdentifierToken functionNameToken)
            {
                throw new UnexpectedTokenException(functionToken.Next, typeof(IdentifierToken));
            }

            var functionName = functionNameToken.RawToken.Text;
            BuilderLogger.LogFunctionName(functionName, context.Depth);

            // Register function in parent scope's declaration table
            var declaration = new FunctionDeclaration(functionNameToken);
            context.CurrentScope.Decarations.Add(functionName, declaration);
            functionScope.ScopeName = functionName;

            Token currentToken = functionNameToken;

            // Expect opening parenthesis for parameter list
            if (currentToken.Next is not ParenthesisOpen)
            {
                throw new UnexpectedTokenException(currentToken.Next, typeof(ParenthesisOpen));
            }

            currentToken = currentToken.Next;
            BuilderLogger.LogParametersStarting(context.Depth);

            // Delegate parameter processing to specialized processor
            var (parameters, nextToken) = _parameterProcessor.ProcessParameters(currentToken.Next);

            // Return the new function scope for the ScopeProcessor to handle
            return TokenProcessingResult.ContinueWithScope(nextToken, functionScope);
        }
    }
}
