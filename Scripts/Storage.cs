using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storage : BaseProperty, IDropOff
{
    public ResourseType ResourseType;
    public List<GameObject> VisualObjects = new List<GameObject>();
    public GameObject ResourseGO;
    public int MaxStorageAmount;
    public bool IsFull;
    private bool HasUpgraded;
    //[HideInInspector]
    public int FakeAmount;
    [SerializeField] int storageAmount;
    public bool IsGonaBeFull = false;
    public List<int> MaxStorageAmounts;
    private int TotalSaveAmount;
    public int StorageAmount
    {
        get { return storageAmount; }
        set
        {
            storageAmount = value;
            if (storageAmount < MaxStorageAmount)
                IsFull = false;
            else
                IsFull = true;                   
        }
    }

    new void OnEnable()
    {
        if (GameManager.Instance.IsRotating)
        {
            SetMaterial(PreBuilt);
            return;
        }
        else
            SetMaterial(UnFaded);
        switch (Level)
        {
            case 1:
                MaxStorageAmount = MaxStorageAmounts[0];
                break;
            case 2:
                MaxStorageAmount = MaxStorageAmounts[1];
                break;
            case 3:
                MaxStorageAmount = MaxStorageAmounts[2];
                break;
            case 4:
                MaxStorageAmount = MaxStorageAmounts[3];
                break;
        }

        foreach (GameObject obj in VisualObjects)
            obj.SetActive(false);
        if(HasUpgraded)
        {
            for(int i = 0; i < StorageAmount; i++)
            {
                VisualObjects[i].SetActive(true);
            }
            HasUpgraded = false;
        }          
        switch (ResourseType)
        {
            case ResourseType.Logs:
                GameManager.Instance.LogHolders.Add(transform);               
                break;
            case ResourseType.Stone:
                GameManager.Instance.StoneHolders.Add(transform);
                break;
            case ResourseType.Food:
                GameManager.Instance.FoodHolders.Add(transform);
                Farm[] Farms = FindObjectsOfType<Farm>();
                foreach(Farm farm in Farms)
                {
                    if(farm.ReadyToCollect)
                    {
                        if (farm.Worker.StoreRef == null)
                            farm.Worker.StoreRef = transform;
                        farm.Collect();
                        farm.ReadyToCollect = false;
                    }
                }
                break;
            case ResourseType.Iron:
                GameManager.Instance.IronHolders.Add(transform);
                break;
        }
        GameManager.Instance.SetFakeAmounts += SetFakeAmount;
        StartCoroutine(WaitToCollect());
        base.OnEnable();
    }

    IEnumerator WaitToCollect()
    {
        yield return new WaitForSeconds(1);
        GameManager.Instance.PickUpRouges();
    }
    public override void LoadProperty()
    {

        switch (Level)
        {
            case 1:
                MaxStorageAmount = MaxStorageAmounts[0];
                break;
            case 2:
                MaxStorageAmount = MaxStorageAmounts[1];
                break;
            case 3:
                MaxStorageAmount = MaxStorageAmounts[2];
                break;
            case 4:
                MaxStorageAmount = MaxStorageAmounts[3];
                break;
        }
        int t = PlayerPrefs.GetInt(PropertyName + Tile.TileIndex);
        if (t <= MaxStorageAmount)
        {
            StorageAmount += t;
            for (int i = 0; i < StorageAmount; i++)
            {
                VisualObjects[i].SetActive(true);
            }
        }
        else
        {
            StorageAmount += MaxStorageAmount;
            for (int i = 0; i < StorageAmount; i++)
            {
                VisualObjects[i].SetActive(true);
            }
        }
        switch (ResourseType)
        {
            case ResourseType.Logs:
                GameManager.Instance.LogStoreAmount += StorageAmount;
                GameManager.Instance.Logs += StorageAmount;
                break;
            case ResourseType.Stone:
                GameManager.Instance.StoneStoreAmount += StorageAmount;
                GameManager.Instance.Stone += StorageAmount;
                break;
            case ResourseType.Food:
                GameManager.Instance.FoodStoreAmount += StorageAmount;
                GameManager.Instance.Food += StorageAmount;
                break;
            case ResourseType.Iron:
                GameManager.Instance.IronStoreAmount += StorageAmount;
                GameManager.Instance.Iron += StorageAmount;
                break;
        }
    }
    public void RecieveResourse(BaseWorker Worker)
    {
        if(Worker != null)
        Worker.CurrentTask = null;
        if(ResourseType != ResourseType.Food)
        StorageAmount++;
        foreach(GameObject obj in VisualObjects)
        {
            if (!obj.activeInHierarchy)
            {
                obj.SetActive(true);
                break;
            }                
        }       
    }
    
    public void SaveResourse(PickUp item)
    {
        if (item.HasSaved)
        {
            return;
        }
        else
            item.HasSaved = true;
        int t = 0;
        t = PlayerPrefs.GetInt(PropertyName + Tile.TileIndex);
        t++;
        PlayerPrefs.SetInt(PropertyName + Tile.TileIndex, t);
    }
    
    public void RemoveResourse()
    {
        IsGonaBeFull = false;
        StorageAmount--;
        for (int i = VisualObjects.Count - 1; i >= 0; i--)
        {
            if (VisualObjects[i].activeInHierarchy)
            {
                VisualObjects[i].SetActive(false);
                
                break;
            }                
        }
        int t = 0;
        t = PlayerPrefs.GetInt(PropertyName + Tile.TileIndex);
        t--;
        PlayerPrefs.SetInt(PropertyName + Tile.TileIndex, t);
    }
    public Transform SpawnResourse()
    {
       return Instantiate(ResourseGO,transform.position, transform.rotation).transform;
    }
    public override void SellProperty()
    {
        GameManager.Instance.SetFakeAmounts -= SetFakeAmount;
        IsGonaBeFull = false;       
        PlayerPrefs.SetInt(PropertyName + Tile.TileIndex, 0);
        switch (ResourseType)
        {
            case ResourseType.Logs:
                for (int i = 0; i < StorageAmount; i++)
                {
                    PickUp pickup = Instantiate(Tile.LogsGO, transform.position + new Vector3(Random.Range(-3, 3), .3f, Random.Range(-3, 3)), Quaternion.Euler(0, 0, 90)).GetComponent<PickUp>();
                    GameManager.Instance.AddRougue(pickup, true);
                }
                GameManager.Instance.LogHolders.Remove(transform);
                break;
            case ResourseType.Stone:
                for (int i = 0; i < StorageAmount; i++)
                {
                    PickUp pickup = Instantiate(Tile.StoneGO, transform.position + new Vector3(Random.Range(-3, 3), .3f, Random.Range(-3, 3)), Quaternion.Euler(0, 0, 90)).GetComponent<PickUp>();
                    GameManager.Instance.AddRougue(pickup, true);
                }               
                GameManager.Instance.StoneHolders.Remove(transform);
                break;
            case ResourseType.Food:
                GameManager.Instance.Food -= StorageAmount;
                GameManager.Instance.FoodHolders.Remove(transform);
                break;
            case ResourseType.Iron:
                for (int i = 0; i < StorageAmount; i++)
                {
                    PickUp pickup = Instantiate(Tile.IronGO, transform.position + new Vector3(Random.Range(-3, 3), .3f, Random.Range(-3, 3)), Quaternion.Euler(0, 0, 90)).GetComponent<PickUp>();
                    GameManager.Instance.AddRougue(pickup, true);
                }
                GameManager.Instance.IronHolders.Remove(transform);
                break;
        }
        StorageAmount = 0;
        base.SellProperty();
    }
    public void SetFakeAmount(bool JustFood)
    {
        /*
        if (JustFood)
        {
            if(ResourseType == ResourseType.Food)
            {
                FakeAmount = storageAmount;
            }
        }            
        else
            FakeAmount = storageAmount;
        */
    }
    protected override void SetMaterial(Material Mat)
    {
        if (transform.GetComponent<MeshRenderer>() != null)
            transform.GetComponent<MeshRenderer>().material = Mat;
    }
    public override void UpgradeProperty()
    {
        GameManager.Instance.SetFakeAmounts -= SetFakeAmount;
        IsGonaBeFull = false;
        PlayerPrefs.SetInt(PropertyName + Tile.TileIndex, 0);
        switch (ResourseType)
        {
            case ResourseType.Logs:
                for (int i = 0; i < StorageAmount; i++)
                {
                    PickUp pickup = Instantiate(Tile.LogsGO, transform.position + new Vector3(Random.Range(-3, 3), .3f, Random.Range(-3, 3)), Quaternion.Euler(0, 0, 90)).GetComponent<PickUp>();
                    GameManager.Instance.AddRougue(pickup, true);
                }
                GameManager.Instance.LogHolders.Remove(transform);
                break;
            case ResourseType.Stone:
                for (int i = 0; i < StorageAmount; i++)
                {
                    PickUp pickup = Instantiate(Tile.StoneGO, transform.position + new Vector3(Random.Range(-3, 3), .3f, Random.Range(-3, 3)), Quaternion.Euler(0, 0, 90)).GetComponent<PickUp>();
                    GameManager.Instance.AddRougue(pickup, true);
                }
                GameManager.Instance.StoneHolders.Remove(transform);
                break;
            case ResourseType.Food:
                GameManager.Instance.Food -= StorageAmount;
                GameManager.Instance.FoodHolders.Remove(transform);
                break;
            case ResourseType.Iron:
                for (int i = 0; i < StorageAmount; i++)
                {
                    PickUp pickup = Instantiate(Tile.IronGO, transform.position + new Vector3(Random.Range(-3, 3), .3f, Random.Range(-3, 3)), Quaternion.Euler(0, 0, 90)).GetComponent<PickUp>();
                    GameManager.Instance.AddRougue(pickup, true);
                }
                GameManager.Instance.IronHolders.Remove(transform);
                break;
        }
        StorageAmount = 0;
        HasUpgraded = true;
    }
}
