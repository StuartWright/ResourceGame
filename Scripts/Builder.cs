using System.Collections;
using System.Collections.Generic;
//using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
public class Builder : MonoBehaviour, IDropOff, ISellable
{
    public int Index, LogAmount, StoneAmount;
    private int LogsToDrop, StonesToDrop;
    public Task task;
    public Tile tile;
    public bool InitiallyBuilt;
    public BaseWorker Worker;
    private ResourseType TypeToDrop;
    private PickUp RemovedResourse;
    public Slider ProgressBar;
    public Image ProgressBarImg;
    private int ProgressBarMaxAmount, ProgressBarCurrentValue;
    private PropertyID Property;
    public Gradient ProgressColour;
    public void LoadBuilder()
    {
        Index = PlayerPrefs.GetInt("BuilderIndex"+tile.TileIndex.ToString());
        LogAmount = PlayerPrefs.GetInt("BuilderLogAmount" + tile.TileIndex.ToString());
        StoneAmount = PlayerPrefs.GetInt("BuilderStoneAmount" + tile.TileIndex.ToString());
        GameManager.Instance.Logs -= LogAmount;
        GameManager.Instance.Stone -= StoneAmount;


    }
    public void Init()
    {
        ProgressBar.value = 0;
        PlayerPrefs.SetInt("BuilderLogAmount"+tile.TileIndex.ToString(), LogAmount);
        PlayerPrefs.SetInt("BuilderStoneAmount"+tile.TileIndex.ToString(), StoneAmount);
        PlayerPrefs.SetInt("BuilderIndex"+tile.TileIndex.ToString(), Index);
        foreach(PropertyID id in GameManager.Instance.AllIDs)
        {
            if(id.Property.Index == Index)
            {
                Property = id;
                break;
            }
        }
        if (CheckHasBuilt())
        {
            //ProgressBar.value = 0;
            tile.BuyProperty(Property, true);
            PlayerPrefs.DeleteKey("BuilderMaxValue" + tile.TileIndex.ToString());
            return;
        }
        task.task = Tasks.PickUp;
        task.StoppingDistance = 2;
        task.buildingToBuild = this;
        task.IsBuilding = true;
        //CheckResourses();     
        
        if(PlayerPrefs.HasKey("BuilderMaxValue" + tile.TileIndex.ToString()))
        {
            ProgressBar.maxValue = PlayerPrefs.GetInt("BuilderMaxValue" + tile.TileIndex.ToString());
            ProgressBar.value = PlayerPrefs.GetInt("BuilderProgressBarAmount" + tile.TileIndex.ToString());
            ProgressBarImg.color = ProgressColour.Evaluate(ProgressBar.value / ProgressBar.maxValue);
        }       
        else
        {
            ProgressBarMaxAmount = StoneAmount + LogAmount;
            ProgressBar.maxValue = ProgressBarMaxAmount;
            PlayerPrefs.SetInt("BuilderMaxValue" + tile.TileIndex.ToString(), ProgressBarMaxAmount);
        }       
        //ProgressBar.value = ProgressBarCurrentValue / ProgressBarMaxAmount;
        if (!GameManager.Instance.AssignTask(task))
        {
            if (LogAmount > 0)
                task.ResourseType = ResourseType.Logs;
            if (StoneAmount > 0)
                task.ResourseType = ResourseType.Stone;////////////////////////////////////////////////////////////////////////////////////////////////////               make iron
            GameManager.Instance.QueuedTasks.Add(task); 
        }           
    }
    public void RecieveResourse(BaseWorker Worker)
    {
        //ProgressBarCurrentValue++;
        //ProgressBar.value = ProgressBarCurrentValue / ProgressBarMaxAmount;
        ProgressBar.value++;
        ProgressBarImg.color = ProgressColour.Evaluate(ProgressBar.value / ProgressBar.maxValue);
        PlayerPrefs.SetInt("BuilderProgressBarAmount" + tile.TileIndex.ToString(), (int)ProgressBar.value);
        switch (TypeToDrop)
        {
            case ResourseType.Logs:
                LogAmount--;
                LogsToDrop++;                             
                break;
            case ResourseType.Stone:
                StoneAmount--;
                StonesToDrop++;               
                break;
        }

        Worker.CurrentTask = null;
        if(CheckHasBuilt())
        {
            InitiallyBuilt = false;
            ProgressBar.value = 0;
            tile.BuyProperty(Property, true);
            PlayerPrefs.DeleteKey("BuilderProgressBarAmount" + tile.TileIndex.ToString());
            PlayerPrefs.DeleteKey("BuilderMaxValue" + tile.TileIndex.ToString());
           // Worker.StoreRef = null;////////////////////////////////////////////////////////////////////////////////////////lets see
            return;
        }
        if (CheckResourses())
        {
            task.task = Tasks.PickUp;
            task.buildingToBuild = this;
            task.IsBuilding = true;
            task.ResourseType = TypeToDrop;
            //if(ExtentionMethods.CheckEmpty(TypeToDrop))///////////////////////////////////not sure...
           // {
             //   GameManager.Instance.QueuedTasks.Add(task);
             //   return;
            //}
            if (!Worker.Busy)
                Worker.CurrentTask = task;
            else
                GameManager.Instance.AssignTask(task);
        }
        else
        {
            task.task = Tasks.PickUp;
            task.buildingToBuild = this;
            task.IsBuilding = true;
            task.ResourseType = TypeToDrop;
            GameManager.Instance.QueuedTasks.Add(task);
        }
    }
    public bool CheckHasBuilt()
    {
        if(LogAmount > 0)
        {          
            return false;
        }
        if(StoneAmount > 0)
        {
            return false;
        }
        return true;
    }
    public bool CheckResourses()
    {
        List<Transform> holder = null;
        ResourseType currentType = ResourseType.Logs;
        string savename = "";
        if(LogAmount > 0)
        {
            currentType = ResourseType.Logs;
            savename = "BuilderLogAmount";
            holder = GameManager.Instance.LogHolders;
        }
        else if(StoneAmount > 0)
        {
            currentType = ResourseType.Stone;
            savename = "BuilderStoneAmount";
            holder = GameManager.Instance.StoneHolders;
        }
        if (GameManager.Instance.RougueResourses.Count > 0)
        {
            foreach (PickUp resource in GameManager.Instance.RougueResourses)
            {
                bool resourceUsed = false;
                foreach (Task obj in GameManager.Instance.QueuedTasks)
                {
                    if (obj.Target == resource.transform)
                    {
                        resourceUsed = true;
                        break;
                    }
                }
                if (resource != null && resource.ResourseType == currentType && !resourceUsed)
                {
                    task.Target = resource.transform;
                    RemovedResourse = resource;
                    GameManager.Instance.AddRougue(resource, false);
                    int res = PlayerPrefs.GetInt(savename + tile.TileIndex.ToString());
                    PlayerPrefs.SetInt(savename + tile.TileIndex.ToString(), res - 1);
                    PlayerPrefs.SetInt("BuilderProgressBarAmount" + tile.TileIndex.ToString(), (int)ProgressBar.value + 1);
                    TypeToDrop = currentType;
                    task.FromHolder = false;
                    return true;
                }
            }
        }
        if (holder.Count > 0)
        {
            for (int i = 0; i < holder.Count; i++)
            {
                if (holder[i].GetComponent<Storage>().StorageAmount > 0)
                {
                    task.Target = holder[i];
                    Worker.StoreRef = holder[i];
                    break;
                }
            }
            task.FromHolder = true;
            TypeToDrop = currentType;
            if (task.Target == null)
                return false;
            int res = PlayerPrefs.GetInt(savename + tile.TileIndex.ToString());
            PlayerPrefs.SetInt(savename + tile.TileIndex.ToString(), res - 1);
            PlayerPrefs.SetInt("BuilderProgressBarAmount" + tile.TileIndex.ToString(), (int)ProgressBar.value + 1);
            return true;
        }
        return false;       
    }

    public void SellProperty()
    {
        GameManager.Instance.Stone += StoneAmount + StonesToDrop;
        GameManager.Instance.Logs += LogAmount + LogsToDrop;
        if (RemovedResourse != null)
            GameManager.Instance.AddRougue(RemovedResourse, true);
        int ResourcesToDrop = LogsToDrop + StonesToDrop;
        for (int i = 0; i < ResourcesToDrop; i++)
        {
            if (LogsToDrop > 0)
            {
                LogsToDrop--;
                //GameManager.Instance.Logs++;
                PickUp pickup = Instantiate(tile.LogsGO, transform.position + new Vector3(Random.Range(-3, 3), .3f, Random.Range(-3, 3)), Quaternion.Euler(0, 0, 90)).GetComponent<PickUp>();
                GameManager.Instance.AddRougue(pickup, true);
            }
            else if (StonesToDrop > 0)
            {
                StonesToDrop--;
                //GameManager.Instance.Stone++;
                PickUp pickup = Instantiate(tile.StoneGO, transform.position + new Vector3(Random.Range(-3, 3), .3f, Random.Range(-3, 3)), Quaternion.Euler(0, 0, 90)).GetComponent<PickUp>();
                GameManager.Instance.AddRougue(pickup, true);
            }
        }
        GameManager.Instance.PickUpRouges();
        PlayerPrefs.DeleteKey("BuilderProgressBarAmount" + tile.TileIndex.ToString());
        LogsToDrop = 0;
        StonesToDrop = 0;
        if(Worker != null)
        Worker.CurrentTask = null;
    }

    public void TurnOnOff()
    {
        
    }
}
