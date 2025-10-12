using ppotepa.tokenez.Logging;
using ppotepa.tokenez.Tree.Builders.Interfaces;
using ppotepa.tokenez.Tree.Exceptions;
using ppotepa.tokenez.Tree.Statements;
using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Keywords;
using ppotepa.tokenez.Tree.Tokens.Values;

namespace ppotepa.tokenez.Tree.Builders
{
    /// <summary>
    ///     Processes EXECUTE commands for running external PowerScript files.
    ///     Syntax: EXECUTE "filename.ps"
    /// </summary>
    internal class ExecuteCommandProcessor : ITokenProcessor
    {

        public bool CanProcess(Token token)
        {
            return token is ExecuteKeywordToken;
        }

        public TokenProcessingResult Process(Token token, ProcessingContext context)
        {
            LoggerService.Logger.Debug(
                $"ExecuteCommandProcessor: Processing EXECUTE command in scope '{context.CurrentScope.ScopeName}'");

            var executeToken = token as ExecuteKeywordToken;
            var nextToken = executeToken!.Next;

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
}