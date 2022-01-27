using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using TMPro;

[ExecuteAlways]
public class LightingManager : MonoBehaviour
{
    public static LightingManager Instance;
    public delegate void ActivateLights();
    public event ActivateLights ActivatePropertyLights;
    public event ActivateLights DeactivatePropertyLights;
    public TextMeshProUGUI TimeText;
    public Light DirectionalLight;
    public LightingPreset Preset;
    [Range(0,24)] public float TimeOfDay;
    public Transform Clock;
    public Material Water;
    private bool lightenWater, darkenWater, LightenColourGrade, DarkenColourGrade, CheckHomes, HasAte, DarkenFloor, LightenFloor;
    public bool NightTime;
    public float DarkenTime = 0, LightenTime = 0, FloorDarkenTime = 0, FloorLightenTime = 0;
    public PostProcessVolume Volume;
    private ColorGrading grading;
    public Color FloorDay, FloorNight;
    private float GradientValue = 0.40f;
    TerrainLayer[] tlayers;
    public void OnValidate()
    {
        if (DirectionalLight != null)
            return;
        if(RenderSettings.sun != null)
        {
            DirectionalLight = RenderSettings.sun;
        }
        else
        {
            Light[] lights = GameObject.FindObjectsOfType<Light>();
            foreach(Light light in lights)
            {
                DirectionalLight = light;
                return;
            }
        }
    }
    private void Start()
    {
        tlayers = Terrain.activeTerrain.terrainData.terrainLayers;
        if (Volume != null)
        {            
            Volume.profile.TryGetSettings(out grading);
            if (grading != null) { grading.gain.value = new Vector4(0.40f, 0.40f, 0.40f, 0.40f); }
        }

    }
    private void Awake()
    {
        Instance = this;
        ActivatePropertyLights += DarkenWater;
        DeactivatePropertyLights += LightenWater;
        Water.SetColor("_DepthGradientDeep", Water.GetColor("_DepthGradientDeepDay"));
    }
    public void UpdateLighting(float TimePercent)
    {
        RenderSettings.ambientLight = Preset.AmbientColour.Evaluate(TimePercent);
        //RenderSettings.fogColor = Preset.FogColour.Evaluate(TimePercent);

        if(DirectionalLight != null)
        {
            DirectionalLight.color = Preset.DirectionalColour.Evaluate(TimePercent);
            DirectionalLight.transform.localRotation = Quaternion.Euler(new Vector3((TimePercent * 360f) - 90f, 170f, 0));
            Clock.rotation = Quaternion.Euler(new Vector3(0, 0, (-TimePercent * 360f) - 180));
        }
    }
    public float DayLength = 0.5f;//Length of day in mins
    public float TimeScale = 100;
    void LateUpdate()
    {
        if (Preset == null)
            return;
        if(Application.isPlaying)
        {
            //TimeOfDay += Time.deltaTime;
            TimeScale = 24 / (DayLength / 60);
            TimeOfDay += Time.deltaTime * TimeScale / 86400;
            TimeText.text = (int)TimeOfDay + ":00";
            //TimeOfDay %= 24; // clamb between 0-24
            if (TimeOfDay >= 24)
            {
                TimeOfDay = 0;
                print("ok den");
                GameManager.Instance.FeedingTimeBois();
                GameManager.Instance.SpawnPeople();
            }
            if(TimeOfDay > 12 && TimeOfDay < 12.1f)
            {
                if(!HasAte)
                {
                    HasAte = true;
                    GameManager.Instance.FeedingTimeBois();
                }
                
            }
            else if (TimeOfDay > 12.15f && TimeOfDay < 12.2f)
                HasAte = false;
            if (TimeOfDay > 17 && TimeOfDay < 17.1f)
            {
                NightTime = true;
                DarkenFloor = true;
                
                ActivatePropertyLights?.Invoke();
                DarkenWater();
                CheckForHomes();                
            }                
            if (TimeOfDay > 6 && TimeOfDay < 6.1f)
            {
                NightTime = false;
                LightenFloor = true;
                DeactivatePropertyLights?.Invoke();
            }
            if (TimeOfDay > 16.5f && TimeOfDay < 16.6f)
            {
                DarkenColourGrade = true;
            }
            if (TimeOfDay > 7 && TimeOfDay < 7.1f)
            {
                LightenColourGrade = true;                
            }
            UpdateLighting(TimeOfDay / 24f);
            
            if(darkenWater)
            {
                Water.SetColor("_DepthGradientDeep", Color.Lerp(Water.GetColor("_DepthGradientDeepDay"), Water.GetColor("_DepthGradientDeepNight"), DarkenTime));               
                DarkenTime += 0.2f * Time.deltaTime;
                if(DarkenTime > 0.8f)
                {
                    DarkenTime = 0;
                    darkenWater = false;
                    CheckHomes = false;
                }
            }
            else if(lightenWater)
            {
                Water.SetColor("_DepthGradientDeep", Color.Lerp(Water.GetColor("_DepthGradientDeep"), Water.GetColor("_DepthGradientDeepDay"), LightenTime));               
                LightenTime += 0.1f * Time.deltaTime;
                if (LightenTime > 0.8f)
                {
                    LightenTime = 0;
                    lightenWater = false;
                }
            }
            if (DarkenColourGrade)
            {
                grading.gain.value = new Vector4(GradientValue, GradientValue, GradientValue, GradientValue);                             
                GradientValue -= 0.02f * Time.deltaTime;
                if (GradientValue <= 0)
                {
                    GradientValue = 0;
                    DarkenColourGrade = false;
                }
            }
            else if (LightenColourGrade)
            {
                grading.gain.value = new Vector4(GradientValue, GradientValue, GradientValue, GradientValue);
                GradientValue += 0.02f * Time.deltaTime;
                if (GradientValue >= 0.40f)
                {
                    GradientValue = 0.40f;
                    LightenColourGrade = false;
                }
            }
            if(DarkenFloor)
            {
                tlayers[0].diffuseRemapMax = Color.Lerp(FloorDay, FloorNight, FloorDarkenTime);
                FloorDarkenTime += 0.03f * Time.deltaTime;
                if (FloorDarkenTime > 1f)
                {
                    FloorDarkenTime = 0;
                    DarkenFloor = false;
                }
            }
            else if(LightenFloor)
            {
                tlayers[0].diffuseRemapMax = Color.Lerp(FloorNight, FloorDay, FloorLightenTime);
                FloorLightenTime += 0.03f * Time.deltaTime;
                if (FloorLightenTime > 1f)
                {
                    FloorLightenTime = 0;
                    LightenFloor = false;
                }
            }
        }
        else
        {
            UpdateLighting(TimeOfDay / 24f);
        }

        if (Input.GetKey("z"))
            ActivatePropertyLights?.Invoke();
        if (Input.GetKey("x"))
            DeactivatePropertyLights?.Invoke();
    }
    private void OnApplicationQuit()
    {
        //grading.gain.value = new Vector4(0.40f, 0.40f, 0.40f, 0.40f);
        Water.SetColor("_DepthGradientDeep", Water.GetColor("_DepthGradientDeepDay"));
        tlayers[0].diffuseRemapMax = FloorDay;
    }
    /*
    private IEnumerator Stupdate()
    {
        yield return new WaitForSeconds(0.2f);
        if (Preset != null)
        {
            //TimeOfDay += Time.deltaTime;
            TimeScale = 24 / (DayLength / 60);
            TimeOfDay += Time.deltaTime * TimeScale / 86400;
            //TimeOfDay %= 24; // clamb between 0-24
            if (TimeOfDay >= 24)
            {
                TimeOfDay = 0;
                print("ok den");
                GameManager.Instance.FeedingTimeBois();
            }

            UpdateLighting(TimeOfDay / 24f);
        }
        StartCoroutine(Stupdate());
    }
    */
    public void DarkenWater()
    {
        darkenWater = true;        
        //Water.SetColor("_DepthGradientDeep", new Color(0,0,0,1));
    }
    public void LightenWater()
    {
        lightenWater = true;
        LightenColourGrade = true;
    }
    private void CheckForHomes()
    {
        if(!CheckHomes)
        {
            List<BaseWorker> LeavingWorkers = new List<BaseWorker>();
            CheckHomes = true;
            foreach(BaseWorker worker in GameManager.Instance.Workers)
            {
                if(worker.Home == null)
                {
                    worker.Happy--;
                    worker.SadImg.DOPlayForward();
                    if (worker.IsLeaving)
                        LeavingWorkers.Add(worker);
                }
            }
            foreach (BaseWorker worker in LeavingWorkers)
            {
                GameManager.Instance.Workers.Remove(worker);
            }
            GameManager.Instance.SetHappiniessText();
        }
    }
}
