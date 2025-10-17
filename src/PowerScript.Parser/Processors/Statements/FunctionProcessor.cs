using PowerScript.Common.Logging;
using PowerScript.Core.AST;
using PowerScript.Core.Exceptions;
using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Delimiters;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Interfaces;
using PowerScript.Core.Syntax.Tokens.Keywords;
using PowerScript.Core.Syntax.Tokens.Keywords.Types;
using PowerScript.Core.Syntax.Tokens.Scoping;
using PowerScript.Core.Syntax.Tokens.Values;
using PowerScript.Parser.Processors.Base;
using PowerScript.Parser.Processors.Expressions;

namespace PowerScript.Parser.Processors.Statements;

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

        // Check if function already exists (e.g., from linked libraries being inlined multiple times)
        if (context.CurrentScope.Decarations.ContainsKey(functionName))
        {
            LoggerService.Logger.Debug($"Function '{functionName}' already declared in scope '{context.CurrentScope.ScopeName}', skipping duplicate");

            // Skip to the end of this duplicate function definition
            Token? skipToken = functionNameToken;
            int braceDepth = 0;
            bool foundOpenBrace = false;

            while (skipToken != null)
            {
                if (skipToken is ScopeStartToken)
                {
                    braceDepth++;
                    foundOpenBrace = true;
                }
                else if (skipToken is ScopeEndToken)
                {
                    braceDepth--;
                    if (foundOpenBrace && braceDepth == 0)
                    {
                        // Found the matching closing brace, continue after it
                        return TokenProcessingResult.Continue(skipToken.Next!);
                    }
                }
                skipToken = skipToken.Next;
            }

            throw new InvalidOperationException($"Duplicate function '{functionName}' has no matching closing brace");
        }

        // Register function in parent scope's declaration table
        FunctionDeclaration declaration = new(functionNameToken);
        context.CurrentScope.Decarations.Add(functionName, declaration);
        functionScope.ScopeName = functionName;

        // Link the scope to the declaration so we can visualize it later
        declaration.Scope = functionScope;
        functionScope.FunctionDeclaration = declaration;

        LoggerService.Logger.Debug($"Registered function '{functionName}' in scope '{context.CurrentScope.ScopeName}'");

        Token currentToken = functionNameToken;

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
            int? returnTypeBitWidth = null;

            // Check for bit-width specification in return type (e.g., [INT[8]])
            if (currentReturnToken is BracketOpen)
            {
                LoggerService.Logger.Debug($"[FunctionProcessor] Found bit-width in return type");
                currentReturnToken = currentReturnToken.Next!;

                if (currentReturnToken is not ValueToken bitWidthToken)
                {
                    throw new UnexpectedTokenException(
                        currentReturnToken,
                        "Expected numeric value for bit-width specification in return type",
                        typeof(ValueToken)
                    );
                }

                string bitWidthText = bitWidthToken.RawToken!.Text;
                if (!int.TryParse(bitWidthText, out int parsedBitWidth))
                {
                    throw new InvalidOperationException(
                        $"Invalid bit-width value '{bitWidthText}' in return type - must be an integer"
                    );
                }

                returnTypeBitWidth = parsedBitWidth;
                LoggerService.Logger.Debug($"[FunctionProcessor] Return type bit-width: {returnTypeBitWidth}");

                currentReturnToken = bitWidthToken.Next!;

                if (currentReturnToken is not BracketClosed)
                {
                    throw new UnexpectedTokenException(
                        currentReturnToken,
                        "Expected closing bracket ']' after bit-width value in return type",
                        typeof(BracketClosed)
                    );
                }

                currentReturnToken = currentReturnToken.Next!;
            }

            // Check if this is a composite return type (e.g., INT CHAIN, PREC CHAIN)
            if (currentReturnToken is ChainToken)
            {
                LoggerService.Logger.Debug(
                    $"Function '{functionName}' has composite return type: {returnTypeToken.RawToken?.Text} CHAIN");
                currentReturnToken = currentReturnToken.Next;
            }
            else
            {
                string typeSpec = returnTypeBitWidth.HasValue
                    ? $"{returnTypeToken.RawToken?.Text}[{returnTypeBitWidth}]"
                    : returnTypeToken.RawToken?.Text!;
                LoggerService.Logger.Debug(
                    $"Function '{functionName}' has return type: {typeSpec}");
            }

            // Store the return type in the function declaration (using base type for now)
            declaration.ReturnType = returnTypeToken;
            declaration.ReturnTypeBitWidth = returnTypeBitWidth;

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
        // Return control with the new function scope so ScopeProcessor knows we're inside a function
        return TokenProcessingResult.ContinueWithScope(nextToken, functionScope);
    }
}