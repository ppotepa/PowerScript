using ppotepa.tokenez.Logging;
using ppotepa.tokenez.Tree.Exceptions;
using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Delimiters;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Keywords.Types;
using ppotepa.tokenez.Tree.Tokens.Operators;

namespace ppotepa.tokenez.Tree.Builders
{
    /// <summary>
    ///     Processes function parameter lists.
    ///     Handles the tokens between opening and closing parentheses in function declarations.
    ///     Validates parameter syntax: TYPE identifier [, TYPE identifier]*
    /// </summary>
    internal class ParameterProcessor
    {
        /// <summary>
        ///     Processes all parameters from opening parenthesis to closing parenthesis.
        ///     Returns the parameter list and the next token after the closing parenthesis.
        /// </summary>
        public (FunctionParametersToken Parameters, Token NextToken) ProcessParameters(Token startToken)
        {
            FunctionParametersToken parameters = new();
            var currentToken = startToken;
            var paramSafetyCounter = 0;

            // Process until we hit the closing parenthesis
            while (currentToken is not ParenthesisClosed)
            {
                paramSafetyCounter++;
                if (paramSafetyCounter > 100)
                {
                    LoggerService.Logger.Error(
                        "Exceeded 100 iterations in ProcessParameters. Possible endless loop in parameter list.");
                    break;
                }

                // Try to parse a type-identifier pair (e.g., "INT x")
                if (TryProcessTypeAndIdentifier(currentToken, parameters, out var nextToken))
                {
                    currentToken = nextToken;

                    // After a parameter, expect either comma (more params) or closing parenthesis
                    if (currentToken is CommaToken)
                    {
                        currentToken = currentToken.Next; // Skip comma and continue
                        continue;
                    }

                    if (currentToken is ParenthesisClosed) break; // End of parameter list

                    // Neither comma nor closing parenthesis - syntax error
                    ThrowUnexpectedTokenException(currentToken);
                }

                // Empty parameter list case
                if (currentToken is ParenthesisClosed) break;

                // Invalid token in parameter list
                ThrowUnexpectedTokenException(currentToken);
            }

            // Return collected parameters and token after closing parenthesis
            if (currentToken is ParenthesisClosed) return (parameters, currentToken.Next);

            ThrowUnexpectedTokenException(currentToken);
            return default;
        }

        /// <summary>
        ///     Attempts to parse a type-identifier pair (e.g., "INT x" or "INT CHAIN numbers").
        ///     Supports composite types like INT CHAIN, PREC CHAIN, CHAR CHAIN.
        ///     Returns true if successfully parsed, false if token is not a type.
        /// </summary>
        private bool TryProcessTypeAndIdentifier(Token token, FunctionParametersToken parameters, out Token nextToken)
        {
            LoggerService.Logger.Debug(
                $"TryProcessTypeAndIdentifier: token={token.GetType().Name} '{token.RawToken?.Text}'");

            // First token must be a type (INT, PREC, CHAR, STRING, etc.)
            if (token is not ITypeToken)
            {
                // If it's an identifier where we expect a type, it's an invalid type name
                if (token is IdentifierToken)
                    throw new UnexpectedTokenException(
                        token,
                        $"Invalid parameter type '{token.RawToken?.Text}'. Valid types are: INT, PREC, CHAR, STRING, CHAIN",
                        typeof(IntToken)
                    );

                nextToken = null!;
                return false;
            }

            var typeToken = token;
            var currentToken = token.Next;

            // Check if this is a composite type (e.g., INT CHAIN, PREC CHAIN)
            if (currentToken is ChainToken chainToken)
            {
                // Create a composite type representation (for now, we'll use the base type)
                // In the future, this could be enhanced to create a proper composite type declaration
                currentToken = chainToken.Next;
                LoggerService.Logger.Debug($"Found composite type: {typeToken.GetType().Name} CHAIN");
            }

            // Current token should now be the identifier (parameter name)
            if (currentToken is not IdentifierToken identifierToken)
                throw new UnexpectedTokenException(currentToken, typeof(IdentifierToken));

            // Add parameter to the list (using the base type token for now)
            parameters.Declarations.Add(new ParameterDeclaration(typeToken, identifierToken));
            nextToken = identifierToken.Next;
            return true;
        }

        /// <summary>
        ///     Throws a formatted exception for unexpected tokens in parameter lists.
        /// </summary>
        private static void ThrowUnexpectedTokenException(Token token)
        {
            throw new UnexpectedTokenException(
                token,
                "Unexpected token in parameters list",
                typeof(ITypeToken),
                typeof(CommaToken),
                typeof(ParenthesisClosed));
        }
    }
}