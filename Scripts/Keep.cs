using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Keep : BaseProperty
{
    public static Keep Instance;
    //public int MaxLogStorage, MaxStoneStorage, MaxFoodStorage, MaxIronStorage;
    public int MaxStorage;
    private void Start()
    {
        if(Instance == null)
        Instance = this;
    }
    private new void OnEnable()
    {
        if (GameManager.Instance.IsRotating)
        {
            SetMaterial(PreBuilt);
            return;
        }
        else
            SetMaterial(UnFaded);
        PropertyID keepButton = null;
        foreach (PropertyID button in GameManager.Instance.AllIDs)
        {
            button.button.interactable = true;
            if (button.Property.PropertyName == PropertyTypes.Keep)
                keepButton = button;
        }

        keepButton.button.interactable = false;
        if (Level == 1)
        {
            MaxStorage = 1;
        }
        if (Level == 2)
        {
            
        }
        if (Level == 3)
        {
            MaxStorage = 2;
        }
        if (Level == 4)
        {
           
        }
        if (Level == 5)
        {
            MaxStorage = 3;
        }
        if (Level == 6)
        {
            
        }
        if (Level == 7)
        {
            
        }
        if (Level == 8)
        {
            
        }
        if (Level == 9)
        {
            
        }
        if (Level == 10)
        {
           
        }
        PlayerPrefs.SetInt("KeepBought", 1);
        base.OnEnable();
    }
}
