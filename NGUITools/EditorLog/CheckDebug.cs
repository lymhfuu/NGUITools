using UnityEngine;

public class CheckDebug 
{
    public static void Log(string info)
    {
        Debug.Log(info);
    }

    public static void LogError(string info)
    {
        Debug.LogError(info);
    }

    public static void LogWarning(string info)
    {
        Debug.LogWarning(info);
    }
}