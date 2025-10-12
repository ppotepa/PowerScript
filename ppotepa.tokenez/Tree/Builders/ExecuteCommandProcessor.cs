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
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(
                $"[DEBUG] ExecuteCommandProcessor: Processing EXECUTE command in scope '{context.CurrentScope.ScopeName}'");
            Console.ResetColor();

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

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[DEBUG] Parsed EXECUTE file path: {filePath}");
            Console.ResetColor();

            // Create and register the execute statement
            ExecuteStatement executeStatement = new(filePath)
            {
                StartToken = executeToken
            };

            context.CurrentScope.Statements.Add(executeStatement);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(
                $"[DEBUG] Registered EXECUTE statement for file '{filePath}' in scope '{context.CurrentScope.ScopeName}'");
            Console.ResetColor();

            return new TokenProcessingResult
            {
                NextToken = nextToken,
                ShouldValidateExpectations = false
            };
        }
    }
}