using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Text;
using Spine;

public class CheckTextureWindow : CheckWindowBase
{
    private static CheckTextureWindow window;
    private string texName = "";
    private bool searchEmpty = false;
    [HideInInspector][SerializeField] UnityEngine.Object mTex;

    [MenuItem("NGUITools/检测UITexture窗口", false, 7)]
    static void CheckTexWin()
    {
        //创建窗口
        window = (CheckTextureWindow)GetWindow(typeof(CheckTextureWindow), false, "检测UITexture窗口");
        window.Show();
    }

    void OnGUI()
    {
        DrawCheckTargetGUI();

        searchEmpty = GUILayout.Toggle(searchEmpty, "检索未赋值Texture", toggleTextStyle);
        GUILayout.Space(10);

        if (!searchEmpty)
        {
            GUILayout.Label("输入Texture：", btTextStyle);
            var tex = mTex as Texture;
            GUILayout.BeginHorizontal();


            mTex = EditorGUILayout.ObjectField("", mTex, typeof(Texture), false, GUILayout.Width(80), GUILayout.Height(80)) as Texture;
            texName = EditorGUILayout.TextField(texName, inputStyle, GUILayout.Width(350), GUILayout.Height(40));

            GUILayout.EndHorizontal();
            if (mTex) texName = mTex.name;
        }

        DrawSearchGUI();
    }

    protected override bool Check(GameObject root, string assistPath, CheckType checkType)
    {
        bool flag = false;

        if (root != null)
        {
            UITexture[] texList = root.GetComponentsInChildren<UITexture>(true);

            for (int i = 0; i < texList.Length; i++)
            {
                if (!searchEmpty)
                {
                    try
                    {
                        if (texList[i] != null && texList[i].mainTexture != null && CheckResult(texList[i].mainTexture.name, texName))
                        {
                            flag = true;
                            //CheckDebug.LogError("UITexture：" + texList[i].mainTexture.name + "\nPath: " + GetObjPath(texList[i].gameObject));
                            StringBuilder sb = new StringBuilder();
                            sb.Append("UITexture：");
                            sb.Append(texList[i].mainTexture.name);
                            sb.Append("            Path: ");
                            LogObjPath(sb.ToString(), texList[i].gameObject, assistPath, checkType);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("发生异常: " + ex.Message);
                    }
                }
                else
                {
                    if (texList[i].mainTexture == null)
                    {
                        flag = true;
                        //CheckDebug.LogError("UITexture 未赋值：" + GetObjPath(texList[i].gameObject));
                        LogObjPath("UITexture 未赋值：", texList[i].gameObject, assistPath, checkType);
                    }
                }
            }
        }

        return flag;
    }
}
