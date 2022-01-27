using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class BaseProperty : MonoBehaviour, ISellable, IDamagable
{
    public delegate void OnDestroyed(BaseProperty property);
    public event OnDestroyed Destroyed;
    public Tile Tile;
    public int Level;
    public string PropertyName;
    public int LogAmount, StoneAmount;
    private int logAmount, stoneAmount;
    public BaseWorker Worker;
    public Task PropertyTask;
    public Material UnFaded, Faded, PreBuilt;
    public bool IsOn, CanTurnOff, IsLoggery, Init, UpdateNavMesh, IsBurnable;
    public Light PropertyLight;
    public float LightTurnOnSpeed, MaxIntensity;
    private bool NightTime, DayTime, IsDestroyed, IsOnFire;
    public List<ParticleSystem> Fire = new List<ParticleSystem>();
    [HideInInspector]
    public int MaxLevel;
    private int health = 10;
    public int Health
    {
        get { return health; }
        set
        {
            health = value;
            if(Health <= 0)
            {
                StopAllCoroutines();
                IsDestroyed = true;
                Tile.SellProperty();
                IsDestroyed = false;
                Destroyed?.Invoke(this);
            }
        }
    }
    private void Start()
    {
        if (IsBurnable)
            DisasterManager.Instance.BurnableBuildings.Add(this);
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
        logAmount = LogAmount;
        stoneAmount = StoneAmount;
        if(PropertyLight != null)
        {
            LightingManager.Instance.ActivatePropertyLights += TurnOnLight;
            LightingManager.Instance.DeactivatePropertyLights += TurnOffLight;
            MaxIntensity = PropertyLight.intensity;
            PropertyLight.gameObject.SetActive(false);
            if (LightingManager.Instance.NightTime)
                TurnOnLight();
        }
       
        if(UpdateNavMesh)
        {
            gameObject.layer = 10;
            NavMeshSurface nm = FindObjectOfType<NavMeshSurface>();
            nm.UpdateNavMesh(nm.navMeshData);
        }
        else
            gameObject.layer = 9;
        MaxLevel = Tile.PropertyID.Property.MaxLevel;
    }
    public virtual void HasActivated()
    {
        Init = true;
    }
    public virtual void SellProperty()
    {
        CloseProperty();
        PlayerPrefs.SetInt("TilePropertyLevel" + Tile.TileIndex.ToString(), 0);
        Init = false;
        if(!IsDestroyed)
        {
            int ResourcesToDrop = LogAmount + StoneAmount;
            for (int i = 0; i < ResourcesToDrop; i++)
            {
                if (LogAmount > 0)
                {
                    LogAmount--;
                    GameManager.Instance.Logs++;
                    PickUp pickup = Instantiate(Tile.LogsGO, transform.position + new Vector3(Random.Range(-3, 3), .3f, Random.Range(-3, 3)), Quaternion.Euler(0, 0, 90)).GetComponent<PickUp>();
                    GameManager.Instance.AddRougue(pickup, true);
                }
                else if (StoneAmount > 0)
                {
                    StoneAmount--;
                    GameManager.Instance.Stone++;
                    PickUp pickup = Instantiate(Tile.StoneGO, transform.position + new Vector3(Random.Range(-3, 3), .3f, Random.Range(-3, 3)), Quaternion.Euler(0, 0, 90)).GetComponent<PickUp>();
                    GameManager.Instance.AddRougue(pickup, true);
                }
            }
            GameManager.Instance.PickUpRouges();
            LogAmount = logAmount;
            StoneAmount = stoneAmount;
        }        
        if (UpdateNavMesh)
        {
            gameObject.layer = 9;
            NavMeshSurface nm = FindObjectOfType<NavMeshSurface>();
            nm.UpdateNavMesh(nm.navMeshData);
        }
    }
    public void TurnOnOff()
    {

        if (IsOn)
        {
            //Tile.canvas.SetActive(true);
            SetMaterial(Faded);
            IsOn = false;
            CloseProperty();
        }
        else
        {
            //Tile.canvas.SetActive(false);
            SetMaterial(UnFaded);
            IsOn = true;
            ReOpenProperty();
        }

    }

    public virtual void CloseProperty()
    {
        if (IsBurnable)
            DisasterManager.Instance.BurnableBuildings.Remove(this);
        if (PropertyLight != null)
        {
            LightingManager.Instance.ActivatePropertyLights -= TurnOnLight;
            LightingManager.Instance.DeactivatePropertyLights -= TurnOffLight;
        }
            
        if (Worker != null)
        {
            Worker.JobTask = null;
            Worker.HasJob = false;
            Init = false;
            Worker.CheckAgain();
        }       
    }
    public virtual void ReOpenProperty()
    {

    }
    protected virtual void SetMaterial(Material Mat)
    {
        int num = transform.childCount;
        if(transform.GetComponent<MeshRenderer>() != null)
        transform.GetComponent<MeshRenderer>().material = Mat;
        if (num == 0)
        {
            return;
        }            
        for (int i = 0; i < num; i++)
        {
            if (transform.GetChild(i).GetComponent<MeshRenderer>() != null)
            transform.GetChild(i).GetComponent<MeshRenderer>().material = Mat;
        }
    }
    public virtual Transform CollectedCrops() { return null; }
    public virtual void LoadProperty() { }
    public virtual void TurnOnLight()
    {
        //PropertyLight.intensity = 0;
        PropertyLight.gameObject.SetActive(true);
        NightTime = true;
    }
    public virtual void TurnOffLight()
    {
        DayTime = true;
    }
    private void Update()
    { 
        if(NightTime)
        {
            PropertyLight.intensity = Mathf.Lerp(0, MaxIntensity, LightTurnOnSpeed);
            LightTurnOnSpeed += 0.1f * Time.deltaTime;
            if (PropertyLight.intensity >= MaxIntensity)
            {
                //LightTurnOnSpeed = 0;
                NightTime = false;
            }
        }
        else if(DayTime)
        {
            PropertyLight.intensity = Mathf.Lerp(0, MaxIntensity, LightTurnOnSpeed);
            LightTurnOnSpeed -= 0.1f * Time.deltaTime;
            if (PropertyLight.intensity <= 0)
            {
                LightTurnOnSpeed = 0;
                DayTime = false;
                PropertyLight.gameObject.SetActive(false);
            }
        }
    }
    public virtual void UpgradeProperty()
    {
        
    }

    public void TakeDamage(int Damage)
    {
        Health -= Damage;
    }   
    public void SetOnFire()
    {
        IsOnFire = true;
        foreach(ParticleSystem fire in Fire)
        {
            fire.Play();
        }
        StartCoroutine(TakeFireDamage());
        Task FireTask = new Task();
        FireTask.task = Tasks.PutOutFire;
        FireTask.Target = transform;
        FireTask.StoppingDistance = 6;
        FireTask.Property = this;
        FireTask.FireTask = true;
        if (!GameManager.Instance.AssignTask(FireTask))
            GameManager.Instance.QueuedTasks.Add(FireTask);
    }
    IEnumerator TakeFireDamage()
    {
        yield return new WaitForSeconds(5);
        Health--;
        if(IsOnFire)
        {
            StartCoroutine(TakeFireDamage());
        }
            
    }
    public void PutOutFire()
    {
        IsOnFire = false;
        foreach (ParticleSystem fire in Fire)
        {
            fire.Stop();
        }
    }
}
