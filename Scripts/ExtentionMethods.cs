using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtentionMethods
{
    public static GameObject Logs, Stone;
    private static RaycastHit Hit;
    public static bool CheckHolders(ResourseType type, BaseWorker Worker, PickUp item = null)
    {
        //GameManager.Instance.SetFakes();
        List<Transform> holder = null;
        switch (type)
        {
            case ResourseType.Logs:
                holder = GameManager.Instance.LogHolders;
                break;
            case ResourseType.Stone:
                holder = GameManager.Instance.StoneHolders;
                break;
            case ResourseType.Food:
                holder = GameManager.Instance.FoodHolders;
                break;
            case ResourseType.Iron:
                holder = GameManager.Instance.IronHolders;
                break;
        }
        if (holder.Count > 0)
        {
            for (int i = 0; i < holder.Count; i++)
            {
                Storage tempholder = holder[i].GetComponent<Storage>();
                if (!holder[i].GetComponent<Storage>().IsFull)
                {
                    if(tempholder.ResourseType == ResourseType.Food)
                    {
                        tempholder.StorageAmount++;
                    }
                    //tempholder.AmountOnTheWay++;
                    if (item != null)
                        tempholder.SaveResourse(item);
                    if (Worker != null)
                        Worker.StoreRef = holder[i];
                    return true;
                }
                if (i + 1 == holder.Count)
                {
                    return false;
                }
            }
            return false;
        }
        else
            return false;       
    }

    public static Vector3 RandomPos()
    {
        bool CanPlace = false;
        while (!CanPlace)
        {
            Vector3 pos = new Vector3(Random.Range(2, 98), 0, Random.Range(2, 98));
            if (Physics.Raycast(pos + new Vector3(0, 10, 0), Vector3.down, out Hit, 12))
            {
                if (Hit.collider.name == "Tile(Clone)" && Hit.collider.GetComponent<Tile>().IsEmpty)
                {
                    CanPlace = true;
                    return pos;
                }
            }
        }
        return new Vector3(0, 0, 0);
    }

    public static bool CheckEmpty(ResourseType type, BaseWorker Worker = null)
    {
        List<Transform> holder = null;
        switch(type)
        {
            case ResourseType.Logs:
                holder = GameManager.Instance.LogHolders;
                break;
            case ResourseType.Stone:
                holder = GameManager.Instance.StoneHolders;
                break;
            case ResourseType.Food:
                holder = GameManager.Instance.FoodHolders;
                break;
            case ResourseType.Iron:
                holder = GameManager.Instance.IronHolders;
                break;
        }
        foreach(Transform storage in holder)
        {
            Storage Holder = storage.GetComponent<Storage>();
            if (Holder.StorageAmount == 0)
            {
                return true;
            }
            else
            {
                if(Worker != null)
                Worker.StoreRef = Holder.transform;
                return false;
            }
        }
        return false;
    }
    public static bool CheckResources(int LogAmount, int StoneAmount)
    {
        if (GameManager.Instance.Logs < LogAmount)
            return false;
        if (GameManager.Instance.Stone < StoneAmount)
            return false;
        else
            return true;
    }
}
