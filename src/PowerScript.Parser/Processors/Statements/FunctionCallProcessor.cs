using PowerScript.Common.Logging;
using PowerScript.Core.AST.Expressions;
using PowerScript.Core.AST.Statements;
using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Delimiters;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Values;
using PowerScript.Parser.Processors.Base;

namespace PowerScript.Parser.Processors.Statements;

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

        // Parse function arguments
        List<Token> arguments = [];
        Token currentToken = openParen.Next;

        // Check for empty parameter list
        if (currentToken is ParenthesisClosed)
        {
            // Empty parameter list - create statement with no arguments
            FunctionCallStatement emptyStatement = new()
            {
                FunctionName = functionName,
                StartToken = token,
                Arguments = arguments
            };

            context.CurrentScope.Statements.Add(emptyStatement);

            LoggerService.Logger.Debug(
                $"FunctionCallProcessor: Added function call '{functionName}()' with no arguments to scope '{context.CurrentScope.ScopeName}'");

            return TokenProcessingResult.Continue(currentToken.Next);
        }

        // Parse arguments using ExpressionParser to support nested function calls
        var parser = new ExpressionParser();

        while (currentToken != null && currentToken is not ParenthesisClosed)
        {
            // Parse the argument as an expression (supports nested function calls, operators, etc.)
            var expression = parser.Parse(ref currentToken);

            // Convert expression to token(s) for backwards compatibility
            if (expression is LiteralExpression literalExpr)
            {
                arguments.Add(literalExpr.Value);
            }
            else if (expression is StringLiteralExpression stringExpr)
            {
                arguments.Add(stringExpr.Value);
            }
            else if (expression is IdentifierExpression identifierExpr)
            {
                arguments.Add(identifierExpr.Identifier);
            }
            else if (expression is FunctionCallExpression funcCallExpr)
            {
                // For nested function calls, we need to pass the expression itself
                // Create a temporary token to hold the expression
                arguments.Add(funcCallExpr.FunctionName);
            }
            else if (expression is BinaryExpression binaryExpr)
            {
                // For complex expressions (like a % b), we need to handle them differently
                // For now, add the first token of the expression
                // TODO: This may need better handling for complex expressions
                if (binaryExpr.Left is IdentifierExpression leftIdent)
                {
                    arguments.Add(leftIdent.Identifier);
                }
                else if (binaryExpr.Left is LiteralExpression leftValue)
                {
                    arguments.Add(leftValue.Value);
                }
            }

            // Check for comma (more arguments) or closing parenthesis (done)
            if (currentToken is CommaToken)
            {
                currentToken = currentToken.Next; // Skip comma
            }
            else if (currentToken is not ParenthesisClosed)
            {
                throw new InvalidOperationException(
                    $"Expected ',' or ')' in function call '{functionName}', but found '{currentToken.GetType().Name}'");
            }
        }

        if (currentToken is not ParenthesisClosed)
        {
            throw new InvalidOperationException(
                $"Expected ')' to close function call '{functionName}'");
        }

        // Create the function call statement with arguments
        FunctionCallStatement statement = new()
        {
            FunctionName = functionName,
            StartToken = token,
            Arguments = arguments
        };

        // Add the statement to the current scope
        context.CurrentScope.Statements.Add(statement);

        LoggerService.Logger.Debug(
            $"FunctionCallProcessor: Added function call '{functionName}()' with {arguments.Count} argument(s) to scope '{context.CurrentScope.ScopeName}'");

        // Continue from the token after the closing parenthesis
        return TokenProcessingResult.Continue(currentToken.Next);
    }
}
