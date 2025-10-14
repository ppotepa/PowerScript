using Tokenez.Common.Logging;
using Tokenez.Core.AST;
using Tokenez.Core.Exceptions;
using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Delimiters;
using Tokenez.Core.Syntax.Tokens.Identifiers;
using Tokenez.Core.Syntax.Tokens.Keywords;
using Tokenez.Core.Syntax.Tokens.Keywords.Types;
using Tokenez.Core.Syntax.Tokens.Scoping;
using Tokenez.Parser.Processors.Base;
using Tokenez.Parser.Processors.Expressions;

namespace Tokenez.Parser.Processors.Statements;

/// <summary>
///     Processes FUNCTION keyword tokens.
///     Responsible for:
///     - Extracting function name
///     - Processing parameter list
///     - Creating function scope
///     - Registering function declaration in parent scope
/// </summary>
public class FunctionProcessor(ParameterProcessor parameterProcessor) : ITokenProcessor
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
        FunctionToken? functionToken = token as FunctionToken;
        BuilderLogger.LogFunctionFound(context.Depth);

        // Create a new scope for this function
        Scope functionScope = new()
        {
            OuterScope = context.CurrentScope,
            Type = ScopeType.Function // Mark as function scope to enforce RETURN requirement
        };
        LoggerService.Logger.Debug(
            $"Created new function scope: {functionScope.ScopeName} (Type={functionScope.Type})");

        // Function must be followed by an identifier (function name)
        if (functionToken?.Next is not IdentifierToken functionNameToken)
        {
            throw new UnexpectedTokenException(functionToken?.Next ?? functionToken!, typeof(IdentifierToken));
        }

        string functionName = functionNameToken.RawToken!.Text;
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

        // Check which syntax is being used:
        // Standard: FUNCTION name(params)[TYPE]{
        // Alternative: FUNCTION name RETURNS TYPE WITH params{
        bool isAlternativeSyntax = currentToken.Next is ReturnsKeywordToken;

        if (isAlternativeSyntax)
        {
            // Alternative syntax: FUNCTION name RETURNS TYPE WITH params{
            if (currentToken.Next is not ReturnsKeywordToken returnsToken)
            {
                throw new UnexpectedTokenException(currentToken.Next, typeof(ReturnsKeywordToken));
            }

            currentToken = returnsToken;

            // Expect return type after RETURNS
            // NUMBER is special - it's treated as INT but written as identifier to avoid conflicts
            ITypeToken returnTypeInterface;
            Token returnTypeToken;

            if (currentToken.Next is IdentifierToken identToken &&
                identToken.RawToken!.Text == "NUMBER")
            {
                // Create synthetic INT token for NUMBER
                returnTypeInterface = new IntToken();
                returnTypeToken = (Token)returnTypeInterface;
                currentToken = identToken;
            }
            else if (currentToken.Next is ITypeToken typeToken)
            {
                returnTypeInterface = typeToken;
                returnTypeToken = (Token)returnTypeInterface;
                currentToken = returnTypeToken;
            }
            else
            {
                throw new UnexpectedTokenException(currentToken.Next, typeof(ITypeToken));
            }

            // Check for composite type (e.g., NUMBER CHAIN)
            if (currentToken.Next is ChainToken)
            {
                LoggerService.Logger.Debug(
                    $"Function '{functionName}' has composite return type: {returnTypeToken.RawToken?.Text} CHAIN");
                currentToken = currentToken.Next;
            }
            else
            {
                LoggerService.Logger.Debug(
                    $"Function '{functionName}' has return type: {returnTypeToken.RawToken?.Text}");
            }

            // Store the return type
            declaration.ReturnType = returnTypeToken;

            // Expect WITH keyword
            if (currentToken.Next is not WithKeywordToken withToken)
            {
                throw new UnexpectedTokenException(currentToken.Next, typeof(WithKeywordToken));
            }

            currentToken = withToken;

            // Now parse parameters (no parentheses in alternative syntax)
            BuilderLogger.LogParametersStarting(context.Depth);
            LoggerService.Logger.Debug(
                $"Delegating parameter processing for function '{functionName}' (alternative syntax)");

            // Parse parameters until we hit the opening brace
            (FunctionParametersToken altParametersToken, Token altNextToken) = ParseParametersAlternativeSyntax(currentToken.Next);
            declaration.Parameters = altParametersToken.Declarations;

            // altNextToken should now be ScopeStartToken
            if (altNextToken is not ScopeStartToken)
            {
                LoggerService.Logger.Error(
                    $"Expected ScopeStartToken after parameters for function '{functionName}', but got {altNextToken?.GetType().Name} '{altNextToken?.RawToken?.Text}'");
                throw new UnexpectedTokenException(altNextToken!, typeof(ScopeStartToken));
            }

            LoggerService.Logger.Debug($"Passing control to ScopeProcessor for function '{functionName}'");
            return TokenProcessingResult.Continue(altNextToken);
        }

        // Standard syntax: FUNCTION name(params)[TYPE]{
        // Expect opening parenthesis for parameter list
        if (currentToken.Next is not ParenthesisOpen)
        {
            throw new UnexpectedTokenException(currentToken.Next, typeof(ParenthesisOpen));
        }

        currentToken = currentToken.Next;
        BuilderLogger.LogParametersStarting(context.Depth);

        // Delegate parameter processing to specialized processor
        LoggerService.Logger.Debug($"Delegating parameter processing for function '{functionName}'");
        (FunctionParametersToken parametersToken, Token nextToken) = _parameterProcessor.ProcessParameters(currentToken.Next);

        // Store the parameters in the function declaration
        declaration.Parameters = parametersToken.Declarations;

        // Check if there's an optional return type specified with brackets
        if (nextToken is BracketOpen)
        {
            BuilderLogger.LogFunctionName($"Processing return type for function '{functionName}'", context.Depth);

            // Expect a type token after the opening bracket
            if (nextToken.Next is not ITypeToken returnTypeInterface)
            {
                throw new UnexpectedTokenException(nextToken.Next, typeof(ITypeToken));
            }

            // Cast to Token to access Token properties
            Token returnTypeToken = (Token)returnTypeInterface;
            Token currentReturnToken = returnTypeToken.Next;

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
            {
                throw new UnexpectedTokenException(currentReturnToken, typeof(BracketClosed));
            }

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

    /// <summary>
    ///     Parses parameters in alternative syntax (no parentheses).
    ///     Alternative syntax: FUNCTION name RETURNS TYPE WITH param1, param2, param3 {
    ///     Parses: param1, param2, param3
    /// </summary>
    private static (FunctionParametersToken, Token) ParseParametersAlternativeSyntax(Token startToken)
    {
        FunctionParametersToken parameters = new();
        Token currentToken = startToken;

        // Parse parameters until we hit the opening brace
        while (currentToken is not ScopeStartToken)
        {
            // Expect an identifier (parameter name without type in alt syntax)
            if (currentToken is not IdentifierToken paramNameToken)
            {
                throw new UnexpectedTokenException(currentToken, typeof(IdentifierToken));
            }

            // In alternative syntax, all parameters are implicitly NUMBER (INT)
            // Create a synthetic INT token for the parameter type
            IntToken implicitTypeToken = new();
            parameters.Declarations.Add(new ParameterDeclaration(implicitTypeToken, paramNameToken));

            currentToken = paramNameToken.Next;

            // Check for comma (more parameters) or scope start (end of parameters)
            if (currentToken is CommaToken)
            {
                currentToken = currentToken.Next; // Skip comma
                continue;
            }

            if (currentToken is ScopeStartToken)
            {
                break;
            }

            // Unexpected token
            throw new UnexpectedTokenException(
                currentToken,
                "Unexpected token in alternative syntax parameter list",
                typeof(CommaToken),
                typeof(ScopeStartToken));
        }

        return (parameters, currentToken);
    }
}