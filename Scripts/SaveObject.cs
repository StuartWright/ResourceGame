using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveObject : MonoBehaviour
{
    private string FileName = "PhoneReset.txt";
    public string FilePath;
    public string Test;
    public static SaveObject Instance;
    public GameData Data;
    private bool DataDeleted, LoadingData = true;
    private void Start()
    {
        Instance = this;
        if (File.Exists(Application.persistentDataPath + FileName))
        {
            using (FileStream file = File.Open(Application.persistentDataPath + FileName, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                Data = formatter.Deserialize(file) as GameData;
            }
        }
        else
            Data = new GameData();
        //Player.LeveledUp += Save;/////////////////////////////////
        Load();
    }
    
    public void Save()
    {
        //if (LoadingData)
            //return;
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + FileName, FileMode.OpenOrCreate);

        //save stuff
        //SavePlayerInventory();
        Data.Rouges = GameManager.Instance.RougueResourses;
        formatter.Serialize(file, Data);
        file.Close();
    }
    public void Load()
    {
        if (File.Exists(Application.persistentDataPath + FileName))
        {
            using (FileStream file = File.Open(Application.persistentDataPath + FileName, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                GameData data = formatter.Deserialize(file) as GameData;
                //GameManager.Instance.RougueResourses = data.Rouges;
                //GameManager.Instance.LoadRouges();
            }
        }
        //LoadingData = false;
    }
    public void DeleteData()
    {
        PlayerPrefs.DeleteAll();
        DataDeleted = true;
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + FileName, FileMode.Open);

        Data.Rouges.Clear();
        formatter.Serialize(file, Data);
        file.Close();
        Application.Quit();
    }
    public void SavePlayerInventory()
    {
        
        /*
        foreach (Items item in PlayerInventory.Items)
        {
            ItemStats newItem = new ItemStats();
            newItem.ItemName = item.ItemName;
            newItem.Strength = item.Strength;
            newItem.Intelligence = item.Intelligence;
            newItem.Dexterity = item.Dexterity;
            newItem.Agility = item.Agility;
            newItem.PhysicalDamage = item.PhysicalDamage;
            newItem.Defence = item.Defence;
            newItem.StackAmount = item.StackAmount;
            newItem.ItemPrice = item.ItemPrice;
            newItem.type = item.type;
            newItem.EquipmentType = item.EquipmentType;
            newItem.ShopItem = item.ShopItem;
            newItem.weaponType = item.weaponType;
            newItem.CanDrop = item.CantDrop;
            Data.Stats.Add(newItem);
        }
        */        
    }
    /*
    private void OnApplicationQuit()
    {
        if (!DataDeleted)
            Save();
    }
    */
    private void Update()
    {
        if (Input.GetKeyDown("s"))
            Save();
        if (Input.GetKeyDown("d"))
            DeleteData();
    }
}
[Serializable]
public class GameData
{
    public List<PickUp> Rouges = new List<PickUp>();
}



