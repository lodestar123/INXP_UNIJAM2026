using System.Runtime.CompilerServices;
using UnityEngine;

namespace Utils
{
    /// <summary>
    /// 빌드 시 Garbage가 남지 않는 커스텀 로그 유틸리티 클래스입니다. 로그 메시지에 카테고리와 호출자 정보를 포함할 수 있습니다.
    /// </summary>
    public static class CustomLog
    {
        public static string Prefix { get; set; } = "[INXP]";
        public static bool IncludeCallerInfo { get; set; } = false;
        public static bool EnableInfoLogs { get; set; } = true;
        public static bool EnableVerboseLogs { get; set; } = false;

        /// <summary>
        /// 정보성 로그를 출력합니다. UNITY_EDITOR 또는 DEVELOPMENT_BUILD 빌드에서만 활성화됩니다.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="context"></param>
        /// <param name="category"></param>
        /// <param name="memberName"></param>
        /// <param name="filePath"></param>
        /// <param name="lineNumber"></param>
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


        /// <summary>
        /// 자세한 로그를 출력합니다. UNITY_EDITOR 또는 DEVELOPMENT_BUILD 빌드에서만 활성화됩니다.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="context"></param>
        /// <param name="category"></param>
        /// <param name="memberName"></param>
        /// <param name="filePath"></param>
        /// <param name="lineNumber"></param>
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

        /// <summary>
        /// 경고 로그를 출력합니다. UNITY_EDITOR 또는 DEVELOPMENT_BUILD 빌드에서만 활성화됩니다.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="context"></param>
        /// <param name="category"></param>
        /// <param name="memberName"></param>
        /// <param name="filePath"></param>
        /// <param name="lineNumber"></param>
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

        /// <summary>
        /// 오류 로그를 출력합니다. UNITY_EDITOR 또는 DEVELOPMENT_BUILD 빌드에서만 활성화됩니다.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="context"></param>
        /// <param name="category"></param>
        /// <param name="memberName"></param>
        /// <param name="filePath"></param>
        /// <param name="lineNumber"></param>
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

        /// <summary>
        /// 예외 로그를 출력합니다. UNITY_EDITOR 또는 DEVELOPMENT_BUILD 빌드에서만 활성화됩니다.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="context"></param>
        /// <param name="category"></param>
        /// <param name="memberName"></param>
        /// <param name="filePath"></param>
        /// <param name="lineNumber"></param>
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

        /// <summary>
        /// 로그 메시지를 포맷팅합니다. IncludeCallerInfo가 true인 경우, 호출자 정보(파일명, 멤버명, 라인 번호)를 로그 메시지에 포함합니다.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="category"></param>
        /// <param name="memberName"></param>
        /// <param name="filePath"></param>
        /// <param name="lineNumber"></param>
        /// <returns></returns>
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
