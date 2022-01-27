using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loggery : BaseProperty
{
    private void OnEnable()
    {
        if (GameManager.Instance.IsRotating)
        {
            SetMaterial(PreBuilt);
            return;
        }
        else
            SetMaterial(UnFaded);
        GameManager.Instance.TreeSpawn += SendWorker;
        PropertyTask.Target = transform;
        PropertyTask.Property = this;
        if (!GameManager.Instance.AssignTask(PropertyTask))
            GameManager.Instance.QueuedTasks.Add(PropertyTask);
        base.OnEnable();
    }

    public void SendWorker()
    {
        if(!Worker.Busy && GameManager.Instance.Trees.Count > 0)
        {
            Worker.CurrentTask = null;
            //Worker.Idle = false;

            Worker.CurrentTask = GameManager.Instance.Trees[0].Task;
            GameManager.Instance.Trees.Remove(GameManager.Instance.Trees[0]);
        }        
    }
    public override void ReOpenProperty()
    {
        GameManager.Instance.TreeSpawn += SendWorker;
        PropertyTask.Target = transform;
        PropertyTask.Property = this;
        if (!GameManager.Instance.AssignTask(PropertyTask))
        {
            if (!GameManager.Instance.QueuedTasks.Contains(PropertyTask))
                GameManager.Instance.QueuedTasks.Add(PropertyTask);
        }
            

    }
    public override void CloseProperty()
    {
        //Worker.Idle = false;
        GameManager.Instance.TreeSpawn -= SendWorker;
        Worker.IsLogger = false;
        base.CloseProperty();
    }
    public override void HasActivated()
    {
        SendWorker();
        base.HasActivated();
    }
}
