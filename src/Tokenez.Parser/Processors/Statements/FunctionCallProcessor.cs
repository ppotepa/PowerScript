using Tokenez.Common.Logging;
using Tokenez.Core.AST.Statements;
using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Delimiters;
using Tokenez.Core.Syntax.Tokens.Identifiers;
using Tokenez.Parser.Processors.Base;

namespace Tokenez.Parser.Processors.Statements
{
    /// <summary>
    ///     Processes function call statements: identifier followed by parentheses.
    ///     Example: sayHello() or add(5, 3)
    /// </summary>
    public class FunctionCallProcessor : ITokenProcessor
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
            LoggerService.Logger.Debug(
                $"FunctionCallProcessor: Processing function call '{token.RawToken?.Text}' in scope '{context.CurrentScope.ScopeName}'");

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

            LoggerService.Logger.Debug(
                $"FunctionCallProcessor: Added function call '{functionName}()' to scope '{context.CurrentScope.ScopeName}'");

            // Continue from the token after the closing parenthesis
            return TokenProcessingResult.Continue(closeParen.Next);
        }
    }
}
