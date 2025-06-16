using System;
using MelonLoader;
using UnityEngine;

namespace BluePrinceModPreferencesManager;

internal static class Logger
{
    internal static void Log(string message, LogType logType)
    {
        string log = message?.ToString() ?? "";

        switch (logType)
        {
            case LogType.Assert:
            case LogType.Log:
                Melon<Melon>.Logger.Msg(log);
                break;
            case LogType.Warning:
                Melon<Melon>.Logger.Warning(log);
                break;
            case LogType.Error:
                Melon<Melon>.Logger.Error(log);
                break;
            case LogType.Exception:
                Melon<Melon>.Logger.BigError(log);
                break;
        }
    }
    internal static void LogMsg(string message) => Log(message, LogType.Log);
    internal static void LogError(string message) => Log(message, LogType.Error);
    internal static void LogWarning(string message) => Log(message, LogType.Warning);
    internal static void LogException(Exception ex) => Log($"{ex}", LogType.Exception);
    internal static void LogUniverseLib(string message, LogType logType) => Log(message, logType);
}