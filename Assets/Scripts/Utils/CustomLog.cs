using System.Runtime.CompilerServices;
using UnityEngine;

namespace Utils
{
    /// <summary>
    /// Global logging helper for consistent formatting and build-safe logging.
    /// </summary>
    public static class CustomLog
    {
        public static string Prefix { get; set; } = "[INXP]";
        public static bool IncludeCallerInfo { get; set; } = false;
        public static bool EnableInfoLogs { get; set; } = true;
        public static bool EnableVerboseLogs { get; set; } = false;

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void Info(
            string message,
            Object context = null,
            string category = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (!EnableInfoLogs) return;
            Debug.Log(Format(message, category, memberName, filePath, lineNumber), context);
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void Verbose(
            string message,
            Object context = null,
            string category = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (!EnableVerboseLogs) return;
            Debug.Log(Format(message, category, memberName, filePath, lineNumber), context);
        }

        public static void Warn(
            string message,
            Object context = null,
            string category = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            Debug.LogWarning(Format(message, category, memberName, filePath, lineNumber), context);
        }

        public static void Error(
            string message,
            Object context = null,
            string category = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            Debug.LogError(Format(message, category, memberName, filePath, lineNumber), context);
        }

        public static void Exception(
            System.Exception exception,
            Object context = null,
            string category = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            string header = Format(exception.Message, category, memberName, filePath, lineNumber);
            Debug.LogError(header, context);
            Debug.LogException(exception, context);
        }

        private static string Format(string message, string category, string memberName, string filePath, int lineNumber)
        {
            if (!IncludeCallerInfo)
            {
                return string.IsNullOrEmpty(category)
                    ? $"{Prefix} {message}"
                    : $"{Prefix}[{category}] {message}";
            }

            string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
            string caller = $"{fileName}.{memberName}:{lineNumber}";

            return string.IsNullOrEmpty(category)
                ? $"{Prefix} [{caller}] {message}"
                : $"{Prefix}[{category}] [{caller}] {message}";
        }
    }
}
