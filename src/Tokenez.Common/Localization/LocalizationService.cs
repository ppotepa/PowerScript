using Microsoft.Extensions.Localization;

namespace Tokenez.Common.Localization
{
    /// <summary>
    ///     Service for accessing localized strings using .NET's built-in localization.
    ///     Provides static access to IStringLocalizer for the entire application.
    /// </summary>
    public static class LocalizationService
    {
        private static IStringLocalizer? _localizer;

        /// <summary>
        ///     Initializes the localization service with a string localizer instance.
        /// </summary>
        public static void Initialize(IStringLocalizer localizer)
        {
            _localizer = localizer;
        }

        /// <summary>
        ///     Gets a localized string by key.
        /// </summary>
        public static string GetString(string key)
        {
            if (_localizer == null)
                // Fallback if not initialized - return the key
                return $"[{key}]";

            return _localizer[key];
        }

        /// <summary>
        ///     Gets a localized string by key with format arguments.
        /// </summary>
        public static string GetString(string key, params object[] args)
        {
            if (_localizer == null)
                // Fallback if not initialized
                return $"[{key}]";

            return _localizer[key, args];
        }
    }
}