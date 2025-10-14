using Tokenez.Common.Logging;
using Tokenez.Core.AST.Statements;
using Tokenez.Core.Exceptions;
using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Keywords;
using Tokenez.Core.Syntax.Tokens.Values;
using Tokenez.Parser.Processors.Base;

namespace Tokenez.Parser.Processors.Statements;

/// <summary>
///     Processes EXECUTE commands for running external PowerScript files.
///     Syntax: EXECUTE "filename.ps"
/// </summary>
public class ExecuteCommandProcessor : ITokenProcessor
{
    public bool CanProcess(Token token)
    {
        return token is ExecuteKeywordToken;
    }

    public TokenProcessingResult Process(Token token, ProcessingContext context)
    {
        LoggerService.Logger.Debug(
            $"ExecuteCommandProcessor: Processing EXECUTE command in scope '{context.CurrentScope.ScopeName}'");

        ExecuteKeywordToken? executeToken = token as ExecuteKeywordToken;
        Token nextToken = executeToken!.Next;

        // Parse the file path (should be a string literal)
        string filePath;
        if (nextToken is StringLiteralToken strLiteral)
        {
            filePath = strLiteral.RawToken.OriginalText;
            // Remove quotes from the string literal
            filePath = filePath.Trim('"');
            nextToken = strLiteral.Next;
        }
        else
        {
            throw new UnexpectedTokenException(nextToken!, typeof(StringLiteralToken));
        }

        LoggerService.Logger.Debug($"Parsed EXECUTE file path: {filePath}");

        // Create and register the execute statement
        ExecuteStatement executeStatement = new(filePath)
        {
            StartToken = executeToken
        };

        context.CurrentScope.Statements.Add(executeStatement);

        LoggerService.Logger.Debug(
            $"Registered EXECUTE statement for file '{filePath}' in scope '{context.CurrentScope.ScopeName}'");

        return new TokenProcessingResult
        {
            NextToken = nextToken,
            ShouldValidateExpectations = false
        };
    }
}