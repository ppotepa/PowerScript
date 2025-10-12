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
    ///     Processes NET:: method call syntax for direct .NET framework access.
    ///     Handles syntax like: NET::System.Console.WriteLine("Hello")
    /// </summary>
    internal class NetMethodCallProcessor : ITokenProcessor
    {

        public bool CanProcess(Token token)
        {
            return token is NetKeywordToken;
        }

        public TokenProcessingResult Process(Token token, ProcessingContext context)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(
                $"[DEBUG] NetMethodCallProcessor: Processing NET:: or # call in scope '{context.CurrentScope.ScopeName}'");
            Console.ResetColor();

            var netToken = token as NetKeywordToken;
            var currentToken = netToken!.Next;

            // Check if using # shorthand (no ::) or NET:: syntax
            var isShorthand = netToken.RawToken.Text == "#";

            if (!isShorthand)
            {
                // Expect namespace operator :: for NET syntax
                if (currentToken is not NamespaceOperatorToken)
                    throw new Exception($"Expected :: after NET keyword, got {currentToken?.GetType().Name}");
                currentToken = currentToken.Next;
            }

            // Parse the fully qualified .NET path (e.g., System.Console.WriteLine or Console.WriteLine)
            var fullPath = ParseDotPath(ref currentToken);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[DEBUG] Parsed .NET path: {fullPath}");
            Console.ResetColor();

            // Check for open parenthesis for parameters
            if (currentToken is not ParenthesisOpen)
                throw new Exception(
                    $"Expected opening parenthesis after .NET method name '{fullPath}', got {currentToken?.GetType().Name}");

            // Parse method arguments
            var arguments = ParseArguments(ref currentToken);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[DEBUG] Parsed {arguments.Count} argument(s)");
            Console.ResetColor();

            // Create the .NET method call statement
            NetMethodCallStatement netMethodCall = new(fullPath, arguments)
            {
                StartToken = netToken
            };

            // Add the statement to the current scope
            context.CurrentScope.Statements.Add(netMethodCall);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(
                $"[DEBUG] Registered .NET method call to '{fullPath}' in scope '{context.CurrentScope.ScopeName}'");
            Console.ResetColor();

            return new TokenProcessingResult
            {
                NextToken = currentToken,
                ShouldValidateExpectations = false
            };
        }

        /// <summary>
        ///     Parses a dot-separated path like System.Console.WriteLine
        ///     Preserves original casing for .NET type/method names
        /// </summary>
        private string ParseDotPath(ref Token token)
        {
            List<string> pathParts = [];

            while (token is IdentifierToken identifierToken)
            {
                // Use OriginalText to preserve casing for .NET type names
                pathParts.Add(identifierToken.RawToken.OriginalText);
                token = token.Next;

                // Check if next token is a dot
                if (token is DotToken)
                    token = token.Next; // Skip the dot
                else
                    break;
            }

            return pathParts.Count == 0
                ? throw new Exception("Expected at least one identifier in .NET path")
                : string.Join(".", pathParts);
        }

        /// <summary>
        ///     Parses method arguments inside parentheses
        /// </summary>
        private List<Expression> ParseArguments(ref Token token)
        {
            List<Expression> arguments = [];

            // Skip opening parenthesis
            token = token.Next;

            // Parse arguments until we hit closing parenthesis
            while (token is not ParenthesisClosed)
            {
                // Handle empty argument list
                if (token is ParenthesisClosed) break;

                // Parse single argument expression
                var arg = ParseArgumentExpression(ref token);
                arguments.Add(arg);

                // Handle comma for multiple arguments
                if (token is CommaSeparatorToken)
                    token = token.Next;
                else if (token is not ParenthesisClosed)
                    throw new Exception($"Expected comma or closing parenthesis, got {token?.GetType().Name}");
            }

            // Skip closing parenthesis
            if (token is ParenthesisClosed) token = token.Next;

            return arguments;
        }

        /// <summary>
        ///     Parses a single argument expression (string literal, number, identifier, etc.)
        /// </summary>
        private Expression ParseArgumentExpression(ref Token token)
        {
            // Handle string literals
            if (token is StringLiteralToken stringToken)
            {
                StringLiteralExpression expr = new(stringToken);
                token = token.Next;
                return expr;
            }

            // Handle numeric literals
            if (token is ValueToken valueToken)
            {
                LiteralExpression expr = new(valueToken);
                token = token.Next;
                return expr;
            }

            // Handle identifiers (variables, parameters)
            if (token is IdentifierToken identToken)
            {
                IdentifierExpression expr = new(identToken);
                token = token.Next;
                return expr;
            }

            throw new NotImplementedException(
                $"Expression type {token?.GetType().Name} not yet supported in .NET method call arguments");
        }
    }
}