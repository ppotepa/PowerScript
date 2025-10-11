using ppotepa.tokenez.Tree.Builders.Interfaces;
using ppotepa.tokenez.Tree.Expressions;
using ppotepa.tokenez.Tree.Statements;
using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Delimiters;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Keywords;
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
        /// Parses a simple expression (literal, identifier, or binary expression)
        /// </summary>
        private Expression? ParseExpression(ref Token? token)
        {
            if (token is ValueToken valueToken)
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
                // Check if it's a function call
                if (identifierToken.Next is ParenthesisOpen)
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
