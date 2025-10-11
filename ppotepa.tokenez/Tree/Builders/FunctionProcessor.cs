using ppotepa.tokenez.Tree;
using ppotepa.tokenez.Tree.Builders.Interfaces;
using ppotepa.tokenez.Tree.Exceptions;
using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Delimiters;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Keywords;

namespace ppotepa.tokenez.Tree.Builders
{
    /// <summary>
    /// Processes FUNCTION keyword tokens.
    /// Responsible for:
    /// - Extracting function name
    /// - Processing parameter list
    /// - Creating function scope
    /// - Registering function declaration in parent scope
    /// </summary>
    internal class FunctionProcessor : ITokenProcessor
    {
        private readonly ParameterProcessor _parameterProcessor;
        private readonly ExpectationValidator _validator;

        public FunctionProcessor(ParameterProcessor parameterProcessor, ExpectationValidator validator)
        {
            _parameterProcessor = parameterProcessor;
            _validator = validator;
        }

        public bool CanProcess(Token token)
        {
            return token is FunctionToken;
        }

        public TokenProcessingResult Process(Token token, ProcessingContext context)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[DEBUG] FunctionProcessor: Processing FUNCTION token '{token.RawToken?.Text}' at depth {context.Depth}");
            Console.ResetColor();
            var functionToken = token as FunctionToken;
            BuilderLogger.LogFunctionFound(context.Depth);

            // Create a new scope for this function
            var functionScope = new Scope
            {
                OuterScope = context.CurrentScope,
                Type = ScopeType.Function  // Mark as function scope to enforce RETURN requirement
            };
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[DEBUG] Created new function scope: {functionScope.ScopeName} (Type={functionScope.Type})");
            Console.ResetColor();

            // Function must be followed by an identifier (function name)
            if (functionToken.Next is not IdentifierToken functionNameToken)
            {
                throw new UnexpectedTokenException(functionToken.Next, typeof(IdentifierToken));
            }

            var functionName = functionNameToken.RawToken.Text;
            BuilderLogger.LogFunctionName(functionName, context.Depth);

            // Register function in parent scope's declaration table
            var declaration = new FunctionDeclaration(functionNameToken);
            context.CurrentScope.Decarations.Add(functionName, declaration);
            functionScope.ScopeName = functionName;

            // Link the scope to the declaration so we can visualize it later
            declaration.Scope = functionScope;
            functionScope.FunctionDeclaration = declaration;

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[DEBUG] Registered function '{functionName}' in scope '{context.CurrentScope.ScopeName}'");
            Console.ResetColor();

            Token currentToken = functionNameToken;

            // Expect opening parenthesis for parameter list
            if (currentToken.Next is not ParenthesisOpen)
            {
                throw new UnexpectedTokenException(currentToken.Next, typeof(ParenthesisOpen));
            }

            currentToken = currentToken.Next;
            BuilderLogger.LogParametersStarting(context.Depth);

            // Delegate parameter processing to specialized processor
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[DEBUG] Delegating parameter processing for function '{functionName}'");
            Console.ResetColor();
            var (parametersToken, nextToken) = _parameterProcessor.ProcessParameters(currentToken.Next);

            // Store the parameters in the function declaration
            declaration.Parameters = parametersToken.Declarations;

            // Check if there's an optional return type specified with brackets
            if (nextToken is Tokens.Delimiters.BracketOpen)
            {
                BuilderLogger.LogFunctionName($"Processing return type for function '{functionName}'", context.Depth);
                
                // Expect a type token after the opening bracket
                if (nextToken.Next is not Tokens.Keywords.Types.ITypeToken returnTypeInterface)
                {
                    throw new UnexpectedTokenException(nextToken.Next, typeof(Tokens.Keywords.Types.ITypeToken));
                }

                // Cast to Token to access Token properties
                var returnTypeToken = (Token)returnTypeInterface;
                
                // Store the return type in the function declaration
                declaration.ReturnType = returnTypeToken;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"[DEBUG] Function '{functionName}' has return type: {returnTypeToken.RawToken?.Text}");
                Console.ResetColor();

                // Expect closing bracket after the return type
                if (returnTypeToken.Next is not Tokens.Delimiters.BracketClosed)
                {
                    throw new UnexpectedTokenException(returnTypeToken.Next, typeof(Tokens.Delimiters.BracketClosed));
                }

                // Move to the token after the closing bracket
                nextToken = returnTypeToken.Next.Next;
            }

            // After parameters (and optional return type), we expect a scope start token ('{')
            if (nextToken is not Tokens.Scoping.ScopeStartToken)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] Expected ScopeStartToken after parameters for function '{functionName}', but got {nextToken?.GetType().Name} '{nextToken?.RawToken?.Text}'");
                Console.ResetColor();
                throw new UnexpectedTokenException(nextToken, typeof(Tokens.Scoping.ScopeStartToken));
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[DEBUG] Passing control to ScopeProcessor for function '{functionName}'");
            Console.ResetColor();
            // Return control to continue processing the ScopeStartToken without modifying the parent context
            // The ScopeProcessor will handle the function scope internally
            return TokenProcessingResult.Continue(nextToken);
        }
    }
}
