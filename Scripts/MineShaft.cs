using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineShaft : BaseProperty, IDamagable
{
    public int Health;
    public GameObject Drop;
    public void TakeDamage(int Damage)
    {
        Health -= Damage;
        if (Health <= 0)
        {
            Mined();
            Health = 3;
        }
    }

    public virtual void Mined()
    {
        Worker.CurrentTask = null;
        PickUp pickup = Instantiate(Drop, transform.position + new Vector3(Random.Range(Random.Range(-4, -3), Random.Range(3, 4)), 0, Random.Range(Random.Range(-4, -3), Random.Range(3, 4))), Quaternion.Euler(0, 0, 90)).GetComponent<PickUp>();
        if (ExtentionMethods.CheckHolders(pickup.ResourseType, Worker, pickup))
            Worker.CurrentTask = pickup.Task;
        else
        {
            GameManager.Instance.AddRougue(pickup, true);
            Worker.CurrentTask = Worker.JobTask;
        }

        GameManager.Instance.Iron++;
    }

    public void OnEnable()
    {
        if (GameManager.Instance.IsRotating)
        {
            SetMaterial(PreBuilt);
            return;
        }
        else
            SetMaterial(UnFaded);
        PropertyTask.Target = transform;
        PropertyTask.Property = this;
        GameManager.Instance.AssignTask(PropertyTask);
        Worker.JobTask = PropertyTask;
        base.OnEnable();
    }
    public override void ReOpenProperty()
    {
        PropertyTask.Target = transform;
        PropertyTask.Property = this;
        if (!GameManager.Instance.AssignTask(PropertyTask))
        {
            if (!GameManager.Instance.QueuedTasks.Contains(PropertyTask))
                GameManager.Instance.QueuedTasks.Add(PropertyTask);
        }


    }
}
