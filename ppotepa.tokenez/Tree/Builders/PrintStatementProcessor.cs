using ppotepa.tokenez.Tree.Builders.Interfaces;
using ppotepa.tokenez.Tree.Expressions;
using ppotepa.tokenez.Tree.Statements;
using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Keywords;
using ppotepa.tokenez.Tree.Tokens.Values;

namespace ppotepa.tokenez.Tree.Builders
{
    /// <summary>
    /// Processes PRINT keyword tokens.
    /// Responsible for:
    /// - Parsing the expression to print (string literal, function call, etc.)
    /// - Creating PrintStatement objects
    /// - Registering print statements in the current scope
    /// </summary>
    internal class PrintStatementProcessor : ITokenProcessor
    {
        private readonly ExpectationValidator _validator;

        public PrintStatementProcessor(ExpectationValidator validator)
        {
            _validator = validator;
        }

        public bool CanProcess(Token token)
        {
            return token is PrintKeywordToken;
        }

        public TokenProcessingResult Process(Token token, ProcessingContext context)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[DEBUG] PrintStatementProcessor: Processing PRINT token '{token.RawToken?.Text}' in scope '{context.CurrentScope.ScopeName}'");
            Console.ResetColor();

            var printToken = token as PrintKeywordToken;
            var nextToken = printToken.Next;

            // Parse the expression to print
            Expression expression = null;

            if (nextToken is StringLiteralToken stringToken)
            {
                // Simple string literal
                expression = new StringLiteralExpression(stringToken);
                nextToken = stringToken.Next;
            }
            else
            {
                // TODO: Handle other expression types (function calls, variables, etc.)
                throw new NotImplementedException("PRINT currently only supports string literals");
            }

            // Create and register the print statement
            var printStatement = new PrintStatement(expression)
            {
                StartToken = printToken
            };

            context.CurrentScope.Statements.Add(printStatement);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[DEBUG] Registered PRINT statement in scope '{context.CurrentScope.ScopeName}'");
            Console.ResetColor();

            // Don't validate expectations since we already handled the token sequence
            return new TokenProcessingResult
            {
                NextToken = nextToken,
                ShouldValidateExpectations = false
            };
        }
    }
}
