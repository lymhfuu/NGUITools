using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Text;
using Spine;


public class CheckWindowBase : EditorWindow
{
    public enum CheckType
    {
        Folder = 1,
        GameObject = 2,
    }

    //查找后执行的委托（是打印还是替换）
    public delegate bool CheckDel(GameObject root, string assistPath, CheckType checkType);
    //是查找文件夹还是GameObject的区分
    public delegate void SearchDel(CheckDel del);

    protected static GUIStyle btStyle;
    protected static GUIStyle btTextStyle;
    protected static GUIStyle toggleTextStyle;
    protected static GUIStyle inputStyle;

    private bool selectFolder = true;//是否查找文件夹
    private SearchDel searchAction; //查找时的回调（查找文件夹或者GameObject的方法）
    private SearchDel exchangeAction;//替换回调
    private bool needClearConsoleBeforeSearch = true;//查找前是否清空控件台
    private bool needClearConsoleBeforeExchange = true;//替换前是否清空控件台
    private bool ignoreCase = false; //查找时是否忽略大小写
    private bool exchange = false; //是否替换

    protected virtual void ResetStyle()
    {
        if (btStyle == null)
        {  
            btStyle = new GUIStyle("ButtonLeft");
            btStyle.fontSize = 20;
        }
           
        if ( btTextStyle == null)
        {
            btTextStyle = new GUIStyle("HeaderLabel");
            btTextStyle.fontSize = 20;
        }

        if (toggleTextStyle == null)
        {
            toggleTextStyle = new GUIStyle(GUI.skin.toggle)
            {
                fontSize = 15, // 设置字体大小
                fontStyle = FontStyle.Normal, // 设置字体加粗
                normal = { textColor = Color.gray }, // 未选中时文本颜色
            };
        }
        
        if (inputStyle == null)
        {
            inputStyle = new GUIStyle(EditorStyles.textField);
            inputStyle.fontSize = 25;
            inputStyle.normal.textColor = Color.gray;
            inputStyle.focused.textColor = Color.white;
            inputStyle.border = new RectOffset(4, 4, 4, 4);
        } 
    }


    /// <summary>
    /// 检测文件夹或者GameObject的选择
    /// </summary>
    protected virtual void DrawCheckTargetGUI()
    {
        ResetStyle();
        GUILayout.BeginHorizontal();
        selectFolder = GUILayout.Toggle(selectFolder, "检索文件夹", toggleTextStyle);
        bool selectGameObject = GUILayout.Toggle(!selectFolder, "检索GameObject", toggleTextStyle);
        selectFolder = !selectGameObject;
        GUILayout.EndHorizontal();

  
        ignoreCase = GUILayout.Toggle(ignoreCase, "检索时忽略字母大小写", toggleTextStyle);

        if (selectFolder)
            searchAction = CheckFolder;// 查找文件夹
        else
            searchAction = CheckObj; //查找GameObject
    }

    /// <summary>
    /// 查找按钮
    /// </summary>
    protected virtual void DrawSearchGUI()
    {
        GUILayout.Space(20);

        if (selectFolder && !CheckSelectFolder())
        {
            if (GUILayout.Button("请选择要检测的文件夹", btStyle, GUILayout.Width(350), GUILayout.Height(40)))
            {
                Debug.LogError("未指定要检测的文件夹");
            }
        }
        else if (!selectFolder && !CheckSelectObj())
        {
            if (GUILayout.Button("请选择要检测的GameObject", btStyle, GUILayout.Width(350), GUILayout.Height(40)))
            {
                Debug.LogError("未指定要检测的GameObject");
            }
        }
        else
        {
            needClearConsoleBeforeSearch = GUILayout.Toggle(needClearConsoleBeforeSearch, "查找前清空Log信息", toggleTextStyle);
            if (GUILayout.Button("查找", btStyle, GUILayout.Width(350), GUILayout.Height(40)))
            {
                if (needClearConsoleBeforeSearch)
                {
                    ClearConsole();
                }
                if (searchAction != null) searchAction(Check);// 单纯的查找（查找后打印信息）
            }
        }
    }

    /// <summary>
    /// 替换资源
    /// </summary>
    protected virtual void DrawExchangeGUI(Action drawAction)
    {
        GUILayout.Space(20);
        if ((selectFolder && CheckSelectFolder()))
        {
            exchangeAction = CheckFolder;
        }
        else if (!selectFolder && CheckSelectObj())
        {
            exchangeAction = CheckObj;
            
        }
        exchange = GUILayout.Toggle(exchange, "替换", toggleTextStyle);
        
        if(exchange && drawAction != null)
        {
            drawAction();
        }

        if(exchange)
        {
            needClearConsoleBeforeExchange = GUILayout.Toggle(needClearConsoleBeforeExchange, "替换前清空Log信息", toggleTextStyle);
            if (GUILayout.Button("替换", btStyle, GUILayout.Width(350), GUILayout.Height(40)))
            {
                StringBuilder sb = new StringBuilder();
                if(selectFolder)
                {
                    sb.Append("你确定对以下路径进行资源替换吗？\n");
                    for (int i = 0;i < prefabPath.Count; i ++)
                    {
                        sb.Append(prefabPath[i]);
                        sb.Append("\n");
                    }
                }else
                {
                    sb.Append("你确定对以下对象进行资源替换吗？\n");
                    for (int i = 0; i < objs.Length; i++)
                    {
                        sb.Append(GetObjPath(objs[i]));
                        sb.Append("\n");
                    }
                }

                OperatingHint help = new OperatingHint("替换", sb.ToString(), ComfirmExchange);
            }
        }
    }

    private void ComfirmExchange()
    {
        if (needClearConsoleBeforeExchange)
        {
            ClearConsole();
        }
        if (exchangeAction != null)
        {
            exchangeAction(Exchange);
        }
    }

    /// <summary>
    /// 检测对象，由子类定义
    /// </summary>
    /// <param name="root"></param>
    /// <returns></returns>
    protected virtual bool Check(GameObject root, string assistPath, CheckType checkType)
    {
        return false;
    }

    /// <summary>
    /// 替换
    /// </summary>
    protected virtual bool Exchange(GameObject root, string assistPath, CheckType checkType)
    {
        return false;
    }


    //打印节点信息
    protected virtual string GetObjPath(GameObject go)
    {
        if (go != null)
        {
            bool loop = true;
            string str = go.name;
            Transform temp = go.transform;
            while (loop)
            {
                Transform parent = temp.parent;
                if (parent != null)
                {
                    str = parent.name + "/" + str;
                    temp = parent;
                }
                else
                {
                    //str = temp.name + ":\n" + str;
                    loop = false;
                }
            }
            return str;
        }
        return "";
    }

    protected virtual void DrawWarning(string log)
    {
        EditorGUILayout.HelpBox(log, MessageType.Warning, true);
    }

    /// <summary>
    /// 打印路径信息
    /// </summary>
    /// <param name="str1">打印前缀信息</param>
    /// <param name="go">检测到的对象</param>
    /// <param name="assistPath">资源路径，或者GameObject的节点信息</param>
    /// <param name="checkType">检测的类型：文件夹/GameObject </param>
    protected virtual void LogObjPath(string str1, GameObject go, string assistPath, CheckType checkType)
    {
        string objPath = GetObjPath(go);
        if (checkType == CheckType.GameObject)
        {
            assistPath = objPath;
        }
        // 双击log定位的类型和路径
        //string pingPath = "|" + checkType + "|" + assistPath + "|";
        //CheckDebug.LogError(str1 + objPath + "\n\n" + pingPath);
        StringBuilder sb = new StringBuilder();
        sb.Append(str1);
        sb.Append(objPath);
        sb.Append("\n\n");
        sb.Append("|");
        sb.Append(checkType);
        sb.Append("|");
        sb.Append(assistPath);
        sb.Append("|");
        CheckDebug.LogError(sb.ToString());
    }

    /// <summary>
    /// 搜索关键字匹配
    /// </summary>
    /// <param name="checkTarget"></param>
    /// <param name="pattern"></param>
    /// <returns></returns>
    protected bool CheckResult(string checkTarget, string pattern)
    {
        if (string.IsNullOrEmpty(checkTarget))
        {
            Debug.LogError("待测对象为空。");
            return false;
        }
        if(ignoreCase)
        {
            checkTarget = checkTarget.ToLower();
            pattern = pattern.ToLower();
        }
        return checkTarget.Contains(pattern);
    }



    private static List<string> prefabPath = new List<string>();
    /// <summary>
    /// 是否有选中待检测的文件夹
    /// </summary>
    /// <returns></returns>
    private static bool CheckSelectFolder()
    {
        string[] guids = Selection.assetGUIDs;
        prefabPath.Clear();
        for (int i = 0; i < guids.Length; i++)
        {
            string p = AssetDatabase.GUIDToAssetPath(guids[i]);
            prefabPath.Add(p);
        }
        return guids.Length > 0;
    }

    /// <summary>
    /// 检测文件夹
    /// </summary>
    void CheckFolder(CheckDel callback)
    {
        string[] prefabArr = AssetDatabase.FindAssets("t:Prefab", prefabPath.ToArray());
        if (prefabArr == null || prefabArr.Length <= 0)
        {
            Debug.LogError("没有找到可检测的预制体!");
            return;
        }

        bool flag = false;
        for (int i = 0; i < prefabArr.Length; ++i)
        {
            string pathName = AssetDatabase.GUIDToAssetPath(prefabArr[i]);
            GameObject prefabGo = AssetDatabase.LoadAssetAtPath<GameObject>(pathName);
            EditorUtility.DisplayProgressBar("Checking", pathName, (float)i / prefabArr.Length);

            if (callback != null && callback(prefabGo, pathName, CheckType.Folder) && !flag)
                flag = true;
        }
        EditorUtility.ClearProgressBar();

        if (!flag)
        {
            Debug.LogError("该资源没有被引用");
        }
    }



    private static GameObject[] objs;
    /// <summary>
    /// 是否有选中检测的对象
    /// </summary>
    /// <returns></returns>
    private static bool CheckSelectObj()
    {
        objs = Selection.gameObjects;
        return objs.Length > 0;
    }

    /// <summary>
    /// 检测GameObject
    /// </summary>
    void CheckObj(CheckDel callback)
    {
        bool flag = false;
        if (objs != null)
        {
            for (int i = 0; i < objs.Length; i ++)
            {
                if (callback != null && callback(objs[i], "", CheckType.GameObject) && !flag)
                    flag = true;
            }
        }
        if (!flag)
        {
            Debug.LogError("该资源没有被引用");
        }
    }


    static MethodInfo clearMethod = null;
    /// <summary>
    /// 清空log信息
    /// </summary>
    private static void ClearConsole()
    {
        if (clearMethod == null)
        {
            Type log = typeof(EditorWindow).Assembly.GetType("UnityEditor.LogEntries");
            clearMethod = log.GetMethod("Clear");
        }
        clearMethod.Invoke(null, null);
    }
}
