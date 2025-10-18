using PowerScript.Common.Logging;
using PowerScript.Core.AST.Expressions;
using PowerScript.Core.AST.Statements;
using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Delimiters;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Keywords;
using PowerScript.Core.Syntax.Tokens.Operators;
using PowerScript.Core.Syntax.Tokens.Values;
using PowerScript.Parser.Processors.Base;

namespace PowerScript.Parser.Processors.Statements;

/// <summary>
///     Processes .NET member access statements using arrow operator.
///     Handles syntax like: ##Console -> WriteLine(42) or #myObject -> SomeMethod()
///     Requires # prefix before the identifier to indicate .NET interop.
/// </summary>
public class NetMemberAccessStatementProcessor : ITokenProcessor
{
    public bool CanProcess(Token token)
    {
        // Check if this is a # followed by identifier followed by arrow operator
        if (token is not NetKeywordToken)
        {
            return false;
        }

        // Look ahead to see if next token is identifier
        Token nextToken = token.Next;
        if (nextToken is not IdentifierToken)
        {
            return false;
        }

        // And then arrow operator
        Token arrowToken = nextToken.Next;
        return arrowToken is ArrowToken;
    }

    public TokenProcessingResult Process(Token token, ProcessingContext context)
    {
        LoggerService.Logger.Debug(
            $"NetMemberAccessStatementProcessor: Processing '#... -> ...' in scope '{context.CurrentScope.ScopeName}'");

        // Skip the # token
        Token currentToken = token.Next;

        IdentifierToken objectToken = (IdentifierToken)currentToken;
        string objectName = objectToken.RawToken.OriginalText; // Preserve casing for .NET types

        // Skip the identifier and arrow operator
        currentToken = currentToken.Next.Next;

        // Expect member name (method or property)
        if (currentToken is not IdentifierToken memberToken)
        {
            throw new InvalidOperationException(
                $"Expected member name after '->', got {currentToken?.GetType().Name}");
        }

        string memberName = memberToken.RawToken.OriginalText; // Preserve casing
        currentToken = memberToken.Next;

        // Check if it's a method call (has parentheses)
        if (currentToken is ParenthesisOpen)
        {
            // Parse method arguments
            List<Expression> arguments = ParseArguments(ref currentToken);

            // Build the full path (e.g., "Console.WriteLine")
            string fullPath = $"{objectName}.{memberName}";

            // Create the .NET method call statement
            NetMethodCallStatement methodCall = new(fullPath, arguments)
            {
                StartToken = token
            };

            // Add the statement to the current scope
            context.CurrentScope.Statements.Add(methodCall);

            LoggerService.Logger.Debug(
                $"NetMemberAccessStatementProcessor: Added method call '{fullPath}' with {arguments.Count} argument(s) to scope '{context.CurrentScope.ScopeName}'");

            return TokenProcessingResult.Continue(currentToken);
        }
        else
        {
            // Property access without parentheses - this is more complex
            // For now, throw an error since we need to handle this as an expression evaluation
            throw new InvalidOperationException(
                $"Property access '{objectName} -> {memberName}' without method call is not supported as a statement. Use it in an assignment instead.");
        }
    }

    /// <summary>
    ///     Parses method arguments inside parentheses
    /// </summary>
    private static List<Expression> ParseArguments(ref Token token)
    {
        List<Expression> arguments = [];

        // Skip opening parenthesis
        token = token.Next;

        // Parse arguments until we hit closing parenthesis
        while (token is not ParenthesisClosed)
        {
            // Handle empty argument list
            if (token is ParenthesisClosed)
            {
                break;
            }

            // Parse single argument expression
            Expression arg = ParseArgumentExpression(ref token);
            arguments.Add(arg);

            // Handle comma for multiple arguments
            if (token is CommaSeparatorToken)
            {
                token = token.Next;
            }
            else if (token is not ParenthesisClosed)
            {
                throw new InvalidOperationException(
                    $"Expected comma or closing parenthesis, got {token?.GetType().Name}");
            }
        }

        // Skip closing parenthesis
        if (token is ParenthesisClosed)
        {
            token = token.Next;
        }

        return arguments;
    }

    /// <summary>
    ///     Parses a single argument expression (literal, identifier, or more complex expression)
    /// </summary>
    private static Expression ParseArgumentExpression(ref Token token)
    {
        // Handle string literals
        if (token is StringLiteralToken stringToken)
        {
            Expression expr = new StringLiteralExpression(stringToken);
            token = token.Next;
            return expr;
        }

        // Handle numeric literals
        if (token is ValueToken valueToken)
        {
            Expression expr = new LiteralExpression(valueToken);
            token = token.Next;
            return expr;
        }

        // Handle TRUE keyword
        if (token is TrueToken trueToken)
        {
            Expression expr = new LiteralExpression(trueToken);
            token = token.Next;
            return expr;
        }

        // Handle FALSE keyword
        if (token is FalseToken falseToken)
        {
            Expression expr = new LiteralExpression(falseToken);
            token = token.Next;
            return expr;
        }

        // Handle identifiers (variable references)
        if (token is IdentifierToken identifierToken)
        {
            Expression expr = new IdentifierExpression(identifierToken);
            token = token.Next;
            return expr;
        }

        throw new InvalidOperationException(
            $"Unsupported expression type in argument: {token?.GetType().Name}");
    }
}
