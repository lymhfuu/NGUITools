using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class LogEditor 
{
    private class LogEditorConfig
    {
        public string logScriptPath = "";   //自定义日志脚本路径
        public string logTypeName = "";     //脚本type
        public int instanceID = 0;

        public LogEditorConfig(string logScriptPath, System.Type logType)
        {
            this.logScriptPath = logScriptPath;
            this.logTypeName = logType.FullName;
        }
    }

    //检测window的日志
    private static LogEditorConfig _CheckDebug = new LogEditorConfig("Assets/ThirdParty/NGUI/Scripts/Editor/NGUITools/EditorLog/CheckDebug.cs", typeof(CheckDebug));

    //处理从ConsoleWindow双击事件
    [UnityEditor.Callbacks.OnOpenAssetAttribute(-1)]
    private static bool OnOpenAsset(int instanceID,int line)
    {
        //实例id匹配失败，直接返回
        if(!GetLogInstanceID(_CheckDebug))
        {
            return false;
        }

        //双击的是检测窗口的log信息，处理双击事件
        if (instanceID == _CheckDebug.instanceID)
        {
            OnCheckResultClick();
            return true;
        }

        return false;
    }

    /// <summary>
    /// 双击检测结果
    /// </summary>
    private static void OnCheckResultClick()
    {
        string logStack = GetStackTrace();
        string[] strs = logStack.Split('|');
        if (strs.Length == 4)//只有能分割出4段的才是正确的格式 xxx|checkType|assistPath|xxx
        {
            try
            {
                var checkType = (CheckWindowBase.CheckType)Enum.Parse(typeof(CheckWindowBase.CheckType), strs[1]);
                var assistPath = strs[2];
                GameObject target = null;
                //文件夹定位
                if (checkType == CheckWindowBase.CheckType.Folder)
                {
                    target = AssetDatabase.LoadAssetAtPath<GameObject>(assistPath);
                }
                //GameObject定位
                else if (checkType == CheckWindowBase.CheckType.GameObject)
                {
                    target = GameObject.Find(assistPath);
                }
                if (null != target)
                {
                    EditorGUIUtility.PingObject(target);
                    Selection.activeGameObject = target;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
        else
        {
            Debug.LogError("路径命名本身含有竖杠 | ；或者工具组装数据格式错误，无法定位。");
        }
    }

    /// <summary>
    /// 拿到自定义Log脚本的instance id
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    private static bool GetLogInstanceID(LogEditorConfig config)
    {
        if (config.instanceID > 0) //已经初始化过 id
        {
            return true;
        }

        var assetLoadTmp = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(config.logScriptPath);
        // 找不到自定义的log脚本，可能是路径设置错了
        if (null == assetLoadTmp)
        {
            Debug.LogError("找不到：" + config.logScriptPath);
            return false;
        }
        //把instance id 保存起来
        config.instanceID = assetLoadTmp.GetInstanceID();
        return true;
    }



    static EditorWindow consoleWindow = null;
    static FieldInfo textFidld = null;
    /// <summary>
    /// 通过反射获得堆栈信息
    /// </summary>
    /// <returns></returns>
    private static string GetStackTrace()
    {
        if (textFidld == null)
        {
            Type console = typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow");
            FieldInfo consoleInfo = console.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
            consoleWindow = consoleInfo.GetValue(null) as EditorWindow;
            textFidld = console.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
        }
        
        if (EditorWindow.focusedWindow == consoleWindow)
        {
            return textFidld.GetValue(consoleWindow).ToString();
        }
        return "";
    }
}