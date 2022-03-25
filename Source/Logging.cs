using System;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace WarpDrive
{
    /// <summary>
    /// Class Logging
    /// v3
    /// update namespace, prefix, put
    /// using static Logging;
    /// and use by LogDebug() etc
    /// </summary>
    public static class Logging
    {
        private const string PREFIX = "<color=green>[WarpDrive]</color> ";
        private const bool time = false;

        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogDebug(params object[] args)
        {
            Log(args);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogFormatDebug(string msg, params object[] args)
        {
            LogFormat(msg, args);
        }

        public static void Log(params object[] args)
        {
            Debug.Log(PREFIX + (time ? DateTime.Now.ToString("HH:mm:ss.f ") : "") +
                String.Join(", ", args)
                );
        }

        public static void LogFormat(string msg, params object[] args)
        {
            Debug.LogFormat(PREFIX + (time ? DateTime.Now.ToString("HH:mm:ss.f ") : "") +
                msg, args);
        }
    }
}
