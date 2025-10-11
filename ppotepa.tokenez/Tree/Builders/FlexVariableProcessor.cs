using ppotepa.tokenez.Tree.Builders.Interfaces;
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
    /// Processes FLEX keyword for dynamic variable declarations.
    /// Syntax: FLEX variableName = expression
    /// </summary>
    internal class FlexVariableProcessor : ITokenProcessor
    {
        private readonly ExpectationValidator _validator;

        public FlexVariableProcessor(ExpectationValidator validator)
        {
            _validator = validator;
        }

        public bool CanProcess(Token token)
        {
            return token is FlexKeywordToken;
        }

        public TokenProcessingResult Process(Token token, ProcessingContext context)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"[FlexVariableProcessor] Processing FLEX variable declaration in scope '{context.CurrentScope.ScopeName}'");
            Console.ResetColor();

            var flexToken = token as FlexKeywordToken;
            var currentToken = flexToken!.Next;

            // Expect identifier (variable name)
            if (currentToken is not IdentifierToken identifierToken)
            {
                throw new Exception($"Expected variable name after FLEX keyword, got {currentToken?.GetType().Name}");
            }

            string variableName = identifierToken.RawToken!.Text;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[FlexVariableProcessor] Variable name: {variableName}");
            Console.ResetColor();

            currentToken = currentToken.Next;

            // Check if this is an array element assignment (FLEX arr[0] = value)
            if (currentToken is BracketOpen)
            {
                // Parse array index assignment
                currentToken = currentToken.Next; // Move past '['
                var indexExpr = ParseExpression(ref currentToken);

                // Expect closing bracket
                if (currentToken is not BracketClosed)
                {
                    throw new Exception($"Expected ']' after array index but found {currentToken?.GetType().Name}");
                }

                currentToken = currentToken.Next; // Skip ']'

                // Expect assignment operator
                if (currentToken is not EqualsToken)
                {
                    throw new Exception($"Expected = after array index, got {currentToken?.GetType().Name}");
                }

                currentToken = currentToken.Next;

                // Parse the value expression
                Expression? valueExpr = ParseExpression(ref currentToken);

                // Create an ArrayAssignmentStatement
                var arrayAssignStmt = new ArrayAssignmentStatement(identifierToken, indexExpr, valueExpr);
                arrayAssignStmt.StartToken = flexToken;

                context.CurrentScope.Statements.Add(arrayAssignStmt);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[FlexVariableProcessor] Registered array assignment '{variableName}[...] = ...' in scope '{context.CurrentScope.ScopeName}'");
                Console.ResetColor();

                return new TokenProcessingResult
                {
                    NextToken = currentToken,
                    ShouldValidateExpectations = false
                };
            }

            // Expect assignment operator
            if (currentToken is not EqualsToken)
            {
                throw new Exception($"Expected = after variable name '{variableName}', got {currentToken?.GetType().Name}");
            }

            currentToken = currentToken.Next;

            // Parse the initialization expression
            Expression? initExpression = ParseExpression(ref currentToken);

            // Create variable declaration
            var variableDecl = new VariableDeclaration(null, identifierToken); // No type token for FLEX variables

            // Create variable declaration statement
            var statement = new VariableDeclarationStatement(variableDecl, initExpression, isDynamic: true);
            statement.StartToken = flexToken;

            // Add to current scope
            context.CurrentScope.Statements.Add(statement);
            context.CurrentScope.AddDynamicVariable(variableName);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[FlexVariableProcessor] Registered FLEX variable '{variableName}' in scope '{context.CurrentScope.ScopeName}'");
            Console.ResetColor();

            return new TokenProcessingResult
            {
                NextToken = currentToken,
                ShouldValidateExpectations = false
            };
        }


        /// <summary>
        /// Parses an expression with support for binary operations.
        /// Handles operator precedence: * / before + -
        /// </summary>
        private Expression? ParseExpression(ref Token? token)
        {
            // Parse additive expression (lowest precedence)
            return ParseAdditiveExpression(ref token);
        }

        /// <summary>
        /// Parses addition and subtraction (left-to-right)
        /// </summary>
        private Expression ParseAdditiveExpression(ref Token? token)
        {
            var left = ParseMultiplicativeExpression(ref token);

            while (token is PlusToken or MinusToken)
            {
                var operatorToken = token;
                token = token.Next;

                var right = ParseMultiplicativeExpression(ref token);

                left = new BinaryExpression(left, operatorToken, right);
            }

            return left;
        }

        /// <summary>
        /// Parses multiplication and division (left-to-right)
        /// </summary>
        private Expression ParseMultiplicativeExpression(ref Token? token)
        {
            var left = ParsePrimaryExpression(ref token);

            while (token is MultiplyToken or DivideToken)
            {
                var operatorToken = token;
                token = token.Next;

                var right = ParsePrimaryExpression(ref token);

                left = new BinaryExpression(left, operatorToken, right);
            }

            return left;
        }

        /// <summary>
        /// Parses primary expressions: literals, identifiers, function calls, parentheses
        /// </summary>
        private Expression ParsePrimaryExpression(ref Token? token)
        {
            // Handle parentheses for precedence override
            if (token is ParenthesisOpen)
            {
                token = token.Next; // Skip '('

                // Recursively parse the expression inside parentheses
                var innerExpr = ParseExpression(ref token);

                // Expect closing parenthesis
                if (token is not ParenthesisClosed)
                {
                    throw new Exception($"Expected ')' but found {token?.GetType().Name}");
                }

                token = token.Next; // Skip ')'
                return innerExpr;
            }
            else if (token is BracketOpen)
            {
                // Array literal: [1, 2, 3, 4]
                token = token.Next; // Move past '['
                
                var elements = new List<Expression>();
                
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
                    var elementExpr = ParsePrimaryExpression(ref token);
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
                        throw new Exception($"Expected ',' or ']' in array literal, got {token?.GetType().Name}");
                    }
                }
                
                if (token is not BracketClosed)
                {
                    throw new Exception($"Expected ']' to close array literal, got {token?.GetType().Name}");
                }
                
                token = token.Next; // Move past ']'
                return new ArrayLiteralExpression(elements);
            }
            else if (token is ChainToken)
            {
                // CHAIN keyword for array creation: CHAIN <size>
                token = token.Next; // Move past CHAIN

                if (token is not ValueToken sizeToken)
                {
                    throw new Exception($"Expected array size after CHAIN keyword, got {token?.GetType().Name}");
                }

                var expr = new ArrayCreationExpression(sizeToken);
                token = token.Next;
                return expr;
            }
            else if (token is ValueToken valueToken)
            {
                var expr = new LiteralExpression(valueToken);
                token = token.Next;
                return expr;
            }
            else if (token is StringLiteralToken stringToken)
            {
                var expr = new StringLiteralExpression(stringToken);
                token = token.Next;
                return expr;
            }
            else if (token is IdentifierToken identifierToken)
            {
                // Check if it's an array index access
                if (identifierToken.Next is BracketOpen)
                {
                    token = identifierToken.Next.Next; // Move past '[' to index expression
                    var indexExpr = ParseExpression(ref token);

                    // Expect closing bracket
                    if (token is not BracketClosed)
                    {
                        throw new Exception($"Expected ']' after array index but found {token?.GetType().Name}");
                    }

                    token = token.Next; // Skip ']'

                    return new IndexExpression
                    {
                        ArrayIdentifier = identifierToken,
                        Index = indexExpr
                    };
                }
                // Check if it's a function call
                else if (identifierToken.Next is ParenthesisOpen)
                {
                    var funcCallExpr = new FunctionCallExpression
                    {
                        FunctionName = identifierToken
                    };
                    token = identifierToken.Next; // Move to (
                    token = token!.Next; // Move past (

                    // TODO: Parse function call arguments

                    // Skip to )
                    while (token != null && token is not ParenthesisClosed)
                    {
                        token = token.Next;
                    }

                    if (token is ParenthesisClosed)
                    {
                        token = token.Next;
                    }

                    return funcCallExpr;
                }
                else
                {
                    var expr = new IdentifierExpression(identifierToken);
                    token = token.Next;
                    return expr;
                }
            }
            else
            {
                throw new NotImplementedException($"Expression type {token?.GetType().Name} not yet supported in FLEX variable initialization");
            }
        }
    }
}

