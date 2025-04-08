using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Text;

public class CheckAtlasWindow : CheckWindowBase
{
    private static CheckAtlasWindow window;
    private string atlasName = "";
    private string spriteName = "";
    [HideInInspector][SerializeField] UnityEngine.Object mAtlas;
    //替换成新图集
    private NGUIAtlas newAtlas = null;


    private string newSpriteName = "";


    [MenuItem("NGUITools/检测图集窗口", false, 6)]
    static void CheckAtlasWin()
    {
        //创建窗口
        window = (CheckAtlasWindow)GetWindow(typeof(CheckAtlasWindow), false, "检测图集窗口");
        window.Show();
    }

    void OnGUI()
    {
        DrawCheckTargetGUI();

        GUILayout.Space(20);
        GUILayout.Label("输入Atlas：", btTextStyle);
        GUILayout.BeginHorizontal();
        mAtlas = EditorGUILayout.ObjectField("", mAtlas, typeof(NGUIAtlas), false) as NGUIAtlas;
        GUILayout.EndHorizontal();
        atlasName = EditorGUILayout.TextField(atlasName, inputStyle, GUILayout.Width(350), GUILayout.Height(40));
        GUILayout.Label("输入Sprite：", btTextStyle);
        spriteName = EditorGUILayout.TextField(spriteName, inputStyle, GUILayout.Width(350), GUILayout.Height(40));

        if (mAtlas)
        {
            atlasName = mAtlas.name;
        }
        GUILayout.Space(20);

        DrawSearchGUI();

        DrawExchangeGUI(DrawExchangeSprite);
    }

    void OnDragAtlas(UnityEngine.Object obj)
    {
        // Legacy atlas support
        if (obj != null && obj is GameObject) obj = (obj as GameObject).GetComponent<UIAtlas>();

         mAtlas = obj;
    }

    private void DrawExchangeSprite()
    {
        GUILayout.Label("新Atlas：", btTextStyle);
        GUILayout.BeginHorizontal();
        if (NGUIEditorTools.DrawPrefixButton("Atlas"))
            ComponentSelector.Show<NGUIAtlas>(OnSelectAtlas);
        newAtlas = EditorGUILayout.ObjectField(newAtlas, typeof(NGUIAtlas), false, GUILayout.Width(280)) as NGUIAtlas;
        GUILayout.EndHorizontal();

        if (newAtlas == null)
        {
            DrawWarning("Can't exchange if new atlas is null.");
        }
        GUILayout.Label("输入新Sprite：", btTextStyle);
        newSpriteName = EditorGUILayout.TextField(newSpriteName, inputStyle, GUILayout.Width(350), GUILayout.Height(40));

        if (newAtlas != null && string.IsNullOrEmpty(newSpriteName))
        {
            DrawWarning("Will keep original sprite if new sprite name is null.");
        }
    }

    void OnSelectAtlas(UnityEngine.Object obj)
    {
        newAtlas = obj as NGUIAtlas;
        newAtlas = EditorGUILayout.ObjectField(newAtlas, typeof(NGUIAtlas), false, GUILayout.Width(280)) as NGUIAtlas;
    }

    protected override bool Check(GameObject root, string assistPath, CheckType checkType)
    {
        bool flag = false;

        if (root != null)
        {
            UISprite[] spriteList = root.GetComponentsInChildren<UISprite>(true);

            for (int i = 0; i < spriteList.Length; i++)
            {
                NGUIAtlas actualAtlas = spriteList[i].atlas as NGUIAtlas;
                if (actualAtlas == null)
                {
                    continue;
                }

                if (actualAtlas != null && spriteList[i].atlas != null && CheckResult(actualAtlas.name, atlasName) && CheckResult(spriteList[i].spriteName,spriteName))
                {
                    flag = true; 
                    //string str1 = "Atlas：" + spriteList[i].atlas.name + "                Sprite：" + spriteList[i].spriteName + "                   Path：";
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Atlas：");
                    sb.Append(actualAtlas.name);
                    sb.Append("                Sprite：");
                    sb.Append(spriteList[i].spriteName);
                    sb.Append("                   Path：");
                    LogObjPath(sb.ToString(), spriteList[i].gameObject, assistPath, checkType);
                }
            }
        }

        return flag;
    }

    protected override bool Exchange(GameObject root, string assistPath, CheckType checkType)
    {
        if(newAtlas == null)
        {
            Debug.LogError("New atlas is null.");
            return false;
        }

        if (checkType == CheckType.Folder)
        {
            GameObject ori_go = PrefabUtility.InstantiatePrefab(root) as GameObject;
            UISprite[] spriteList = ori_go.GetComponentsInChildren<UISprite>(true);

            //更新prefab
            if (ExchangeSprite(spriteList, false))
            {
                UnityEngine.Object newPrefab = PrefabUtility.CreateEmptyPrefab(assistPath);
                var newGo = PrefabUtility.ReplacePrefab(ori_go, newPrefab, ReplacePrefabOptions.ConnectToPrefab);
                LogObjPath("已更新预设：", newGo, assistPath, checkType);

                GameObject.DestroyImmediate(ori_go);
                return true;
            }
            GameObject.DestroyImmediate(ori_go);
        }else if (checkType == CheckType.GameObject)
        {
            UISprite[] spriteList = root.GetComponentsInChildren<UISprite>(true);
            if (ExchangeSprite(spriteList, true))
            {
                return true;
            }
        }
        
        return false;
    }

    /// <summary>
    /// 替换图集和精灵
    /// </summary>
    /// <param name="spriteList"></param>
    /// <returns></returns>
    private bool ExchangeSprite(UISprite[] spriteList, bool needLogDetail)
    {
        bool flag = false;
        for (int i = 0; i < spriteList.Length; i++)
        {
            UIAtlas actualAtlas = spriteList[i].atlas as UIAtlas;
            if (spriteList[i].atlas != null && CheckResult(actualAtlas.name, atlasName) && CheckResult(spriteList[i].spriteName, spriteName))
            {
                spriteList[i].atlas = newAtlas;
                if (!string.IsNullOrEmpty(newSpriteName))
                {
                    UISpriteData uid = newAtlas.GetSprite(newSpriteName);
                    if (uid == null)
                    {
                        Debug.LogError("Atlas（" + newAtlas.name + "）中不存在Sprite:" + newSpriteName);               
                        return false;
                    }
                    else
                    {
                        spriteList[i].spriteName = newSpriteName;
                    }
                }
                if (needLogDetail)
                {
                    LogObjPath("已更新节点（如有需要，请手动apply）：", spriteList[i].gameObject, "", CheckType.GameObject);
                }
                flag = true;
            }
        }
        return flag;
    }

}
