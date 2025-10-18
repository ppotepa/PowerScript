using PowerScript.Common.Logging;
using PowerScript.Core.AST;
using PowerScript.Core.AST.Expressions;
using PowerScript.Core.AST.Statements;
using PowerScript.Core.Exceptions;
using PowerScript.Core.Syntax;
using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Delimiters;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Keywords;
using PowerScript.Core.Syntax.Tokens.Keywords.Types;
using PowerScript.Core.Syntax.Tokens.Operators;
using PowerScript.Core.Syntax.Tokens.Raw;
using PowerScript.Core.Syntax.Tokens.Scoping;
using PowerScript.Core.Syntax.Tokens.Values;
using PowerScript.Parser.Processors.Base;

namespace PowerScript.Parser.Processors.Statements;

/// <summary>
///     Processes FLEX keyword for dynamic variable declarations.
///     Syntax: FLEX variableName = expression
/// </summary>
public class FlexVariableProcessor : ITokenProcessor
{
    public bool CanProcess(Token token)
    {
        return token is FlexKeywordToken;
    }

    public TokenProcessingResult Process(Token token, ProcessingContext context)
    {
        LoggerService.Logger.Info(
            $"[FlexVariableProcessor] Processing FLEX variable declaration in scope '{context.CurrentScope.ScopeName}'");

        FlexKeywordToken? flexToken = token as FlexKeywordToken;
        Token? currentToken = flexToken!.Next;

        // Expect identifier (variable name)
        if (currentToken is not IdentifierToken identifierToken)
        {
            throw new UnexpectedTokenException(currentToken!, typeof(IdentifierToken));
        }

        string variableName = identifierToken.RawToken!.Text;
        LoggerService.Logger.Debug($"[FlexVariableProcessor] Variable name: {variableName}");

        currentToken = currentToken.Next;

        // Check if this is an array element assignment (FLEX arr[0] = value or FLEX arr[0][1] = value)
        if (currentToken is BracketOpen)
        {
            // Build IndexExpression (supports chaining for 2D arrays)
            Expression currentExpr = new IdentifierExpression(identifierToken);

            while (currentToken is BracketOpen)
            {
                currentToken = currentToken.Next; // Move past '['
                Expression indexExpr = ParseExpression(ref currentToken);

                // Expect closing bracket
                if (currentToken is not BracketClosed)
                {
                    throw new UnexpectedTokenException(currentToken!, typeof(BracketClosed));
                }

                currentToken = currentToken.Next; // Skip ']'

                currentExpr = new IndexExpression
                {
                    ArrayExpression = currentExpr,
                    Index = indexExpr
                };
            }

            // Expect assignment operator
            if (currentToken is not EqualsToken)
            {
                throw new UnexpectedTokenException(currentToken!, typeof(EqualsToken));
            }

            currentToken = currentToken.Next;

            // Parse the value expression
            Expression valueExpr = ParseExpression(ref currentToken);

            // Create an ArrayAssignmentStatement
            ArrayAssignmentStatement arrayAssignStmt = new((IndexExpression)currentExpr, valueExpr)
            {
                StartToken = flexToken
            };

            context.CurrentScope.Statements.Add(arrayAssignStmt);

            LoggerService.Logger.Success(
                $"[FlexVariableProcessor] Registered array assignment '{variableName}[...] = ...' in scope '{context.CurrentScope.ScopeName}'");

            return new TokenProcessingResult
            {
                NextToken = currentToken!,
                ShouldValidateExpectations = false
            };
        }

        // Expect assignment operator
        if (currentToken is not EqualsToken)
        {
            throw new UnexpectedTokenException(currentToken!, typeof(EqualsToken));
        }

        currentToken = currentToken.Next;

        // Check if the value is a pattern syntax expression (e.g., TAKE 3 FROM arr)
        Expression initExpression;
        if (currentToken is CustomKeywordToken customKeywordToken && customKeywordToken.PositionInPattern == 0)
        {
            // This is a pattern keyword at position 0 - delegate to PatternSyntaxProcessor
            LoggerService.Logger.Debug($"[FlexVariableProcessor] Detected pattern syntax starting with '{customKeywordToken.RawToken?.Text}'");

            var patternProcessor = new PatternSyntaxProcessor();

            // Check if the pattern processor can handle this token
            if (!patternProcessor.CanProcess(customKeywordToken))
            {
                LoggerService.Logger.Debug($"[FlexVariableProcessor] Pattern processor cannot handle token '{customKeywordToken}'");
                throw new InvalidOperationException($"Pattern processor cannot handle token '{customKeywordToken}'");
            }

            // Remember the statement count before processing
            int statementCountBefore = context.CurrentScope.Statements.Count;

            var patternResult = patternProcessor.Process(customKeywordToken, context);

            // PatternSyntaxProcessor adds an ExpressionStatement to the scope - remove it and extract the expression
            if (context.CurrentScope.Statements.Count > statementCountBefore &&
                context.CurrentScope.Statements[^1] is ExpressionStatement exprStmt)
            {
                context.CurrentScope.Statements.RemoveAt(context.CurrentScope.Statements.Count - 1);
                initExpression = exprStmt.Expression;
                currentToken = patternResult.NextToken;
            }
            else
            {
                throw new InvalidOperationException($"Pattern processor did not add an ExpressionStatement for pattern starting with '{customKeywordToken.RawToken?.Text}'");
            }
        }
        else if (currentToken is IdentifierToken patternCheckToken)
        {
            // Check if this starts a pattern
            var patterns = CustomSyntaxRegistry.Instance.GetPatternTransformations();
            string firstWord = patternCheckToken.Value.ToUpperInvariant();
            bool isPattern = patterns.Any(p =>
            {
                int spaceIndex = p.Pattern.IndexOf(' ');
                string patternFirst = spaceIndex < 0 ? p.Pattern.ToUpperInvariant() : p.Pattern.Substring(0, spaceIndex).ToUpperInvariant();
                return patternFirst == firstWord;
            });

            if (isPattern)
            {
                // This is a pattern syntax - use PatternSyntaxProcessor to parse it
                var patternProcessor = new PatternSyntaxProcessor();

                // Check if the pattern processor can handle this token
                if (!patternProcessor.CanProcess(currentToken))
                {
                    LoggerService.Logger.Debug($"[FlexVariableProcessor] Pattern processor cannot handle token '{currentToken}' - not a pattern");
                    // Fall back to normal expression parsing
                    initExpression = ParseExpression(ref currentToken);
                }
                else
                {
                    var dummyContext = new ProcessingContext(context.CurrentScope, context.Depth);
                    var result = patternProcessor.Process(currentToken, dummyContext);

                    // Extract the expression that was added to the scope
                    var lastStatement = context.CurrentScope.Statements[context.CurrentScope.Statements.Count - 1];
                    if (lastStatement is ExpressionStatement exprStmt)
                    {
                        initExpression = exprStmt.Expression;
                        // Remove it from statements since we're using it in variable declaration
                        context.CurrentScope.Statements.RemoveAt(context.CurrentScope.Statements.Count - 1);
                        currentToken = result.NextToken;
                    }
                    else
                    {
                        // Fallback to normal parsing
                        initExpression = ParseExpression(ref currentToken);
                    }
                }
            }
            else
            {
                // Normal expression parsing
                initExpression = ParseExpression(ref currentToken);
            }
        }
        else
        {
            // Normal expression parsing
            initExpression = ParseExpression(ref currentToken);
        }

        // Create variable declaration
        VariableDeclaration variableDecl = new(identifierToken); // No type token for FLEX variables

        // Create variable declaration statement
        VariableDeclarationStatement statement = new(variableDecl, initExpression, true)
        {
            StartToken = flexToken
        };

        // Add to current scope
        context.CurrentScope.Statements.Add(statement);
        context.CurrentScope.AddDynamicVariable(variableName);

        LoggerService.Logger.Success(
            $"[FlexVariableProcessor] Registered FLEX variable '{variableName}' in scope '{context.CurrentScope.ScopeName}'");

        return new TokenProcessingResult
        {
            NextToken = currentToken!,
            ShouldValidateExpectations = false
        };
    }


    /// <summary>
    ///     Parses an expression with support for binary operations.
    ///     Handles operator precedence: * / before + -
    /// </summary>
    private Expression ParseExpression(ref Token? token)
    {
        // Parse additive expression (lowest precedence)
        return ParseAdditiveExpression(ref token);
    }

    /// <summary>
    ///     Parses addition and subtraction (left-to-right)
    /// </summary>
    private Expression ParseAdditiveExpression(ref Token? token)
    {
        Expression left = ParseMultiplicativeExpression(ref token);

        while (token is PlusToken or MinusToken)
        {
            Token operatorToken = token;
            token = token.Next;

            Expression right = ParseMultiplicativeExpression(ref token);

            left = new BinaryExpression(left, operatorToken, right);
        }

        return left;
    }

    /// <summary>
    ///     Parses multiplication, division, and modulo (left-to-right)
    /// </summary>
    private Expression ParseMultiplicativeExpression(ref Token? token)
    {
        Expression left = ParsePrimaryExpression(ref token);

        while (token is MultiplyToken or DivideToken or ModuloToken)
        {
            Token operatorToken = token;
            token = token.Next;

            Expression right = ParsePrimaryExpression(ref token);

            left = new BinaryExpression(left, operatorToken, right);
        }

        return left;
    }

    /// <summary>
    ///     Parses primary expressions: literals, identifiers, function calls, parentheses
    /// </summary>
    private Expression ParsePrimaryExpression(ref Token? token)
    {
        // Handle custom syntax blocks: ![MAX OF numbers]
        if (token is CustomSyntaxBlockOpen)
        {
            // Use the global ExpressionParser to handle custom syntax blocks
            var expressionParser = new ExpressionParser();
            return expressionParser.ParseCustomSyntaxBlock(ref token);
        }

        // Handle parentheses for precedence override
        if (token is ParenthesisOpen)
        {
            token = token.Next; // Skip '('

            // Recursively parse the expression inside parentheses
            Expression innerExpr = ParseExpression(ref token);

            // Expect closing parenthesis
            if (token is not ParenthesisClosed)
            {
                throw new UnexpectedTokenException(token!, typeof(ParenthesisClosed));
            }

            token = token.Next; // Skip ')'
            return innerExpr;
        }

        if (token is BracketOpen)
        {
            // Array literal: [1, 2, 3, 4]
            token = token.Next; // Move past '['

            List<Expression> elements = [];

            // Handle empty array []
            if (token is BracketClosed)
            {
                token = token.Next; // Move past ']'
                return new ArrayLiteralExpression(elements);
            }

            // Parse array elements
            while (token != null)
            {
                // Parse element expression
                Expression elementExpr = ParsePrimaryExpression(ref token);
                elements.Add(elementExpr);

                // Check for comma or closing bracket
                if (token is CommaToken)
                {
                    token = token.Next; // Move past comma

                    // Allow trailing comma: [1, 2, 3,]
                    if (token is BracketClosed)
                    {
                        break;
                    }
                }
                else if (token is BracketClosed)
                {
                    break;
                }
                else
                {
                    throw new UnexpectedTokenException(token!, typeof(CommaToken), typeof(BracketClosed));
                }
            }

            if (token is not BracketClosed)
            {
                throw new UnexpectedTokenException(token!, typeof(BracketClosed));
            }

            token = token.Next; // Move past ']'
            return new ArrayLiteralExpression(elements);
        }

        if (token is ChainToken)
        {
            // CHAIN keyword for array creation
            token = token.Next; // Move past CHAIN

            // Check if this is CHAIN OF [...] syntax (array literal)
            if (token is OfKeywordToken)
            {
                token = token.Next; // Move past OF

                // Expect opening bracket [
                if (token is not BracketOpen)
                {
                    throw new UnexpectedTokenException(token!, typeof(BracketOpen));
                }

                token = token.Next; // Move past '['

                List<Expression> elements = [];

                // Handle empty array: CHAIN OF []
                if (token is BracketClosed)
                {
                    token = token.Next; // Move past ']'
                    return new ArrayLiteralExpression(elements);
                }

                // Parse array elements
                while (token != null)
                {
                    // Parse element expression
                    Expression elementExpr = ParsePrimaryExpression(ref token);
                    elements.Add(elementExpr);

                    // Check for comma or closing bracket
                    if (token is CommaToken)
                    {
                        token = token.Next; // Move past comma

                        // Allow trailing comma: CHAIN OF [1, 2, 3,]
                        if (token is BracketClosed)
                        {
                            break;
                        }
                    }
                    else if (token is BracketClosed)
                    {
                        break;
                    }
                    else
                    {
                        throw new UnexpectedTokenException(token!, typeof(CommaToken), typeof(BracketClosed));
                    }
                }

                if (token is not BracketClosed)
                {
                    throw new UnexpectedTokenException(token!, typeof(BracketClosed));
                }

                token = token.Next; // Move past ']'
                return new ArrayLiteralExpression(elements);
            }

            // Otherwise, it's CHAIN <size> syntax (array creation with size)
            // Size can be a literal value or an identifier (variable/parameter)
            Expression sizeExpr;
            if (token is ValueToken sizeValueToken)
            {
                sizeExpr = new LiteralExpression(sizeValueToken);
                token = token.Next;
            }
            else if (token is IdentifierToken sizeIdentToken)
            {
                sizeExpr = new IdentifierExpression(sizeIdentToken);
                token = token.Next;
            }
            else
            {
                throw new UnexpectedTokenException(token!, typeof(ValueToken), typeof(IdentifierToken));
            }

            ArrayCreationExpression expr = new(sizeExpr);
            return expr;
        }

        if (token is ValueToken valueToken)
        {
            LiteralExpression expr = new(valueToken);
            token = token.Next;
            return expr;
        }

        if (token is DecimalToken decimalToken)
        {
            LiteralExpression expr = new(decimalToken);
            token = token.Next;
            return expr;
        }

        if (token is StringLiteralToken stringToken)
        {
            StringLiteralExpression expr = new(stringToken);
            token = token.Next;
            return expr;
        }

        if (token is TemplateStringToken templateToken)
        {
            TemplateStringExpression expr = new(templateToken);
            token = token.Next;
            return expr;
        }

        if (token is IdentifierToken identifierToken)
        {
            // Check for custom syntax operator: identifier::Method()
            if (identifierToken.Next is CustomSyntaxOperatorToken)
            {
                // Delegate to ExpressionParser to handle custom syntax
                Token currentParseToken = identifierToken;
                var parser = new ExpressionParser();
                var expr = parser.Parse(ref currentParseToken);
                token = currentParseToken;
                return expr;
            }

            // Start with identifier expression
            Expression currentExpr = new IdentifierExpression(identifierToken);
            token = identifierToken.Next;

            // Check for array index access (supports chaining: arr[0][1][2])
            while (token is BracketOpen)
            {
                token = token.Next; // Move past '['
                Expression indexExpr = ParseExpression(ref token);

                // Expect closing bracket
                if (token is not BracketClosed)
                {
                    throw new UnexpectedTokenException(token!, typeof(BracketClosed));
                }

                token = token.Next; // Skip ']'

                currentExpr = new IndexExpression
                {
                    ArrayExpression = currentExpr,
                    Index = indexExpr
                };
            }

            // Check for property access: identifier.property (can chain: obj.prop1.prop2)
            while (token is DotToken)
            {
                token = token.Next; // Move past .

                if (token is not IdentifierToken propertyToken)
                {
                    throw new UnexpectedTokenException(token!, typeof(IdentifierToken));
                }

                string propertyName = propertyToken.Value;
                currentExpr = new PropertyAccessExpression(currentExpr, propertyName);
                token = propertyToken.Next; // Move past property name
            }

            // Check if it's a function call (only if no array indexing or property access was done)
            if (currentExpr is IdentifierExpression && token is ParenthesisOpen)
            {
                // Parse function arguments
                var (arguments, tokenAfterArgs) = ParseFunctionArguments(token);

                FunctionCallExpression funcCallExpr = new()
                {
                    FunctionName = identifierToken
                };
                funcCallExpr.Arguments.AddRange(arguments);

                token = tokenAfterArgs;
                return funcCallExpr;
            }

            // Check for ILLEGAL arrow operator (-> without #)
            // Arrow operator -> can ONLY be used with # prefix for .NET calls
            if (token is ArrowToken)
            {
                string identName = identifierToken.RawToken?.Text ?? "identifier";
                throw new InvalidOperationException(
                    $"Arrow operator (->) requires # prefix for .NET calls. " +
                    $"Use '#' before the identifier: #{identName}->... " +
                    $"Example: #Console->WriteLine() instead of {identName}->WriteLine()");
            }

            // Return the expression (could be IdentifierExpression or IndexExpression)
            return currentExpr;
        }

        // Handle unary minus for negative numbers
        if (token is MinusToken minusToken)
        {
            token = token.Next; // Move past minus

            if (token is ValueToken negValueToken)
            {
                // Create negative value token
                token = token.Next;
                RawToken negativeRaw = RawToken.Create($"-{negValueToken.RawToken.OriginalText}");
                ValueToken negativeToken = new(negativeRaw);
                return new LiteralExpression(negativeToken);
            }

            // If it's not followed by a number, it might be a minus expression
            // Put the token back and let the caller handle it
            token = minusToken;
        }

        // Handle object literals: {prop = val, ...}
        if (token is ScopeStartToken)
        {
            // Use ExpressionParser for object literals
            var parser = new ExpressionParser();
            return parser.Parse(ref token);
        }

        // Handle .NET type access: #Char, #Console, #String, etc.
        if (token is NetKeywordToken netToken)
        {
            token = token.Next; // Move past #

            // Expect an identifier (the .NET type/class name)
            if (token is not IdentifierToken typeIdentToken)
            {
                throw new UnexpectedTokenException(token!, typeof(IdentifierToken));
            }

            // Start with identifier expression for the .NET type
            Expression currentExpr = new IdentifierExpression(typeIdentToken);
            token = typeIdentToken.Next;

            // Check for dot notation (static method/property): #Char.IsLetter(c, 0)
            if (token is DotToken)
            {
                token = token.Next; // Move past .

                // Get the member name
                if (token is not IdentifierToken memberToken)
                {
                    throw new UnexpectedTokenException(token!, typeof(IdentifierToken));
                }

                string memberName = memberToken.RawToken?.OriginalText ?? memberToken.RawToken?.Text ?? "";
                token = memberToken.Next; // Move past member name

                // Check if it's a method call (has parentheses)
                if (token is ParenthesisOpen)
                {
                    // Parse function arguments
                    var (arguments, tokenAfterArgs) = ParseFunctionArguments(token);
                    token = tokenAfterArgs;

                    // Create .NET member access expression for static method call
                    NetMemberAccessExpression methodCallExpr = new(currentExpr, memberName, arguments);
                    return methodCallExpr;
                }
                else
                {
                    // Static property access: #Type.Property
                    NetMemberAccessExpression propertyAccessExpr = new(currentExpr, memberName);
                    return propertyAccessExpr;
                }
            }

            // Check for arrow operator (instance method on variable): #var -> Method()
            if (token is ArrowToken)
            {
                token = token.Next; // Move past ->

                // Get the member name
                if (token is not IdentifierToken memberToken)
                {
                    throw new UnexpectedTokenException(token!, typeof(IdentifierToken));
                }

                string memberName = memberToken.RawToken?.OriginalText ?? memberToken.RawToken?.Text ?? "";
                token = memberToken.Next; // Move past member name

                // Check if it's a method call (has parentheses)
                if (token is ParenthesisOpen)
                {
                    // Parse function arguments
                    var (arguments, tokenAfterArgs) = ParseFunctionArguments(token);
                    token = tokenAfterArgs;

                    // Create .NET member access expression for method call
                    NetMemberAccessExpression methodCallExpr = new(currentExpr, memberName, arguments);
                    return methodCallExpr;
                }
                else
                {
                    // Property access: #var -> Property
                    NetMemberAccessExpression propertyAccessExpr = new(currentExpr, memberName);
                    return propertyAccessExpr;
                }
            }

            // Just #Type without dot or arrow (unlikely but handle it)
            return currentExpr;
        }

        throw new NotImplementedException(
            $"Expression type {token?.GetType().Name} not yet supported in FLEX variable initialization");
    }

    /// <summary>
    ///     Parses function call arguments from opening parenthesis to closing parenthesis.
    ///     Returns the list of argument expressions and the token after the closing parenthesis.
    /// </summary>
    private static (List<Expression> Arguments, Token? NextToken) ParseFunctionArguments(Token openParenToken)
    {
        List<Expression> arguments = new();
        Token? currentToken = openParenToken.Next; // Start after '('

        // Handle empty argument list
        if (currentToken is ParenthesisClosed)
        {
            return (arguments, currentToken.Next);
        }

        // Parse comma-separated arguments
        while (currentToken != null && currentToken is not ParenthesisClosed)
        {
            // Collect tokens for this argument until we hit a comma or closing paren
            List<Token> argumentTokens = new();
            int parenDepth = 0;

            while (currentToken != null)
            {
                // Track parenthesis depth for nested expressions
                if (currentToken is ParenthesisOpen)
                {
                    parenDepth++;
                    argumentTokens.Add(currentToken);
                    currentToken = currentToken.Next;
                    continue;
                }

                if (currentToken is ParenthesisClosed)
                {
                    if (parenDepth == 0)
                    {
                        // End of argument list
                        break;
                    }
                    parenDepth--;
                    argumentTokens.Add(currentToken);
                    currentToken = currentToken.Next;
                    continue;
                }

                // Comma at depth 0 means end of this argument
                if (currentToken is CommaToken && parenDepth == 0)
                {
                    currentToken = currentToken.Next; // Skip comma
                    break;
                }

                argumentTokens.Add(currentToken);
                currentToken = currentToken.Next;
            }

            // Build expression from collected tokens
            if (argumentTokens.Count > 0)
            {
                Expression argExpr = BuildSimpleExpression(argumentTokens);
                arguments.Add(argExpr);
            }
        }

        // CurrentToken should now be at ParenthesisClosed
        Token? nextToken = currentToken is ParenthesisClosed ? currentToken.Next : currentToken;
        return (arguments, nextToken);
    }

    /// <summary>
    ///     Builds a simple expression from a list of tokens.
    ///     Supports literals, identifiers, and binary operations.
    /// </summary>
    private static Expression BuildSimpleExpression(List<Token> tokens)
    {
        if (tokens.Count == 0)
        {
            throw new InvalidOperationException("Cannot build expression from empty token list");
        }

        // Ensure tokens are properly linked (they should already be from the original token stream)
        // But if we collected them into a list, we need to verify the .Next pointers
        for (int i = 0; i < tokens.Count - 1; i++)
        {
            if (tokens[i].Next != tokens[i + 1])
            {
                tokens[i].Next = tokens[i + 1];
            }
        }

        // Use the new ExpressionParser to handle nested function calls properly
        var parser = new ExpressionParser();
        Token currentToken = tokens[0];
        var expression = parser.Parse(ref currentToken);

        return expression;
    }

    private static FunctionCallExpression ParseFunctionCallInExpression(IdentifierToken functionNameToken)
    {
        Token openParen = functionNameToken.Next;
        var (arguments, _) = ParseFunctionArguments(openParen);

        FunctionCallExpression funcCall = new FunctionCallExpression
        {
            FunctionName = functionNameToken
        };
        funcCall.Arguments.AddRange(arguments);

        return funcCall;
    }
}