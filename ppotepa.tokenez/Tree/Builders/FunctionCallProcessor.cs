using ppotepa.tokenez.Tree.Builders.Interfaces;
using ppotepa.tokenez.Tree.Statements;
using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Delimiters;
using ppotepa.tokenez.Tree.Tokens.Identifiers;

namespace ppotepa.tokenez.Tree.Builders
{
    /// <summary>
    ///     Processes function call statements: identifier followed by parentheses.
    ///     Example: sayHello() or add(5, 3)
    /// </summary>
    internal class FunctionCallProcessor : ITokenProcessor
    {
        public bool CanProcess(Token token)
        {
            // Check if this is an identifier followed by an opening parenthesis
            if (token is not IdentifierToken)
            {
                return false;
            }

            // Look ahead to see if next token is opening parenthesis
            Token nextToken = token.Next;
            return nextToken is ParenthesisOpen;
        }

        public TokenProcessingResult Process(Token token, ProcessingContext context)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(
                $"[DEBUG] FunctionCallProcessor: Processing function call '{token.RawToken?.Text}' in scope '{context.CurrentScope.ScopeName}'");
            Console.ResetColor();

            IdentifierToken identifierToken = (IdentifierToken)token;
            string functionName = identifierToken.RawToken?.Text?.ToUpperInvariant() ?? "";

            // Consume the opening parenthesis
            Token openParen = token.Next;
            if (openParen is not ParenthesisOpen)
            {
                throw new InvalidOperationException($"Expected '(' after function name '{functionName}'");
            }

            // For now, we only support parameterless function calls
            // Argument parsing will be implemented in future versions
            Token closeParen = openParen.Next;
            if (closeParen is not ParenthesisClosed)
            {
                throw new InvalidOperationException($"Expected ')' after function name '{functionName}()' - parameters not yet supported");
            }

            // Create the function call statement
            FunctionCallStatement statement = new()
            {
                FunctionName = functionName,
                StartToken = token,
                Arguments = [] // Empty for now
            };

            // Add the statement to the current scope
            context.CurrentScope.Statements.Add(statement);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(
                $"[DEBUG] FunctionCallProcessor: Added function call '{functionName}()' to scope '{context.CurrentScope.ScopeName}'");
            Console.ResetColor();

            // Continue from the token after the closing parenthesis
            return TokenProcessingResult.Continue(closeParen.Next);
        }
    }
}