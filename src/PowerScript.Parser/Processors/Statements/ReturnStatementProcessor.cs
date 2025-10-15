using PowerScript.Common.Logging;
using PowerScript.Core.AST.Expressions;
using PowerScript.Core.AST.Statements;
using PowerScript.Core.Exceptions;
using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Delimiters;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Keywords;
using PowerScript.Core.Syntax.Tokens.Operators;
using PowerScript.Core.Syntax.Tokens.Scoping;
using PowerScript.Core.Syntax.Tokens.Values;
using PowerScript.Parser.Processors.Base;

namespace PowerScript.Parser.Processors.Statements;

/// <summary>
///     Processes RETURN keyword tokens.
///     Responsible for:
///     - Validating RETURN is inside a function
///     - Parsing the return expression (or null for void returns)
///     - Marking the scope as having a valid RETURN statement
///     Supports both value returns (RETURN expr) and void returns (RETURN)
/// </summary>
public class ReturnStatementProcessor : ITokenProcessor
{
    public bool CanProcess(Token token)
    {
        return token is ReturnKeywordToken;
    }

    public TokenProcessingResult Process(Token token, ProcessingContext context)
    {
        LoggerService.Logger.Debug(
            $"ReturnStatementProcessor: Processing RETURN token '{token.RawToken?.Text}' in scope '{context.CurrentScope.ScopeName}'");

        // Parse the expression after RETURN keyword
        Token? currentToken = token.Next;
        Expression? expression = currentToken is null or ScopeEndToken ? null : ParseFullExpression(ref currentToken);

        // Enforce language rule: RETURN with a value can only appear inside functions
        // Void RETURN is allowed anywhere (acts as early exit/break)
        if (!context.IsInsideFunction && expression != null)
        {
            throw new InvalidReturnStatementException(token);
        }

        // Create and register the RETURN statement
        ReturnStatement returnStatement = new(token, expression)
        {
            StartToken = token
        };
        context.CurrentScope.Statements.Add(returnStatement);
        context.CurrentScope.HasReturn = true; // Mark scope as having RETURN

        LoggerService.Logger.Debug($"Registered RETURN statement in scope '{context.CurrentScope.ScopeName}'");

        // currentToken now points to the token after the expression
        LoggerService.Logger.Debug(
            $"ReturnStatementProcessor: Next token after RETURN is {currentToken?.GetType().Name} '{currentToken?.RawToken?.Text}'");
        return TokenProcessingResult.Continue(currentToken!);
    }

    /// <summary>
    ///     Parse a full expression including comparisons and logical operators
    /// </summary>
    private Expression ParseFullExpression(ref Token? token)
    {
        Expression left = ParseArithmeticExpression(ref token);

        // Handle comparison operators
        if (token is GreaterThanToken or LessThanToken or GreaterThanOrEqualToken or
            LessThanOrEqualToken or EqualsEqualsToken or NotEqualsToken)
        {
            Token comparisonOp = token;
            token = token.Next;
            Expression right = ParseArithmeticExpression(ref token);
            return new BinaryExpression(left, comparisonOp, right);
        }

        // Handle == as two EqualsToken (tokenizer fallback)
        if (token is EqualsToken && token.Next is EqualsToken)
        {
            Token equalsOp = token;
            token = token.Next.Next; // Skip both = tokens
            Expression right = ParseArithmeticExpression(ref token);
            return new BinaryExpression(left, equalsOp, right);
        }

        return left;
    }

    /// <summary>
    ///     Parse arithmetic expression with support for +, -, *, /, % operators
    /// </summary>
    private Expression ParseArithmeticExpression(ref Token? token)
    {
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
    ///     Parses primary expressions: literals, identifiers, parentheses
    /// </summary>
    private Expression ParsePrimaryExpression(ref Token? token)
    {
        // Handle parentheses for precedence override
        if (token is ParenthesisOpen)
        {
            token = token.Next; // Skip '('

            Expression innerExpr = ParseFullExpression(ref token);

            if (token is not ParenthesisClosed)
            {
                throw new UnexpectedTokenException(token!, typeof(ParenthesisClosed));
            }

            token = token.Next; // Skip ')'
            return innerExpr;
        }

        // Handle identifiers (including function calls)
        if (token is IdentifierToken identToken)
        {
            // Check for function call: identifier(...)
            if (identToken.Next is ParenthesisOpen)
            {
                FunctionCallExpression funcCall = new()
                {
                    FunctionName = identToken
                };

                token = identToken.Next; // Move to '('
                token = token.Next; // Move past '('

                // Parse function arguments
                var (arguments, nextToken) = ParseFunctionArguments(token);
                funcCall.Arguments.AddRange(arguments);

                token = nextToken; // Move past ')'
                return funcCall;
            }

            token = token.Next;
            return new IdentifierExpression(identToken);
        }

        // Handle literal values
        if (token is ValueToken valueToken)
        {
            token = token.Next;
            return new LiteralExpression(valueToken);
        }

        throw new UnexpectedTokenException(token!, typeof(IdentifierToken), typeof(ValueToken),
            typeof(ParenthesisOpen));
    }

    /// <summary>
    /// Parses function arguments from tokens between parentheses.
    /// Returns list of argument expressions and the token after the closing paren.
    /// </summary>
    private static (List<Expression> arguments, Token? nextToken) ParseFunctionArguments(Token tokenAfterOpenParen)
    {
        var arguments = new List<Expression>();
        Token? token = tokenAfterOpenParen;

        // Empty argument list
        if (token is ParenthesisClosed)
        {
            return (arguments, token.Next);
        }

        // Parse arguments separated by commas
        while (token is not null && token is not ParenthesisClosed)
        {
            // Collect tokens for this argument until we hit a comma or closing paren
            var argTokens = new List<Token>();
            int parenDepth = 0;

            while (token is not null)
            {
                if (token is ParenthesisOpen)
                {
                    parenDepth++;
                    argTokens.Add(token);
                    token = token.Next;
                }
                else if (token is ParenthesisClosed)
                {
                    if (parenDepth == 0)
                    {
                        // End of arguments
                        break;
                    }
                    parenDepth--;
                    argTokens.Add(token);
                    token = token.Next;
                }
                else if (token is CommaToken && parenDepth == 0)
                {
                    // End of this argument
                    token = token.Next; // Skip comma
                    break;
                }
                else
                {
                    argTokens.Add(token);
                    token = token.Next;
                }
            }

            // Build expression from collected tokens
            if (argTokens.Count > 0)
            {
                var argExpression = BuildSimpleExpression(argTokens);
                arguments.Add(argExpression);
            }
        }

        // token should now be at ParenthesisClosed
        if (token is not ParenthesisClosed)
        {
            throw new UnexpectedTokenException(token!, typeof(ParenthesisClosed));
        }

        return (arguments, token.Next);
    }

    /// <summary>
    /// Builds a simple expression from a list of tokens.
    /// </summary>
    private static Expression BuildSimpleExpression(List<Token> tokens)
    {
        if (tokens.Count == 0)
        {
            throw new InvalidOperationException("Cannot build expression from empty token list");
        }

        if (tokens.Count == 1)
        {
            // Single token
            return tokens[0] switch
            {
                IdentifierToken ident => new IdentifierExpression(ident),
                ValueToken val => new LiteralExpression(val),
                StringLiteralToken strLit => new StringLiteralExpression(strLit),
                _ => throw new UnexpectedTokenException(tokens[0], "Expected identifier, value, or string literal")
            };
        }

        // Simple binary expression: left operator right
        if (tokens.Count == 3)
        {
            var left = BuildSimpleExpression(new List<Token> { tokens[0] });
            var right = BuildSimpleExpression(new List<Token> { tokens[2] });
            return new BinaryExpression(left, tokens[1], right);
        }

        // For now, just return the first token as an expression
        // TODO: Handle more complex expressions
        return BuildSimpleExpression(new List<Token> { tokens[0] });
    }
}