using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockMiner : BaseProperty
{
    public RockMine RockToMine;

    public void OnEnable()
    {
        if (GameManager.Instance.IsRotating)
        {
            SetMaterial(PreBuilt);
            return;
        }
        else
            SetMaterial(UnFaded);
        StartCoroutine(Delay());
        base.OnEnable();
    }
    IEnumerator Delay()//theres a delay to find tiles
    {
        yield return new WaitForSeconds(.21f);
        RockToMine = CheckForStone();
        PropertyTask.Target = RockToMine.transform;
        PropertyTask.Property = this;
        GameManager.Instance.AssignTask(PropertyTask);
        Worker.JobTask = PropertyTask;
    }
    public RockMine CheckForStone()
    {
        foreach (Tile tile in Tile.SurroundingTiles)
        {
            if (tile.Type == PropertyTypes.Stone)
            {
                return tile.Stone.GetComponent<RockMine>();
            }

        }
        return null;
    }
    public override void ReOpenProperty()
    {
        PropertyTask.Target = RockToMine.transform;
        PropertyTask.Property = this;
        if (!GameManager.Instance.AssignTask(PropertyTask))
        {
            if(!GameManager.Instance.QueuedTasks.Contains(PropertyTask))
            GameManager.Instance.QueuedTasks.Add(PropertyTask);
        }
            
            
    }
}
