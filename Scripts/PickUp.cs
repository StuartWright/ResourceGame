using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ResourseType
{
    Logs,
    Stone,
    Food,
    Iron
}
[Serializable]
public class PickUp : MonoBehaviour
{
    public Task Task;
    public ResourseType ResourseType;
    public Vector3 Pos;
    public bool HasSaved;
    private void Start()
    {
        Pos = transform.position;
        Task.ResourseType = ResourseType;
    }

}
