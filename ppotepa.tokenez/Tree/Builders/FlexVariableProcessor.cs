using ppotepa.tokenez.Logging;
using ppotepa.tokenez.Tree.Builders.Interfaces;
using ppotepa.tokenez.Tree.Exceptions;
using ppotepa.tokenez.Tree.Expressions;
using ppotepa.tokenez.Tree.Statements;
using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Delimiters;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Keywords;
using ppotepa.tokenez.Tree.Tokens.Keywords.Types;
using ppotepa.tokenez.Tree.Tokens.Operators;
using ppotepa.tokenez.Tree.Tokens.Values;

namespace ppotepa.tokenez.Tree.Builders
{
    /// <summary>
    ///     Processes FLEX keyword for dynamic variable declarations.
    ///     Syntax: FLEX variableName = expression
    /// </summary>
    internal class FlexVariableProcessor : ITokenProcessor
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

            // Check if this is an array element assignment (FLEX arr[0] = value)
            if (currentToken is BracketOpen)
            {
                // Parse array index assignment
                currentToken = currentToken.Next; // Move past '['
                Expression indexExpr = ParseExpression(ref currentToken);

                // Expect closing bracket
                if (currentToken is not BracketClosed)
                {
                    throw new UnexpectedTokenException(currentToken!, typeof(BracketClosed));
                }

                currentToken = currentToken.Next; // Skip ']'

                // Expect assignment operator
                if (currentToken is not EqualsToken)
                {
                    throw new UnexpectedTokenException(currentToken!, typeof(EqualsToken));
                }

                currentToken = currentToken.Next;

                // Parse the value expression
                Expression valueExpr = ParseExpression(ref currentToken);

                // Create an ArrayAssignmentStatement
                ArrayAssignmentStatement arrayAssignStmt = new(identifierToken, indexExpr, valueExpr)
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

            // Parse the initialization expression
            Expression initExpression = ParseExpression(ref currentToken);

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
        ///     Parses multiplication and division (left-to-right)
        /// </summary>
        private Expression ParseMultiplicativeExpression(ref Token? token)
        {
            Expression left = ParsePrimaryExpression(ref token);

            while (token is MultiplyToken or DivideToken)
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
                // CHAIN keyword for array creation: CHAIN <size>
                token = token.Next; // Move past CHAIN

                if (token is not ValueToken sizeToken)
                {
                    throw new UnexpectedTokenException(token!, typeof(ValueToken));
                }

                ArrayCreationExpression expr = new(sizeToken);
                token = token.Next;
                return expr;
            }

            if (token is ValueToken valueToken)
            {
                LiteralExpression expr = new(valueToken);
                token = token.Next;
                return expr;
            }

            if (token is StringLiteralToken stringToken)
            {
                StringLiteralExpression expr = new(stringToken);
                token = token.Next;
                return expr;
            }

            if (token is IdentifierToken identifierToken)
            {
                // Check if it's an array index access
                if (identifierToken.Next is BracketOpen)
                {
                    token = identifierToken.Next.Next; // Move past '[' to index expression
                    Expression indexExpr = ParseExpression(ref token);

                    // Expect closing bracket
                    if (token is not BracketClosed)
                    {
                        throw new UnexpectedTokenException(token!, typeof(BracketClosed));
                    }

                    token = token.Next; // Skip ']'

                    return new IndexExpression
                    {
                        ArrayIdentifier = identifierToken,
                        Index = indexExpr
                    };
                }
                // Check if it's a function call

                if (identifierToken.Next is ParenthesisOpen)
                {
                    FunctionCallExpression funcCallExpr = new()
                    {
                        FunctionName = identifierToken
                    };
                    token = identifierToken.Next; // Move to (
                    token = token!.Next; // Move past (

                    // Parse function call arguments - not implemented yet

                    // Skip to )
                    while (token is not null and not ParenthesisClosed)
                    {
                        token = token.Next;
                    }

                    if (token is ParenthesisClosed)
                    {
                        token = token.Next;
                    }

                    return funcCallExpr;
                }

                IdentifierExpression expr = new(identifierToken);
                token = token.Next;
                return expr;
            }

            throw new NotImplementedException(
                $"Expression type {token?.GetType().Name} not yet supported in FLEX variable initialization");
        }
    }
}