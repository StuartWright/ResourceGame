using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class House : BaseProperty
{
    public int HouseSpace, CurrentAmount;
    //public Vector3 WorkerStartPos = new Vector3(131,1.15f,40.21f);
    void OnEnable()
    {
        if (GameManager.Instance.IsRotating)
        {
            SetMaterial(PreBuilt);
            return;
        }
        else
            SetMaterial(UnFaded);
        switch (Level)
        {
            case 1:
                HouseSpace = 2;
                break;
            case 2:
                HouseSpace = 4;
                break;
        }
        GameManager.Instance.Housing += HouseSpace;
        GameManager.Instance.Houses.Add(this);
        GameManager.Instance.AssignHome();
        Worker = null;       
        base.OnEnable();
    }

    public override void SellProperty()
    {
        GameManager.Instance.Housing -= HouseSpace;
        GameManager.Instance.Houses.Remove(this);
        GameManager.Instance.AssignHome();
        base.SellProperty();
    }
}
