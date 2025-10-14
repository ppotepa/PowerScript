namespace Tokenez.Common.Localization
{
    /// <summary>
    ///     Centralized message keys for localization.
    ///     Organized by functional area for better discoverability and maintainability.
    ///     Keys correspond to entries in Resources/Messages.resx
    /// </summary>
    public static class MessageKeys
    {
        /// <summary>
        ///     Application-level messages (startup, version, shutdown)
        /// </summary>
        public static class Application
        {
            public const string Welcome = "App_Welcome";
            public const string Version = "App_Version";
            public const string Goodbye = "App_Goodbye";
        }

        /// <summary>
        ///     Interactive shell interface messages
        /// </summary>
        public static class Shell
        {
            public const string Banner = "Shell_Banner";
            public const string Welcome = "Shell_Welcome";
            public const string HelpHint = "Shell_Help_Hint";
            public const string CodeHint = "Shell_Code_Hint";

            /// <summary>
            ///     Command history related messages
            /// </summary>
            public static class History
            {
                public const string Title = "Shell_History_Title";
                public const string Empty = "Shell_History_Empty";
            }
        }

        /// <summary>
        ///     Script file execution messages
        /// </summary>
        public static class Script
        {
            public const string Executing = "Script_Executing";
            public const string Success = "Script_Success";
            public const string Error = "Script_Error";
        }

        /// <summary>
        ///     File and namespace linking messages
        /// </summary>
        public static class Link
        {
            public const string Namespace = "Link_Namespace";
            public const string File = "Link_File";
            public const string AlreadyLinked = "Link_Already_Linked";
            public const string Loading = "Link_Loading";
            public const string Error = "Link_Error";
            public const string LibraryRegistered = "Link_Library_Registered";
            public const string FileRegistered = "Link_File_Registered";
            public const string BestPractice = "Link_Best_Practice";
            public const string Suggestion = "Link_Suggestion";
        }

        /// <summary>
        ///     Token tree construction and processing messages
        /// </summary>
        public static class TokenTree
        {
            public const string Building = "TokenTree_Building";
            public const string Processing = "TokenTree_Processing";
            public const string TokensCreated = "TokenTree_Tokens_Created";
            public const string TokensLinked = "TokenTree_Tokens_Linked";
            public const string ScopeComplete = "TokenTree_Scope_Complete";
        }

        /// <summary>
        ///     Function compilation messages
        /// </summary>
        public static class Compile
        {
            public const string Compiling = "Compile_Compiling";
            public const string Success = "Compile_Success";
            public const string Error = "Compile_Error";
        }

        /// <summary>
        ///     Scope and variable management messages
        /// </summary>
        public static class Scope
        {
            public const string VariableRegistered = "Scope_Variable_Registered";
        }

        /// <summary>
        ///     Error messages
        /// </summary>
        public static class Errors
        {
            public const string Execution = "Error_Execution";
        }

        /// <summary>
        ///     Warning messages
        /// </summary>
        public static class Warnings
        {
            public const string StdLibLinkFailed = "Warning_StdLib_Link_Failed";
        }

        /// <summary>
        ///     Debug messages (only shown in DEBUG builds)
        /// </summary>
        public static class Debug
        {
            public const string LinkProcessor = "Debug_LinkProcessor";
            public const string VariableDeclaration = "Debug_VariableDeclaration";
        }

        /// <summary>
        ///     Standard library information messages
        /// </summary>
        public static class StandardLibrary
        {
            public const string Summary = "StdLib_Summary";
        }

        /// <summary>
        ///     Help system messages
        /// </summary>
        public static class Help
        {
            public const string Title = "Help_Title";
            public const string Commands = "Help_Commands";
            public const string Syntax = "Help_Syntax";
            public const string Examples = "Help_Examples";

            /// <summary>
            ///     About dialog messages
            /// </summary>
            public static class About
            {
                public const string Title = "Help_About_Title";
                public const string Content = "Help_About_Content";
            }
        }
    }
}