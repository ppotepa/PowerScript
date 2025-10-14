#nullable enable
using Tokenez.Common.Logging;
using Tokenez.Parser.Processors.Base;
using Tokenez.Core.AST;
using Tokenez.Core.AST.Expressions;
using Tokenez.Core.AST.Statements;
using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Identifiers;
using Tokenez.Core.Syntax.Tokens.Keywords;
using Tokenez.Core.Syntax.Tokens.Keywords.Types;
using Tokenez.Core.Syntax.Tokens.Operators;
using Tokenez.Core.Syntax.Tokens.Values;

namespace Tokenez.Parser.Processors
{
    /// <summary>
    ///     Processes VAR keyword tokens for variable declarations.
    ///     Responsible for:
    ///     - Parsing variable name (required)
    ///     - Parsing optional type declaration (INT, etc.)
    ///     - Parsing equals sign and initial value expression
    ///     - Checking for duplicate declarations in current scope
    ///     - Registering variable declaration in scope
    /// </summary>
    public class VariableDeclarationProcessor : ITokenProcessor
    {

        public bool CanProcess(Token token)
        {
            return token is VarKeywordToken;
        }

        public TokenProcessingResult Process(Token token, ProcessingContext context)
        {
            LoggerService.Logger.Debug(
                $"VariableDeclarationProcessor: Processing VAR token in scope '{context.CurrentScope.ScopeName}'");

            var varToken = token as VarKeywordToken;
            var currentToken = varToken!.Next!;

            // Determine if we have explicit type or inferred type
            // Syntax: VAR x = 10         (inferred)
            //     OR: VAR INT x = 10     (explicit)

            Token? typeToken = null;
            Token identifierToken;

            // Check if next token is a type (INT, etc.)
            if (currentToken is IBaseTypeToken)
            {
                typeToken = currentToken;
                currentToken = currentToken.Next!;
            }

            // Next token must be an identifier (variable name)
            if (currentToken is not IdentifierToken)
                throw new InvalidOperationException(
                    $"Expected identifier after VAR{(typeToken != null ? " " + typeToken.RawToken?.Text : "")}, found {currentToken.GetType().Name}");

            identifierToken = currentToken;
            var variableName = identifierToken.RawToken?.Text ?? "";

            LoggerService.Logger.Debug($"Variable name: {variableName}");
            if (typeToken != null) LoggerService.Logger.Debug($"Variable type: {typeToken.RawToken?.Text}");

            // Check for duplicate declarations in current scope
            if (context.CurrentScope.Decarations.ContainsKey(variableName))
                throw new InvalidOperationException(
                    $"Variable '{variableName}' is already declared in scope '{context.CurrentScope.ScopeName}'");

            currentToken = identifierToken.Next!;

            // Expect equals sign
            if (currentToken is not EqualsToken)
                throw new InvalidOperationException(
                    $"Expected '=' after variable name '{variableName}', found {currentToken.GetType().Name}");

            currentToken = currentToken.Next!;

            // Parse the initial value expression
            Expression initialValue;

            if (currentToken is ValueToken valueToken)
            {
                initialValue = new LiteralExpression(valueToken);
                currentToken = valueToken.Next!;
            }
            else if (currentToken is StringLiteralToken stringToken)
            {
                initialValue = new StringLiteralExpression(stringToken);
                currentToken = stringToken.Next!;
            }
            else if (currentToken is IdentifierToken idToken)
            {
                // Could be a variable reference or function call
                initialValue = new IdentifierExpression(idToken);
                currentToken = idToken.Next!;
            }
            else
            {
                throw new InvalidOperationException(
                    $"Expected value, string, or identifier after '=' in variable declaration, found {currentToken.GetType().Name}");
            }

            // Create the variable declaration
            var declaration = typeToken != null
                ? new VariableDeclaration(typeToken, identifierToken)
                : new VariableDeclaration(identifierToken);
            declaration.InitialValue = initialValue;

            // Register the declaration in the scope
            context.CurrentScope.Decarations.Add(variableName, declaration);

            // Create and register the statement
            VariableDeclarationStatement statement = new(declaration, initialValue)
            {
                StartToken = varToken
            };

            context.CurrentScope.Statements.Add(statement);

            LoggerService.Logger.Success(
                $"✓ Registered variable '{variableName}' in scope '{context.CurrentScope.ScopeName}'");

            return TokenProcessingResult.Continue(currentToken);
        }
    }
}
