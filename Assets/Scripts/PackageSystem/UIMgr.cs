using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMgr
{
    private static UIMgr _instance;
    private Transform _uiRoot;

    private Dictionary<string, string> pathDic;
    private Dictionary<string, GameObject> prefabDic;
    public Dictionary<string,BasePanel> panelDic;

    public static UIMgr Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new UIMgr();
            }
            return _instance;
        }
    }

    public Transform UIRoot
    {
        get
        {
            if (_uiRoot == null)
            {
                if(GameObject.Find("Canvas")) _uiRoot = GameObject.Find("Canvas").transform;
                else _uiRoot = new GameObject("Canvas").transform;
            }
            return _uiRoot;
        }
    }

    public BasePanel GetPanel(string panelName)
    {
        BasePanel panel = null;
        if (panelDic.TryGetValue(panelName, out panel))
        {
            return panel;
        }
        return null;
    }

    public BasePanel OpenPanel(string panelName)
    {
        BasePanel panel = null;
        if (panelDic.TryGetValue(panelName, out panel))
        {
            Debug.LogError($"界面已打开：{panelName}");
            return null;
        }

        string path = "";
        if (!pathDic.TryGetValue(panelName, out path))
        {
            Debug.LogError($"界面名称错误，或未配置路径：{panelName}");
            return null;
        }

        GameObject panelPrefab = null;
        if (!prefabDic.TryGetValue(panelName, out panelPrefab))
        {
            string realPath = "Prefab/Panel/" + path;
            panelPrefab = Resources.Load<GameObject>(realPath) as GameObject;
            prefabDic.Add(panelName, panelPrefab);
        }
        GameObject panelObj = GameObject.Instantiate(panelPrefab,UIRoot,false);
        panel = panelObj.GetComponent<BasePanel>();
        panelDic.Add(panelName, panel);
        return panel;
    }

    public bool ClosePanel(string panelName)
    {
        BasePanel panel = null;
        if (!panelDic.TryGetValue(panelName, out panel))
        {
            Debug.LogError($"界面未打开:{panelName}");
            return false;
        }
        panel.ClosePanel();
        return true;
    }

    private UIMgr()
    {
        InitDict();
    }

    private void InitDict()
    {
        prefabDic = new Dictionary<string, GameObject>();
        
        panelDic = new Dictionary<string, BasePanel>();
        pathDic = new Dictionary<string, string>()
        {
            {UIConst.PackagePanel, "Package/PackagePanel"},
        };

    }
     
}

public class UIConst
{
    public const string PackagePanel = "PackagePanel";
}
