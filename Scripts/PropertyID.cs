using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
public enum PropertyTypes
{
    Empty,   
    Builder,
    Road,
    House,
    LogStorage,
    StoneStorage,
    StoneMiner,
    Stone,
    Loggery,
    Farm,
    FoodStorage,
    School,
    MineShaft,
    IronStorage,
    Wall,
    WallCorner,
    Keep,
    WindMill
}
public class PropertyID : MonoBehaviour
{
    public Property Property;
    public Button button;
    private void Start()
    {
        if (Property.PropertyName == PropertyTypes.Keep)
        {
            if (PlayerPrefs.GetInt("KeepBought") == 1)
                button.interactable = false;
        }
        else if(Property.PropertyName != PropertyTypes.Keep && PlayerPrefs.GetInt("KeepBought") == 0)
            button.interactable = false;
        //Property.MaxLevel = Property.UpgradeAmounts.Count;
    }
}

[Serializable]
public class Property
{
    public PropertyTypes PropertyName;
    public int LogAmount, StoneAmount, Index;
    public bool MustBenNextToRoad;
    public bool BuildMultiple;   
    public bool IsStorage;   
    public int MaxLevel;
    public List<UpgradeStats> UpgradeAmounts;
    public bool CheckResources()
    {
        if (GameManager.Instance.Logs < LogAmount)
            return false;
        if (GameManager.Instance.Stone < StoneAmount)
            return false;
        else
            return true;
    }
    public bool CheckUpgradeResources(int Level)
    {
        if (GameManager.Instance.Logs < UpgradeAmounts[Level].LogAmount)
            return false;
        if (GameManager.Instance.Stone < UpgradeAmounts[Level].StoneAmount)
            return false;
        else
            return true;
    }

    [Serializable]
    public class UpgradeStats
    {
        public int RequiredKeepLevel;
        public int LogAmount, StoneAmount;
    }

}
