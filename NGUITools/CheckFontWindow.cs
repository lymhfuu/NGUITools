using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Text;
using Spine;
using System.Diagnostics.Eventing.Reader;

public class CheckFontWindow : CheckWindowBase
{
    private static CheckFontWindow window;
    private string fontName = "";
    private bool searchEmpty = false;

    [HideInInspector][SerializeField] UnityEngine.Object searchFontlas;

    [MenuItem("NGUITools/检测字体窗口", false, 8)]
    static void CheckFontWin()
    {
        //创建窗口
        window = (CheckFontWindow)GetWindow(typeof(CheckFontWindow), false, "检测字体窗口");
        window.Show();
    }

    void OnGUI()
    {
        DrawCheckTargetGUI();

        searchEmpty = GUILayout.Toggle(searchEmpty, "检索字库为空的Label", toggleTextStyle);
        GUILayout.Space(10);

        if (!searchEmpty)
        {
            GUILayout.Label("输入字库名：", btTextStyle);
            GUILayout.BeginHorizontal();
            searchFontlas = EditorGUILayout.ObjectField("", searchFontlas, typeof(UnityEngine.Object), false);
            GUILayout.EndHorizontal();

            fontName = EditorGUILayout.TextField(fontName, inputStyle, GUILayout.Width(350), GUILayout.Height(40));

            var font = searchFontlas as Font;
            var fontScript = searchFontlas as GameObject;
            if (searchFontlas != null)
            {
                if (font != null || (fontScript && fontScript.GetComponent(typeof(UIFont))))
                {
                    fontName = font ? font.name : fontScript.name;
                }
                else
                    searchFontlas = null;
            }
        }

        DrawSearchGUI();
    }

    protected override bool Check(GameObject root, string assistPath, CheckType checkType)
    {
        bool flag = false;

        if (root != null)
        {
            UILabel[] labelList = root.GetComponentsInChildren<UILabel>(true);

            for (int i = 0; i < labelList.Length; i++)
            {
                try
                {
                    if (!searchEmpty)
                    {
                        UIFont actualFont = labelList[i].bitmapFont as UIFont;
                        if (labelList[i] || actualFont == null) continue;

                        if (labelList[i].bitmapFont != null && CheckResult(actualFont.name, fontName))
                        {
                            flag = true;
                            //CheckDebug.LogError("NGUI字库：" + labelList[i].bitmapFont.name + "\nPath: " + GetObjPath(labelList[i].gameObject));
                            StringBuilder sb = new StringBuilder();
                            sb.Append("NGUI字库：");
                            sb.Append(actualFont.name);
                            sb.Append("\nPath: ");
                            LogObjPath(sb.ToString(), labelList[i].gameObject, assistPath, checkType);
                        }
                        else if (labelList[i].trueTypeFont != null && CheckResult(labelList[i].trueTypeFont.name, fontName))
                        {
                            flag = true;
                            //CheckDebug.LogError("Unity字库：" + labelList[i].trueTypeFont.name + "\nPath: " + GetObjPath(labelList[i].gameObject));
                            StringBuilder sb = new StringBuilder();
                            sb.Append("Unity字库：");
                            sb.Append(labelList[i].trueTypeFont.name);
                            sb.Append("\nPath: ");
                            LogObjPath(sb.ToString(), labelList[i].gameObject, assistPath, checkType);
                        }
                    }
                    else
                    {
                        if (labelList[i].bitmapFont == null && labelList[i].trueTypeFont == null)
                        {
                            flag = true;
                            //CheckDebug.LogError("Label字库为空：" + GetObjPath(labelList[i].gameObject));
                            LogObjPath("Label字库为空：", labelList[i].gameObject, assistPath, checkType);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("发生异常: " + ex.Message);
                }
            }
        }

        return flag;
    }
}
