using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Object = UnityEngine.Object;
#if ENABLE_WINMD_SUPPORT
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
#endif

public static class DebugLog
{
    private static string logPath;

    static DebugLog() 
    {
        logPath = Path.Combine(Application.persistentDataPath, "log.txt");
        // if the file exists, prune on startup
        if (File.Exists(logPath))
        {
            PruneLogFile();
        }
    }

    private static void PruneLogFile()
    {
        DateTime oneMonthAgo = DateTime.Now.AddMonths(-1);

        if(File.Exists(logPath))
        {
            string[] lines = File.ReadAllLines(logPath);
            List<string> newLines = new List<string>();

            foreach (string line in lines)
            {
                string[] parts = line.Split(new[] { ':' }, 2);
                if (DateTime.TryParse(parts[0], out DateTime entryDate))
                {
                    if (entryDate > oneMonthAgo)
                    {
                        newLines.Add(line);
                    }
                }
            }
            File.WriteAllLines(logPath, newLines);
            
        }
    }

    public static void Log(object message)
    {
        UnityEngine.Debug.Log(message);

        string logPath = Path.Combine(Application.persistentDataPath, "log.txt");
        using (StreamWriter writer = File.AppendText(logPath))
        {
            writer.WriteLine(DateTime.Now.ToShortDateString() + " " +DateTime.Now.ToShortTimeString() + ": " +  message);
        }
    }
    public static void Log(string message, Object context)
    {
        UnityEngine.Debug.Log(message,context);

        string logPath = Path.Combine(Application.persistentDataPath, "log.txt");
        using (StreamWriter writer = File.AppendText(logPath))
        {
            writer.WriteLine(DateTime.Now.ToShortDateString() + " " +DateTime.Now.ToShortTimeString() + ": " +  message);
        }
    }
    public static void LogFormat(string message, object jobState)
    {
        UnityEngine.Debug.LogFormat(message,jobState);

        string logPath = Path.Combine(Application.persistentDataPath, "log.txt");
        using (StreamWriter writer = File.AppendText(logPath))
        {
            writer.WriteLine(DateTime.Now.ToShortDateString() + " " +DateTime.Now.ToShortTimeString() + ": " +  message);
        }
    }

    public static void LogWarning(string message)
    {
        UnityEngine.Debug.LogWarning(message);
        string logPath = Path.Combine(Application.persistentDataPath, "log.txt");
        using (StreamWriter writer = File.AppendText(logPath))
        {
            writer.WriteLine(DateTime.Now.ToShortDateString() + " " +DateTime.Now.ToShortTimeString() + ": " +  message);
        }
    }

    public static void LogError(string message)
    {
        UnityEngine.Debug.LogError(message);
        string logPath = Path.Combine(Application.persistentDataPath, "log.txt");
        using (StreamWriter writer = File.AppendText(logPath))
        {
            writer.WriteLine(DateTime.Now.ToShortDateString() + " " +DateTime.Now.ToShortTimeString() + ": " +  message);
        }
    }
    public static void LogError(string message, GameObject gameObject)
    {
        UnityEngine.Debug.LogError(message,gameObject);
        string logPath = Path.Combine(Application.persistentDataPath, "log.txt");
        using (StreamWriter writer = File.AppendText(logPath))
        {
            writer.WriteLine(DateTime.Now.ToShortDateString() + " " +DateTime.Now.ToShortTimeString() + ": " +  message);
        }
    }
    public static void LogError(string message, Object context)
    {
        UnityEngine.Debug.LogError(message,context);
        string logPath = Path.Combine(Application.persistentDataPath, "log.txt");
        using (StreamWriter writer = File.AppendText(logPath))
        {
            writer.WriteLine(DateTime.Now.ToShortDateString() + " " +DateTime.Now.ToShortTimeString() + ": " +  message);
        }
    }
    public static void AssertFormat(bool condition, string format, params object[] args)
    {
        UnityEngine.Debug.AssertFormat(condition,format,args);
        // string logPath = Path.Combine(Application.persistentDataPath, "log.txt");
        // using (StreamWriter writer = File.AppendText(logPath))
        // {
        //     writer.WriteLine(message);
        // }
    }

    public static void LogErrorFormat(string format, params object[] args)
    {
        UnityEngine.Debug.LogErrorFormat(format,args);
    }

    public static void Assert(bool condition, string format)
    {
        UnityEngine.Debug.Assert(condition,format);
    }
    public static void Assert(bool condition)
    {
        UnityEngine.Debug.Assert(condition);
    }
    public static void LogException(Exception exception)
    {
        UnityEngine.Debug.LogException(exception);
        string logPath = Path.Combine(Application.persistentDataPath, "log.txt");
        using (StreamWriter writer = File.AppendText(logPath))
        {
            writer.WriteLine(DateTime.Now.ToShortDateString() + " " +DateTime.Now.ToShortTimeString() + ": " +  exception.Message);
        }
    }


    public static void LogWarning(string message, GameObject gameObject)
    {
        UnityEngine.Debug.LogWarning(message,gameObject);
        string logPath = Path.Combine(Application.persistentDataPath, "log.txt");
        using (StreamWriter writer = File.AppendText(logPath))
        {
            writer.WriteLine(message);
        }
    }

    public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration)
    {
       UnityEngine.Debug.DrawLine(start,end,color,duration);
    }

    public static void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        UnityEngine.Debug.DrawLine(start,end,color);
    }


    public static void LogError(object message)
    {
        UnityEngine.Debug.LogError(message);
    }
}