using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class EnemyShip : MonoBehaviour
{
    private NavMeshAgent Agent;
    public Transform SpawnPoint;
    public GameObject EnemyToSpawn;
    public List<GameObject> PropKnights = new List<GameObject>();
    private bool Dismount;
    private void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        Agent.destination = ExtentionMethods.RandomPos();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Terrain")
        {
            if(!Dismount)
            {
                Dismount = true;
                Agent.isStopped = true;
                foreach (GameObject boi in PropKnights)
                    boi.SetActive(false);
                for(int i = 0; i < 3; i++)
                Instantiate(EnemyToSpawn, SpawnPoint.transform.position, transform.rotation);
            }            
        }
    }
}
