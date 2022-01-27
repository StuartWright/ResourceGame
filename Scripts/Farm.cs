using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class Farm : BaseProperty
{
    public delegate void CropsEvent();
    public event CropsEvent FullyGrown;
    public Mesh UnGrown, Grown;
    public MeshFilter MR;
    public GameObject Food;
    public DOTweenAnimation WheetAnim;
    public Transform WheetTransform;
    public bool ReadyToCollect;
    public ParticleSystem Stars;
    private bool IsByWindMill;
    private float NormalDuration = 30, BuffedDuration = 5;
    private float CurrentDuration;
    private void Start()
    {
        Tile.TilesFound += SearchForMill;
    }
    private new void OnEnable()
    {
        if (GameManager.Instance.IsRotating)
        {
            //WheetAnim.DORewind();
            SetMaterial(PreBuilt);
            return;
        }
        else
            SetMaterial(UnFaded);        
        RemoveWindmillEffect();
        PropertyTask.Target = transform;
        PropertyTask.Property = this;
        if (!GameManager.Instance.AssignTask(PropertyTask))
            GameManager.Instance.QueuedTasks.Add(PropertyTask);        
        //StartCoroutine(CheckToCollect());
        //WheetAnim.autoPlay = true;
        SearchForMill();       
        base.OnEnable();
    }

    private void SearchForMill()
    {
        Tile.TilesFound -= SearchForMill;
        foreach (Tile tile in Tile.SurroundingTiles)
        {
            if (tile.Type == PropertyTypes.WindMill)
            {
                NearWindmill();
            }
        }
    }

    private void StartGrow()
    {
        //WheetAnim.DORewindAndPlayNext();
        CurrentDuration = GetAmount();
        WheetTransform.DOKill();
        WheetTransform.DOMoveY(-1f, 0);
        WheetTransform.DOMoveY(0.02f, CurrentDuration);
    }
    private void LateUpdate()
    {
        if(CurrentDuration > 0)
        {
            CurrentDuration -= Time.deltaTime;
            if(CurrentDuration <= 0)
            {
                //GameManager.Instance.SetFakes(true);//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                if (ExtentionMethods.CheckHolders(ResourseType.Food, Worker))
                {
                    FullyGrown();
                }
                else
                {
                    ReadyToCollect = true;
                }
            }
        }
    }
    /*
    IEnumerator GrowCrops()
    {
        WheetAnim.DORewindAndPlayNext();
        if(!IsByWindMill)
            yield return new WaitForSeconds(30);
        else
            yield return new WaitForSeconds(5);
        //MR.mesh = Grown;
        //if (GameManager.Instance.FoodHolders.Count > 0)
        if (ExtentionMethods.CheckHolders(ResourseType.Food, Worker))
        {           
            FullyGrown();
        }   
        else
        {
            ReadyToCollect = true;
        }      
    }
    */
    private float GetAmount()
    {
        if (IsByWindMill)
            return BuffedDuration;
        else
            return NormalDuration;
    }
    public void Collect()
    {
        ReadyToCollect = false;
        FullyGrown();
    }
    public override void CloseProperty()
    {
        if (Worker != null)
        {            
            FullyGrown -= Worker.CropsGrown;
            Worker.IsFarmer = false;
            Worker.FarmRef = null;
            Worker.CurrentTask = null;
            MR.mesh = UnGrown;
        }
        base.CloseProperty();
    }
    public override Transform CollectedCrops()
    {
        MR.mesh = UnGrown;
        StartGrow();
        return Instantiate(Food, transform.position, transform.rotation).transform;
    }
    public override void HasActivated()
    {
        FullyGrown += Worker.CropsGrown;
        Worker.IsFarmer = true;
        Worker.FarmRef = this;
        StartGrow();
        base.HasActivated();
    }
    public void CheckToCollect()
    {
        //yield return new WaitForSeconds(3);
        if (ReadyToCollect)
        {
            //GameManager.Instance.SetFakes();
            if (ExtentionMethods.CheckHolders(ResourseType.Food, Worker, null))
            {
                //storeRef.StorageAmount++;
                //Worker.StoreRef = storeRef.transform;
                ReadyToCollect = false;
                FullyGrown();
            }
        }      
        //StartCoroutine(CheckToCollect());
    }
    public void NearWindmill()
    {
        //CurrentDuration = BuffedDuration;
        IsByWindMill = true;
        Stars.Play();
        //WheetAnim.DORewindAndPlayNext();
        StartGrow();

    }
    public void RemoveWindmillEffect()
    {
        //CurrentDuration = NormalDuration;
        IsByWindMill = false;
        //WheetAnim.DORewindAndPlayNext();
        StartGrow();
        Stars.Stop();
    }
}
