using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindMillRotation : MonoBehaviour
{
    private void LateUpdate()
    {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z + 20 * Time.deltaTime);
    }
}
