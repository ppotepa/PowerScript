using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerScript.Parser.Lexer
{
    /// <summary>
    /// Enhanced context flags for fine-grained control over tokenization behavior
    /// </summary>
    [Flags]
    public enum ContextFlags
    {
        None = 0,
        AllowsCustomKeywords = 1 << 0,
        AllowsDotNetInterop = 1 << 1,
        AllowsPatternMatching = 1 << 2,
        IsExpressionContext = 1 << 3,
        IsTypeContext = 1 << 4,
        RequiresIdentifier = 1 << 5,
        IsStatementStart = 1 << 6,
        IsEphemeral = 1 << 7,
        AllowsVariableDeclaration = 1 << 8,
        IsInsideFunction = 1 << 9,
        IsInsideLoop = 1 << 10,
        AllowsTypeAnnotation = 1 << 11
    }

    /// <summary>
    /// Enhanced lexical context with flags and properties for sophisticated context awareness
    /// </summary>
    public abstract class EnhancedLexicalContext : LexicalContext
    {
        public ContextFlags Flags { get; protected set; }
        public Dictionary<string, object> Properties { get; } = new();
        public EnhancedLexicalContext? Parent { get; set; }

        protected EnhancedLexicalContext(ContextFlags flags = ContextFlags.None)
        {
            Flags = flags;
        }

        public override bool IsEphemeral => HasFlag(ContextFlags.IsEphemeral);

        public bool HasFlag(ContextFlags flag) => Flags.HasFlag(flag);

        public void SetFlag(ContextFlags flag, bool value)
        {
            if (value)
                Flags |= flag;
            else
                Flags &= ~flag;
        }

        public override bool AllowsCustomKeyword(string text)
        {
            // Multiple checks based on flags
            if (!HasFlag(ContextFlags.AllowsCustomKeywords))
                return false;

            if (HasFlag(ContextFlags.RequiresIdentifier))
                return false;

            if (HasFlag(ContextFlags.IsTypeContext))
                return false;

            // Check parent context if needed
            if (Parent != null && !Parent.AllowsCustomKeyword(text))
                return false;

            return true;
        }

        public T? GetProperty<T>(string key) where T : class
        {
            return Properties.TryGetValue(key, out var value) ? value as T : null;
        }

        public void SetProperty(string key, object value)
        {
            Properties[key] = value;
        }
    }

    /// <summary>
    /// Factory for creating contexts based on token patterns
    /// </summary>
    public class ContextFactory
    {
        private readonly Dictionary<string, Func<EnhancedLexicalContext>> _contextCreators = new();

        public ContextFactory()
        {
            RegisterDefaultCreators();
        }

        private void RegisterDefaultCreators()
        {
            // Member access contexts
            _contextCreators["->"] = () => new EnhancedMemberAccessContext();
            _contextCreators["."] = () => new EnhancedMemberAccessContext();
            _contextCreators["#"] = () => new EnhancedDotNetInteropContext();

            // Expression contexts
            _contextCreators["="] = () => new EnhancedExpressionContext();
            _contextCreators["("] = () => new EnhancedFunctionCallContext();
            _contextCreators["["] = () => new EnhancedArrayLiteralContext();

            // Block and statement contexts
            _contextCreators["{"] = () => new EnhancedBlockContext();
            _contextCreators["RETURN"] = () => new EnhancedReturnContext();
            _contextCreators["IF"] = () => new EnhancedConditionalContext();
            _contextCreators["WHILE"] = () => new EnhancedLoopContext();
            _contextCreators["CYCLE"] = () => new EnhancedCycleParameterContext(); // After CYCLE, expect parameter

            // Type contexts
            _contextCreators["INT"] = () => new EnhancedTypeAnnotationContext();
            _contextCreators["STRING"] = () => new EnhancedTypeAnnotationContext();
            _contextCreators["BOOL"] = () => new EnhancedTypeAnnotationContext();
            _contextCreators["VAR"] = () => new EnhancedTypeAnnotationContext();
            _contextCreators["FLEX"] = () => new EnhancedTypeAnnotationContext();
        }

        public EnhancedLexicalContext? CreateContext(string tokenText, EnhancedLexicalContext? current)
        {
            if (_contextCreators.TryGetValue(tokenText.ToUpperInvariant(), out var creator))
            {
                var newContext = creator();
                newContext.Parent = current;
                return newContext;
            }
            return null;
        }
    }

    // Specific context implementations

    public class EnhancedRootContext : EnhancedLexicalContext
    {
        public EnhancedRootContext() : base(
            ContextFlags.AllowsCustomKeywords |
            ContextFlags.AllowsPatternMatching |
            ContextFlags.AllowsDotNetInterop |
            ContextFlags.IsStatementStart)
        { }
    }

    public class EnhancedMemberAccessContext : EnhancedLexicalContext
    {
        public EnhancedMemberAccessContext() : base(
            ContextFlags.AllowsDotNetInterop |
            ContextFlags.RequiresIdentifier |
            ContextFlags.IsEphemeral)
        { }
    }

    public class EnhancedDotNetInteropContext : EnhancedLexicalContext
    {
        public EnhancedDotNetInteropContext() : base(
            ContextFlags.AllowsDotNetInterop |
            ContextFlags.RequiresIdentifier |
            ContextFlags.IsEphemeral)
        { }
    }

    public class EnhancedExpressionContext : EnhancedLexicalContext
    {
        public EnhancedExpressionContext() : base(
            ContextFlags.IsExpressionContext |
            ContextFlags.AllowsDotNetInterop)
        { }
    }

    public class EnhancedFunctionCallContext : EnhancedLexicalContext
    {
        public EnhancedFunctionCallContext() : base(
            ContextFlags.IsExpressionContext |
            ContextFlags.AllowsDotNetInterop)
        { }
    }

    public class EnhancedArrayLiteralContext : EnhancedLexicalContext
    {
        public EnhancedArrayLiteralContext() : base(
            ContextFlags.IsExpressionContext |
            ContextFlags.AllowsDotNetInterop)
        { }
    }

    public class EnhancedBlockContext : EnhancedLexicalContext
    {
        public EnhancedBlockContext() : base(
            ContextFlags.AllowsCustomKeywords |
            ContextFlags.AllowsPatternMatching |
            ContextFlags.AllowsDotNetInterop |
            ContextFlags.AllowsVariableDeclaration |
            ContextFlags.IsInsideFunction |
            ContextFlags.IsStatementStart)
        { }
    }

    public class EnhancedReturnContext : EnhancedLexicalContext
    {
        public EnhancedReturnContext() : base(
            ContextFlags.IsExpressionContext |
            ContextFlags.AllowsDotNetInterop)
        { }
    }

    public class EnhancedConditionalContext : EnhancedLexicalContext
    {
        public EnhancedConditionalContext() : base(
            ContextFlags.IsExpressionContext |
            ContextFlags.AllowsDotNetInterop)
        { }
    }

    public class EnhancedLoopContext : EnhancedLexicalContext
    {
        public EnhancedLoopContext() : base(
            ContextFlags.IsInsideLoop |
            ContextFlags.AllowsDotNetInterop |
            ContextFlags.IsExpressionContext)
        { }
    }

    public class EnhancedCycleParameterContext : EnhancedLexicalContext
    {
        public EnhancedCycleParameterContext() : base(
            ContextFlags.RequiresIdentifier |  // After CYCLE, expect identifier/number, NOT custom keywords
            ContextFlags.IsEphemeral)
        { }
    }

    public class EnhancedTypeAnnotationContext : EnhancedLexicalContext
    {
        public EnhancedTypeAnnotationContext() : base(
            ContextFlags.IsTypeContext |
            ContextFlags.RequiresIdentifier |
            ContextFlags.IsEphemeral)
        { }
    }

    /// <summary>
    /// State machine for managing context transitions
    /// </summary>
    public class ContextStateMachine
    {
        private readonly Dictionary<(Type context, string token), Type> _transitions = new();
        private readonly ContextFactory _factory = new();

        public ContextStateMachine()
        {
            RegisterDefaultTransitions();
        }

        private void RegisterDefaultTransitions()
        {
            // Function transitions
            RegisterTransition<EnhancedRootContext>("FUNCTION", typeof(EnhancedBlockContext));
            RegisterTransition<EnhancedBlockContext>("RETURN", typeof(EnhancedReturnContext));

            // Control flow transitions
            RegisterTransition<EnhancedBlockContext>("IF", typeof(EnhancedConditionalContext));
            RegisterTransition<EnhancedBlockContext>("WHILE", typeof(EnhancedLoopContext));
            RegisterTransition<EnhancedBlockContext>("CYCLE", typeof(EnhancedCycleParameterContext));

            // Type declaration transitions
            RegisterTransition<EnhancedBlockContext>("INT", typeof(EnhancedTypeAnnotationContext));
            RegisterTransition<EnhancedBlockContext>("STRING", typeof(EnhancedTypeAnnotationContext));
            RegisterTransition<EnhancedBlockContext>("BOOL", typeof(EnhancedTypeAnnotationContext));
            RegisterTransition<EnhancedBlockContext>("VAR", typeof(EnhancedTypeAnnotationContext));
            RegisterTransition<EnhancedBlockContext>("FLEX", typeof(EnhancedTypeAnnotationContext));
        }

        public void RegisterTransition<TContext>(string token, Type nextContextType)
            where TContext : EnhancedLexicalContext
        {
            _transitions[(typeof(TContext), token.ToUpperInvariant())] = nextContextType;
        }

        public EnhancedLexicalContext? GetNextContext(EnhancedLexicalContext current, string tokenText)
        {
            // Check for registered transitions first
            if (_transitions.TryGetValue((current.GetType(), tokenText.ToUpperInvariant()), out var nextType))
            {
                return (EnhancedLexicalContext?)Activator.CreateInstance(nextType);
            }

            // Fall back to factory for common patterns
            return _factory.CreateContext(tokenText, current);
        }
    }
}
