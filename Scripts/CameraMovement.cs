using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CameraMovement : MonoBehaviour
{
    public static CameraMovement Instance;
    Vector3 StartPos;
    public bool CanMove, isZooming;
    private void Start()
    {
        Instance = this;
    }

    void LateUpdate()
    {
        if (Input.touchCount > 1)
        {
            Touch TouchOne = Input.GetTouch(0);
            Touch TouchTwo = Input.GetTouch(1);
            Vector2 TouchOnePos = TouchOne.position - TouchOne.deltaPosition;
            Vector2 TouchTwoPos = TouchTwo.position - TouchTwo.deltaPosition;
            //float TouchOneRotation = TouchOne.position.y - TouchOne.deltaPosition.y;
            //float TouchTwoRotation = TouchTwo.position.y - TouchTwo.deltaPosition.y;

            float PrevMagnitude = (TouchOnePos - TouchTwoPos).magnitude;
            float CurrentMagnitude = (TouchOne.position - TouchTwo.position).magnitude;

            //float PrevRotMagnitude = (TouchOneRotation - TouchTwoRotation);
            //float CurrentRotMagnitude = (TouchOne.position.y - TouchTwo.position.y);


            float Difference = CurrentMagnitude - PrevMagnitude;
            float RotDifference = TouchOnePos.y - TouchOne.position.y;
            Zoom(Difference * 0.03f);
            Rotate(RotDifference * 0.08f);
            isZooming = true;
        }
        if (Input.GetMouseButtonDown(0) && !isZooming)
        {
            StartPos = WorldPos();
        }            
        else if(Input.GetMouseButton(0) && !isZooming)
        {            
            if (!CanMove)
            {
                Vector3 NewPos = DistancePos();
                float distance = Vector3.Distance(StartPos, NewPos);
                if (distance > 0.5f)
                    CanMove = true;
            }
            else
            {
                Vector3 Direction = StartPos - WorldPos();               
                
                Camera.main.transform.position += Direction;
                Vector3 pos = Camera.main.transform.position;
                Vector3 clamped = new Vector3(Mathf.Clamp(pos.x, 30,120), pos.y, Mathf.Clamp(pos.z, -20, 100));
                Camera.main.transform.position = clamped;
            }           
        }
        else if (Input.GetMouseButtonUp(0))
        {
            CanMove = false;
            isZooming = false;
        }            
    }
    private Vector3 WorldPos()
    {
        Ray MousePos = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane Ground = new Plane(Vector3.up, new Vector3(0, 0, 0));
        float Distance;
        Ground.Raycast(MousePos, out Distance);
        return MousePos.GetPoint(Distance);
    }
    private Vector3 DistancePos()
    {
        Ray MousePos = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane Ground = new Plane(Vector3.up, new Vector3(0, 0, 0));
        float Distance;
        Ground.Raycast(MousePos, out Distance);
        return MousePos.GetPoint(Distance);
    }

    private void Zoom(float increment)
    {
        Camera.main.gameObject.transform.localPosition = new Vector3(Camera.main.gameObject.transform.localPosition.x, (Camera.main.gameObject.transform.localPosition.y - increment), Camera.main.gameObject.transform.localPosition.z);
        Vector3 pos = Camera.main.transform.localPosition;
        Vector3 clamped = new Vector3(pos.x, Mathf.Clamp(pos.y, 5, 36.5f), pos.z);
        Camera.main.transform.localPosition = clamped;
    }

    private void Rotate(float increment)
    {
        Camera.main.gameObject.transform.eulerAngles = new Vector3(Camera.main.gameObject.transform.transform.eulerAngles.x - increment, Camera.main.gameObject.transform.transform.eulerAngles.y, Camera.main.gameObject.transform.transform.eulerAngles.z);
        float Rot = Camera.main.transform.eulerAngles.x;
        Vector3 clamped = new Vector3(Mathf.Clamp(Rot, 44.4f, 80), -90, 0);
        Camera.main.transform.eulerAngles = clamped;
    }
}
