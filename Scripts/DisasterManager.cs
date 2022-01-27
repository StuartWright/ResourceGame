using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisasterManager : MonoBehaviour
{
    public static DisasterManager Instance;
    public GameObject LongBoat;
    public List<BaseProperty> BurnableBuildings = new List<BaseProperty>();
    void Start()
    {
        Instance = this;
        StartCoroutine(Disaster());
    }

    IEnumerator Disaster()
    {
        yield return new WaitForSeconds(Random.Range(60, 400));
        if(BurnableBuildings.Count > 0)
        {
            int randomNum = Random.Range(0, BurnableBuildings.Count);
            BurnableBuildings[randomNum].SetOnFire();
        }
        StartCoroutine(Disaster());
    }
}
