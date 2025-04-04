using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GMCmd
{
    [MenuItem("GMCmd/读表")]
    public static void ReadTable()
    {
        PackageTable packageTable = Resources.Load<PackageTable>("TableData/PackageTable");
        
    }
    [MenuItem("GMCmd/创建背包测试数据")]
    public static void CreateLocalPackageData()
    {

        PackageLoaclData.Instance.items = new List<PackageLocalItem>();
        for (int i = 1; i <= 10; i++)
        {
            PackageLocalItem packageLocalItem = new PackageLocalItem()
            {
                uid = Guid.NewGuid().ToString(),
                id = i,
                num = i,
                level = i,
                isNew = i % 2 == 1
            };
            PackageLoaclData.Instance.items.Add(packageLocalItem);
        }
        PackageLoaclData.Instance.SavePackage();
    }
    [MenuItem("GMCmd/读取背包测试数据")]
    public static void ReadLocalPackageData()
    {
        List<PackageLocalItem> readItems = PackageLoaclData.Instance.LoadPackage();
        foreach (var item in readItems)
        {
            Debug.Log(item);
        }
    }

    [MenuItem("GMCmd/打开背包主界面")]
    public static void OpenPackagePanel()
    {
        UIMgr.Instance.OpenPanel(UIConst.PackagePanel);
    }
    
}
