using ppotepa.tokenez.Logging;
using ppotepa.tokenez.Tree.Builders.Interfaces;
using ppotepa.tokenez.Tree.Exceptions;
using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Delimiters;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Keywords;
using ppotepa.tokenez.Tree.Tokens.Keywords.Types;
using ppotepa.tokenez.Tree.Tokens.Scoping;

namespace ppotepa.tokenez.Tree.Builders
{
    /// <summary>
    ///     Processes FUNCTION keyword tokens.
    ///     Responsible for:
    ///     - Extracting function name
    ///     - Processing parameter list
    ///     - Creating function scope
    ///     - Registering function declaration in parent scope
    /// </summary>
    internal class FunctionProcessor(ParameterProcessor parameterProcessor) : ITokenProcessor
    {
        private readonly ParameterProcessor _parameterProcessor = parameterProcessor;

        public bool CanProcess(Token token)
        {
            return token is FunctionToken;
        }

        public TokenProcessingResult Process(Token token, ProcessingContext context)
        {
            LoggerService.Logger.Debug(
                $"FunctionProcessor: Processing FUNCTION token '{token.RawToken?.Text}' at depth {context.Depth}");
            var functionToken = token as FunctionToken;
            BuilderLogger.LogFunctionFound(context.Depth);

            // Create a new scope for this function
            Scope functionScope = new()
            {
                OuterScope = context.CurrentScope,
                Type = ScopeType.Function // Mark as function scope to enforce RETURN requirement
            };
            LoggerService.Logger.Debug($"Created new function scope: {functionScope.ScopeName} (Type={functionScope.Type})");

            // Function must be followed by an identifier (function name)
            if (functionToken?.Next is not IdentifierToken functionNameToken)
                throw new UnexpectedTokenException(functionToken?.Next ?? functionToken!, typeof(IdentifierToken));

            var functionName = functionNameToken.RawToken!.Text;
            BuilderLogger.LogFunctionName(functionName, context.Depth);

            // Register function in parent scope's declaration table
            FunctionDeclaration declaration = new(functionNameToken);
            context.CurrentScope.Decarations.Add(functionName, declaration);
            functionScope.ScopeName = functionName;

            // Link the scope to the declaration so we can visualize it later
            declaration.Scope = functionScope;
            functionScope.FunctionDeclaration = declaration;

            LoggerService.Logger.Debug($"Registered function '{functionName}' in scope '{context.CurrentScope.ScopeName}'");

            Token currentToken = functionNameToken;

            // Expect opening parenthesis for parameter list
            if (currentToken.Next is not ParenthesisOpen)
                throw new UnexpectedTokenException(currentToken.Next, typeof(ParenthesisOpen));

            currentToken = currentToken.Next;
            BuilderLogger.LogParametersStarting(context.Depth);

            // Delegate parameter processing to specialized processor
            LoggerService.Logger.Debug($"Delegating parameter processing for function '{functionName}'");
            var (parametersToken, nextToken) = _parameterProcessor.ProcessParameters(currentToken.Next);

            // Store the parameters in the function declaration
            declaration.Parameters = parametersToken.Declarations;

            // Check if there's an optional return type specified with brackets
            if (nextToken is BracketOpen)
            {
                BuilderLogger.LogFunctionName($"Processing return type for function '{functionName}'", context.Depth);

                // Expect a type token after the opening bracket
                if (nextToken.Next is not ITypeToken returnTypeInterface)
                    throw new UnexpectedTokenException(nextToken.Next, typeof(ITypeToken));

                // Cast to Token to access Token properties
                var returnTypeToken = (Token)returnTypeInterface;
                var currentReturnToken = returnTypeToken.Next;

                // Check if this is a composite return type (e.g., INT CHAIN, PREC CHAIN)
                if (currentReturnToken is ChainToken)
                {
                    LoggerService.Logger.Debug(
                        $"Function '{functionName}' has composite return type: {returnTypeToken.RawToken?.Text} CHAIN");
                    currentReturnToken = currentReturnToken.Next;
                }
                else
                {
                    LoggerService.Logger.Debug(
                        $"Function '{functionName}' has return type: {returnTypeToken.RawToken?.Text}");
                }

                // Store the return type in the function declaration (using base type for now)
                declaration.ReturnType = returnTypeToken;

                // Expect closing bracket after the return type
                if (currentReturnToken is not BracketClosed)
                    throw new UnexpectedTokenException(currentReturnToken, typeof(BracketClosed));

                // Move to the token after the closing bracket
                nextToken = currentReturnToken.Next;
            }

            // After parameters (and optional return type), we expect a scope start token ('{')
            if (nextToken is not ScopeStartToken)
            {
                LoggerService.Logger.Error(
                    $"Expected ScopeStartToken after parameters for function '{functionName}', but got {nextToken?.GetType().Name} '{nextToken?.RawToken?.Text}'");
                throw new UnexpectedTokenException(nextToken!, typeof(ScopeStartToken));
            }

            LoggerService.Logger.Debug($"Passing control to ScopeProcessor for function '{functionName}'");
            // Return control to continue processing the ScopeStartToken without modifying the parent context
            // The ScopeProcessor will handle the function scope internally
            return TokenProcessingResult.Continue(nextToken);
        }
    }
}