using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Task
{
    public Transform Target;
    public Tasks task;
    public float StoppingDistance;
    public bool FromHolder, IsBuilding, InitInteruption, LeavingTask, FireTask;
    public Builder buildingToBuild;
    public BaseProperty Property;
    public Transform StorRef;
    [HideInInspector]
    public ResourseType ResourseType;
    public bool CanActivatePickupTask(BaseWorker worker)
    {
        GameManager GM = GameManager.Instance;
        if(GM.RougueResourses.Count > 0)
        {
            for(int i = 0; i < GM.RougueResourses.Count; i++)
            {
                if(GM.RougueResourses[i].ResourseType == ResourseType && ExtentionMethods.CheckHolders(ResourseType, worker))
                {
                    if (worker.StoreRef != null)
                        worker.StoreRef.GetComponent<Storage>().SaveResourse(GM.RougueResourses[i]);
                    if(Target != null)
                    GM.AddRougue(Target.GetComponent<PickUp>(), false);
                    return true;
                }
            }
        }
        if(!ExtentionMethods.CheckEmpty(ResourseType, worker))
        {
            return true;
        }
        return false;



        /*
                    if (GM.RougueResourses.Count == 0 && !ExtentionMethods.CheckEmpty(GM.QueuedTasks[i].ResourseType, this))
                    {
                        CurrentTask = GM.QueuedTasks[i];
                        if (CurrentTask.StorRef != null)
                            StoreRef = CurrentTask.StorRef;
                        GM.QueuedTasks.Remove(GM.QueuedTasks[i]);
                        break;
                    }
                    else
                    {
                        for(int j = 0; j < GM.RougueResourses.Count; j++)
                        {
                            if (GM.RougueResourses.Count > 0 && ExtentionMethods.CheckHolders(GM.RougueResourses[j].ResourseType, this))
                            {
                                CurrentTask = GM.QueuedTasks[i];
                                StoreRef = CurrentTask.StorRef;
                                if (StoreRef != null)
                                    StoreRef.GetComponent<Storage>().SaveResourse(GM.RougueResourses[j]);
                                GM.QueuedTasks.Remove(GM.QueuedTasks[i]);
                                GM.AddRougue(CurrentTask.Target.GetComponent<PickUp>(), false);
                                BreakOut = true;
                                break;
                            }
                        }
                        if (BreakOut)
                            break;
                    }                                     
                        continue;
                    */
    }
}
