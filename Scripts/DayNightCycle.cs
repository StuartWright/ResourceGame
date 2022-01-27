using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Time")]
    [Tooltip("Day Length in Mins")]
    public float DayLength = 0.5f;//Length of day in mins
    [Range(0,1)]
    public float TimeOfDay;
    public float YearLength = 100;
    public int Day = 0;
    public int Year = 0;
    public float TimeScale = 100;
    public bool Paused;
    public Light Sun;
    public float Intensity;
    public float SunBaseIntensity = 1;
    public float SunVarient = 1.5f;
    public Gradient SunColour;
    public void UpdateTimeScale()
    {
        TimeScale = 24 / (DayLength / 60);
    }
    public void UpdateTime()
    {
        TimeOfDay += Time.deltaTime * TimeScale / 86400;
        if(TimeOfDay > 1)
        {
            Day++;
            GameManager.Instance.FeedingTimeBois();
            TimeOfDay -= 1;
            if(Day > YearLength)
            {
                Year++;
                Day = 0;
            }
        }
    }
    private void Update()
    {
        if(!Paused)
        {
            UpdateTimeScale();
            UpdateTime();
        }
        AdjustSunRotation();
        SunIntensity();
        AdjustSunColour();
    }
    public Transform DailyRotation;
    public void AdjustSunRotation()
    {
        float SunAngle = TimeOfDay * 360;
        DailyRotation.transform.localRotation = Quaternion.Euler(new Vector3(SunAngle, 0, 0));
    }
    public void SunIntensity()
    {
        Intensity = Vector3.Dot(Sun.transform.forward, Vector3.down);
        Intensity = Mathf.Clamp01(Intensity);
        Sun.intensity = Intensity * SunVarient * SunBaseIntensity;
    }
    public void AdjustSunColour()
    {
        Sun.color = SunColour.Evaluate(Intensity);
    }
}
