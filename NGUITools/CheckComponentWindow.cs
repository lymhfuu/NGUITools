using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Text;
using System;
using Spine;

public class CheckComponentWindow : CheckWindowBase
{
    private static CheckComponentWindow window;
    private string scriptName = "";

    [MenuItem("NGUITools/检测C#脚本窗口", false, 8)]
    static void CheckFontWin()
    {
        //创建窗口
        window = (CheckComponentWindow)GetWindow(typeof(CheckComponentWindow), false, "检测C#脚本窗口");
        window.Show();
    }

    void OnGUI()
    {
        DrawCheckTargetGUI();

        GUILayout.Space(10);
        GUILayout.Label("输入要查询的脚本：", btTextStyle);
        scriptName = EditorGUILayout.TextField(scriptName, inputStyle, GUILayout.Width(350), GUILayout.Height(40));
        if (string.IsNullOrEmpty(scriptName))
        {
            DrawWarning("The keyword is empty，all path of component will be printed.");
        }

        DrawSearchGUI();
    }

    protected override bool Check(GameObject root, string assistPath, CheckType checkType)
    {
        bool flag = false;

        if (root != null)
        {
            Component[] list = root.GetComponentsInChildren<Component>(true);

            try
            {
                for (int i = 0; i < list.Length; i++)
                {
                    if (list[i] != null && CheckResult(list[i].GetType().ToString(), scriptName))
                    {
                        flag = true;
                        StringBuilder sb = new StringBuilder();
                        sb.Append(list[i].GetType().ToString());
                        sb.Append("   Path: ");
                        LogObjPath(sb.ToString(), list[i].gameObject, assistPath, checkType);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("发生异常: " + ex.Message);
            }

        }

        return flag;
    }
}
