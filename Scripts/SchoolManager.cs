using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SchoolManager : MonoBehaviour
{
    public static SchoolManager Instance;
    void Start()
    {
        Instance = this;
    }

    public void IncreaseWorkerSpeed()
    {
        foreach(BaseWorker worker in GameManager.Instance.Workers)
        {
            worker.Agent.speed += 5;
            worker.Agent.acceleration += 5;
        }
    }
}
