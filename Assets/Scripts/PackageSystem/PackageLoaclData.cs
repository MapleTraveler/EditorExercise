using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PackageLoaclData
{
    private static PackageLoaclData _instance;

    public static PackageLoaclData Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new PackageLoaclData();
            }
            return _instance;
        }
    }

    public List<PackageLocalItem> items;

    public void SavePackage()
    {
        string inventoryJson = JsonUtility.ToJson(this);
        PlayerPrefs.SetString("PackageLocalData",inventoryJson);
        PlayerPrefs.Save();
    }

    public List<PackageLocalItem> LoadPackage()
    {
        if(items != null)
            return items;

        if (PlayerPrefs.HasKey("PackageLocalData"))
        {
            string inventoryJson = PlayerPrefs.GetString("PackageLocalData");
            PackageLoaclData packageData = JsonUtility.FromJson<PackageLoaclData>(inventoryJson);
            items = packageData.items;
            return items;
        }
        else
        {
            items = new List<PackageLocalItem>();
            return items;
        }
    }
}

[Serializable]
public class PackageLocalItem
{
    public string uid;
    public int id;
    public int num;
    public int level;
    public bool isNew;
    public override string ToString()
    {
        return $"[id]:{uid},num:{id},level:{num}";
    }
}
