using UnityEngine;
using UnityEditor;
using System;

public class OperatingHint : ScriptableObject
{
    public OperatingHint(string title, string msg, Action sureCall, string ok = "确定", string cancel = "取消")
    {
        if ( EditorUtility.DisplayDialog(title, msg, ok,cancel) && sureCall != null)
        {
            sureCall();
        }
    }
}
