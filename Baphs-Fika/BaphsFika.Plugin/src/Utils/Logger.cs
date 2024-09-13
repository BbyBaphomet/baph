  using System;
  using System.IO;
  using UnityEngine;

  namespace BaphsFika.Plugin.Utils
  {
      public static class Logger
      {
          private static string LogFilePath { get; set; }
          private static LogLevel MinLogLevel { get; set; }

          public enum LogLevel
          {
              Debug,
              Info,
              Warning,
              Error,
              Fatal
          }

          static Logger()
          {
              LogFilePath = Path.Combine(Application.dataPath, "BaphsFika_Log.txt");
              MinLogLevel = LogLevel.Debug;
          }

          public static void SetLogLevel(LogLevel level)
          {
              MinLogLevel = level;
          }

          public static void Debug(string message) => Log(LogLevel.Debug, message);
          public static void Info(string message) => Log(LogLevel.Info, message);
          public static void Warning(string message) => Log(LogLevel.Warning, message);
          public static void Error(string message) => Log(LogLevel.Error, message);
          public static void Fatal(string message) => Log(LogLevel.Fatal, message);

          private static void Log(LogLevel level, string message)
          {
              if (level < MinLogLevel) return;

              string formattedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";

              Console.WriteLine(formattedMessage);

              switch (level)
              {
                  case LogLevel.Debug:
                  case LogLevel.Info:
                      UnityEngine.Debug.Log(formattedMessage);
                      break;
                  case LogLevel.Warning:
                      UnityEngine.Debug.LogWarning(formattedMessage);
                      break;
                  case LogLevel.Error:
                  case LogLevel.Fatal:
                      UnityEngine.Debug.LogError(formattedMessage);
                      break;
              }

              try
              {
                  File.AppendAllText(LogFilePath, formattedMessage + Environment.NewLine);
              }
              catch (Exception ex)
              {
                  UnityEngine.Debug.LogError($"Failed to write to log file: {ex.Message}");
              }
          }
      }
  }
