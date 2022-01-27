using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockMine : BaseResourse
{
    public override void TakeDamage(int Damage)
    {
        Health -= Damage;
        if (Health <= 0)
        {
            Mined();
            Health = 10;
        }
    }

    public virtual void Mined()
    {
        Worker.CurrentTask = null;

        PickUp pickup = Instantiate(Drop, transform.position + new Vector3(Random.Range(Random.Range(-4, -3), Random.Range(3,4)), 0, Random.Range(Random.Range(-4, -3), Random.Range(3,4))), Quaternion.Euler(0, 0, 90)).GetComponent<PickUp>();
        if (ExtentionMethods.CheckHolders(pickup.ResourseType, Worker, pickup))
            Worker.CurrentTask = pickup.Task;
        else
        {
            GameManager.Instance.AddRougue(pickup, true);
            if(Worker.HasJob)
            Worker.CurrentTask = Worker.JobTask;
        }
            
        GameManager.Instance.Stone++;
        if (Canvas != null)
            Canvas.SetActive(false);
    }

}
