using ppotepa.tokenez.Logging;
using ppotepa.tokenez.Tree.Builders.Interfaces;
using ppotepa.tokenez.Tree.Expressions;
using ppotepa.tokenez.Tree.Statements;
using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Delimiters;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Keywords;
using ppotepa.tokenez.Tree.Tokens.Values;

namespace ppotepa.tokenez.Tree.Builders
{
    /// <summary>
    ///     Processes PRINT keyword tokens.
    ///     Responsible for:
    ///     - Parsing the expression to print (string literal, function call, etc.)
    ///     - Creating PrintStatement objects
    ///     - Registering print statements in the current scope
    /// </summary>
    internal class PrintStatementProcessor : ITokenProcessor
    {

        public bool CanProcess(Token token)
        {
            return token is PrintKeywordToken;
        }

        public TokenProcessingResult Process(Token token, ProcessingContext context)
        {
            LoggerService.Logger.Debug(
                $"PrintStatementProcessor: Processing PRINT token '{token.RawToken?.Text}' in scope '{context.CurrentScope.ScopeName}'");

            var printToken = token as PrintKeywordToken;
            var nextToken = printToken!.Next;

            // Parse the expression to print
            Expression? expression = null;

            if (nextToken is TemplateStringToken templateToken)
            {
                // Template string with variable interpolation
                expression = new TemplateStringExpression(templateToken);
                nextToken = templateToken.Next;
            }
            else if (nextToken is StringLiteralToken stringToken)
            {
                // Simple string literal
                expression = new StringLiteralExpression(stringToken);
                nextToken = stringToken.Next;
            }
            else if (nextToken is IdentifierToken identifierToken)
            {
                // Check if it's a function call
                if (identifierToken.Next is ParenthesisOpen)
                {
                    // Function call expression: PRINT function(args)
                    expression = new FunctionCallExpression
                    {
                        FunctionName = identifierToken
                    };
                    // Skip to closing parenthesis
                    var currentToken = identifierToken.Next.Next; // Move past '('
                    int depth = 1;
                    while (currentToken != null && depth > 0)
                    {
                        if (currentToken is ParenthesisOpen) depth++;
                        if (currentToken is ParenthesisClosed) depth--;
                        if (depth > 0) currentToken = currentToken.Next;
                    }
                    nextToken = currentToken?.Next; // Move past ')'
                }
                // Check if it's an array index access
                else if (identifierToken.Next is BracketOpen)
                {
                    // Parse array indexing expression
                    var currentToken = identifierToken.Next.Next; // Move past '[' 
                    var indexExpr = ParseSimpleExpression(ref currentToken);

                    // Expect closing bracket
                    if (currentToken is not BracketClosed)
                        throw new InvalidOperationException($"Expected ']' after array index but found {currentToken?.GetType().Name}");

                    expression = new IndexExpression
                    {
                        ArrayIdentifier = identifierToken,
                        Index = indexExpr
                    };
                    nextToken = currentToken.Next; // Skip ']'
                }
                else
                {
                    // Simple variable or function call
                    expression = new IdentifierExpression(identifierToken);
                    nextToken = identifierToken.Next;
                }
            }
            else if (nextToken is ValueToken valueToken)
            {
                // Numeric literal
                expression = new LiteralExpression(valueToken);
                nextToken = valueToken.Next;
            }
            else
            {
                // TODO: Handle other expression types (binary expressions, etc.)
                throw new NotImplementedException(
                    $"PRINT does not yet support expression type: {nextToken?.GetType().Name}");
            }

            // Create and register the print statement
            PrintStatement printStatement = new(expression)
            {
                StartToken = printToken
            };

            context.CurrentScope.Statements.Add(printStatement);

            LoggerService.Logger.Debug($"Registered PRINT statement in scope '{context.CurrentScope.ScopeName}'");

            // Don't validate expectations since we already handled the token sequence
            return new TokenProcessingResult
            {
                NextToken = nextToken ?? printToken,
                ShouldValidateExpectations = false
            };
        }

        /// <summary>
        ///     Parses a simple expression (literal, identifier, or value).
        /// </summary>
        private Expression ParseSimpleExpression(ref Token currentToken)
        {
            if (currentToken is ValueToken valueToken)
            {
                LiteralExpression expr = new(valueToken);
                currentToken = currentToken.Next;
                return expr;
            }

            if (currentToken is IdentifierToken identifierToken)
            {
                IdentifierExpression expr = new(identifierToken);
                currentToken = currentToken.Next;
                return expr;
            }

            throw new InvalidOperationException($"Expected expression but found {currentToken?.GetType().Name}");
        }
    }
}