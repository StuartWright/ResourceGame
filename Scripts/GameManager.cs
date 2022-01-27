using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using TMPro;
public class GameManager : MonoBehaviour
{
    public delegate void Spawner();
    public event Spawner TreeSpawn;
    public delegate void BuildEvents();
    public event Spawner ShowBoarders, HideBoarders;
    public delegate void StorageEvents(bool JustFood);
    public event StorageEvents SetFakeAmounts;
    public Text LogsText, StoneText, WorkersText, FoodText, HouseText, IronText;
    public Text SchoolName;
    public TextMeshProUGUI PropertyNameText, PropertyLevelText;
    public GameObject WorkerRef;
    public Transform WorkerStartPos;
    public static GameManager Instance;  
    public List<BaseWorker> Workers = new List<BaseWorker>();
    public GameObject Tile, BuyPanel, ProperyPanel, Tree, Rock, SchoolPanel, RotateUI;
    public Button TurnOnOffButton, UpgradeButton;
    private Camera Cam;
    private Tile selectedTile;
    public Tile SelectedTile
    {
        get { return selectedTile; }
        set
        {
            selectedTile = value;           
            if (SelectedTile != null)
            {
                SelectedTile.Renderer.enabled = true;
            }
        }
    }
    public BuyPanelManager BuyPanelManager;/////////////////////////////////////////////
    public List<PickUp> RougueResourses = new List<PickUp>();
    public List<Transform> LogHolders = new List<Transform>();
    public List<Transform> StoneHolders = new List<Transform>();
    public List<Transform> FoodHolders = new List<Transform>();
    public List<Transform> IronHolders = new List<Transform>();
    public List<Task> QueuedTasks = new List<Task>();
    public List<BaseResourse> Trees = new List<BaseResourse>();
    public List<BaseResourse> Rocks = new List<BaseResourse>();
    public List<House> Houses = new List<House>();
    public List<PropertyID> AllIDs = new List<PropertyID>();
    public bool CanInteract = true, Build, UIClicked;
    public GameObject FeedingTimeText;
    public GameObject LogGO, StoneGO, IronGO;
    public string[] TilePropertyName;
    public string[] RougueResourcesSaves;
    public Vector3[] RougueResourcesPositions;
    [HideInInspector]
    public int LogStoreAmount, StoneStoreAmount, FoodStoreAmount, IronStoreAmount;
    private int MaxTreeAmount = 10, MaxRockAmount = 10;
    public Text AllHappiniessText, FPSText;
    public TextMeshProUGUI ErrorText;
    public Slider HappinessBar;
    public Gradient Gradient;
    public Image HappinessBarImg, HappinessImg;
    public Sprite HappyImg, MiddleImg, SadImg;
    public Image UpgradeLogUI, UpgradeStoneUI;
    public TextMeshProUGUI UpgradeLogAmount, UpgradeStoneAmount;
    public void SetHappiniessText()
    {
        int Happy = 0, MaxHappy = 0;
        foreach(BaseWorker worker in Workers)
        {
            Happy += worker.Happy;
            MaxHappy += worker.MaxHappiness;
        }
        //AllHappiniessText.text = Happy + " / " + MaxHappy;
        HappinessBar.value = (float)Happy / (float)MaxHappy;
        HappinessBarImg.color = Gradient.Evaluate(HappinessBar.value);
        HappinessImg.color = Gradient.Evaluate(HappinessBar.value);
        if (HappinessBar.value > .75f)
            HappinessImg.sprite = HappyImg;
        else if(HappinessBar.value < .75f && HappinessBar.value > .35f)
            HappinessImg.sprite = MiddleImg;
        else if (HappinessBar.value < .35f)
            HappinessImg.sprite = SadImg;
    }
    public void AddRougue(PickUp Rougue, bool add)
    {
        if(add)
            RougueResourses.Add(Rougue);
        else
        {
            RougueResourses.Remove(Rougue);
        }           
        RougueResourcesSaves = new string[RougueResourses.Count];
        RougueResourcesPositions = new Vector3[RougueResourses.Count];
        for (int i = 0; i < RougueResourses.Count; i++)
        {
            RougueResourcesSaves[i] = RougueResourses[i].ResourseType.ToString();
            RougueResourcesPositions[i] = RougueResourses[i].transform.position;
        }
        PlayerPrefsX.SetStringArray("RougueResources", RougueResourcesSaves);
        PlayerPrefsX.SetVector3Array("RougueResourcesPositions", RougueResourcesPositions);
    }
    public void LoadRouges()
    {
        if(PlayerPrefs.HasKey("HasSaved"))
        {
            string[] resources = PlayerPrefsX.GetStringArray("RougueResources");
            Vector3[] resourcesPos = PlayerPrefsX.GetVector3Array("RougueResourcesPositions");
            for(int i = 0; i < resources.Length; i++)
            {
                PickUp rougue = null;
                switch (resources[i])
                {
                    case "Logs":
                        rougue = Instantiate(LogGO, resourcesPos[i], Quaternion.Euler(0, 0, 90)).GetComponent<PickUp>();
                        Logs++;
                        AddRougue(rougue, true);
                        break;
                    case "Stone":
                        rougue = Instantiate(StoneGO, resourcesPos[i], Quaternion.Euler(0, 0, 90)).GetComponent<PickUp>();
                        Stone++;
                        AddRougue(rougue, true);
                        break;
                    case "Iron":
                        rougue = Instantiate(IronGO, resourcesPos[i], Quaternion.Euler(0, 0, 0)).GetComponent<PickUp>();
                        Iron++;
                        AddRougue(rougue, true);
                        break;
                }
            }
        }
        int amount = PlayerPrefs.GetInt("Workers");    
        if (amount == 0)
            amount = 4;
        for (int i = 0; i < amount; i++)
        {
            BaseWorker worker = Instantiate(WorkerRef, WorkerStartPos.position, transform.rotation).GetComponent<BaseWorker>();
            worker.Agent.SetDestination(ExtentionMethods.RandomPos());
            PlayerPrefs.SetInt("Workers", Workers.Count);
        }
        SetHappiniessText();
    }
    public void PickUpRouges()
    {
        List<Transform> tempHolder = null;
        SetFakeAmounts?.Invoke(false);
        for (int i = 0; i < RougueResourses.Count; i++)
        {
            bool IsUsed = false;
            foreach (Task obj in QueuedTasks)
            {
                if (obj.Target == RougueResourses[i].transform)
                {
                    IsUsed = true;
                    break;
                }
            }
            if (IsUsed)
                continue;

            switch(RougueResourses[i].ResourseType)
            {
                case ResourseType.Logs:
                    tempHolder = LogHolders;
                    break;
                case ResourseType.Stone:
                    tempHolder = StoneHolders;
                    break;
                case ResourseType.Iron:
                    tempHolder = IronHolders;
                    break;
            }
            if (tempHolder == null)
                return;
            if (tempHolder.Count > 0)
            {
                for (int j = 0; j < tempHolder.Count; j++)
                {
                    Storage holder = tempHolder[j].GetComponent<Storage>();
                    if (holder.FakeAmount < holder.MaxStorageAmount && !holder.IsGonaBeFull)
                    {
                        if (!QueuedTasks.Contains(RougueResourses[i].Task))
                        {
                            holder.FakeAmount++;
                            //holder.SaveResourse(RougueResourses[i]);
                            if (holder.FakeAmount == holder.MaxStorageAmount)
                                holder.IsGonaBeFull = true;
                            RougueResourses[i].Task.StorRef = tempHolder[j].transform;
                            QueuedTasks.Add(RougueResourses[i].Task);
                            break;
                        }
                    }
                }
            }
        }
    }
    public int MaxWorkers = 2;
    private int avaliableWorkers;
    public int AvaliableWorkers
    {
        get { return avaliableWorkers; }
        set
        {
            avaliableWorkers = value;
            WorkersText.text = AvaliableWorkers + "/" + MaxWorkers;
        }
    }
    private int logs;
    public int Logs
    {
        get { return logs; }
        set
        {
            logs = value;
            LogsText.text = "" + Logs;
            //PlayerPrefs.SetInt("Logs", Logs);
        }
    }
    private int stone;
    public int Stone
    {
        get { return stone; }
        set
        {
            stone = value;
            StoneText.text = "" + Stone;
            //PlayerPrefs.SetInt("Stone", Stone);
        }
    }
    private int food;
    public int Food
    {
        get { return food; }
        set
        {
            food = value;
            FoodText.text = "" + Food;
            //PlayerPrefs.SetInt("Food", Food);
        }
    }
    private int iron;
    public int Iron
    {
        get { return iron; }
        set
        {
            iron = value;
            IronText.text = "" + Iron;
            //PlayerPrefs.SetInt("Iron", Iron);
        }
    }
    private int housing;
    public int Housing
    {
        get { return housing; }
        set
        {
            housing = value;
            HouseText.text = "" + Housing;
            //PlayerPrefs.SetInt("Housing", Housing);
        }
    }   
    
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        //Logs = PlayerPrefs.GetInt("Logs");
        //Stone = PlayerPrefs.GetInt("Stone");
        //Food = PlayerPrefs.GetInt("Food");
        //Iron = PlayerPrefs.GetInt("Iron");
        Logs = 0;
        Stone = 0;
        Food = 0;
        Iron = 0;
        Housing = 0;
        AvaliableWorkers = MaxWorkers;

        if (PlayerPrefs.HasKey("HasSaved"))
        {
            TilePropertyName = PlayerPrefsX.GetStringArray("TilePropertyName");
        }
        else
        {
            TilePropertyName = new string[288];
            for (int i = 0; i < TilePropertyName.Length; i++)
                TilePropertyName[i] = "Empty";
            PlayerPrefsX.SetStringArray("TilePropertyName", TilePropertyName);
        }

        
        bool RoadPlaced = false, RocksPlaced = false;
        int RockAmount = 0;
        Cam = Camera.main;
        Tile tile = null;
        int index = 0;
        
        for (int i = 0; i < 100; i += 6)
        {
            for (int j = 0; j < 100; j += 6)
            {
                tile = Instantiate(Tile, new Vector3(i, 0, j), transform.rotation).GetComponent<Tile>();
                tile.TileIndex = index;
                ShowBoarders += tile.ShowBoarder;
                HideBoarders += tile.HideBoarder;
                if (index < 288 && TilePropertyName[index] != "Empty")
                    tile.LoadTile();
                               
                if (!RocksPlaced && !PlayerPrefs.HasKey("HasSaved"))
                {
                    int num = Random.Range(0, 21);
                    if (num == 1)
                    {
                        RockAmount++;
                        if (RockAmount == 2)
                            RocksPlaced = true;
                        tile.Stone.SetActive(true);
                        tile.Type = PropertyTypes.Stone;
                        tile.IsEmpty = false;
                        tile.CantInteract = true;
                        TilePropertyName[index] = tile.Type.ToString();
                        PlayerPrefsX.SetStringArray("TilePropertyName", TilePropertyName);
                    }
                    
                }
                index++;
            }            
        }
        LoadRouges();
        PlayerPrefs.SetString("HasSaved", "");
        /*
        if (!RoadPlaced)
        {
            tile.Road.SetActive(true);
            tile.Type = PropertyTypes.Road;
            tile.IsEmpty = false;
        }
        */     
        StartCoroutine(SpawnTree());
        StartCoroutine(SpawnStone());
    }

    int avgFrameRate;
    /*
    void Update()
    {
        float current = 0;
        current = Time.frameCount / Time.time;
        avgFrameRate = (int)current;
        FPSText.text = avgFrameRate.ToString() + " FPS";
    }
    */
    IEnumerator test()
    {
        yield return new WaitForSeconds(1);
        List<BaseWorker> LeavingWorkers = new List<BaseWorker>();
        Storage Store = null;
        foreach (BaseWorker worker in Workers)
        {            
            Task eatTask = new Task();
            foreach (Transform holder in FoodHolders)
            {
                Storage tempHolder = holder.GetComponent<Storage>();
                //if (tempHolder.StorageAmount > 0)
                if (tempHolder.FakeAmount > 0)
                {
                    tempHolder.FakeAmount--;
                    eatTask.Target = holder;
                    Store = tempHolder;
                    break;
                }                
            }
            yield return new WaitForSeconds(1);
            if (eatTask.Target != null)
            {
                eatTask.InitInteruption = false;
                eatTask.StoppingDistance = 2;
                eatTask.FromHolder = true;
                eatTask.task = Tasks.PickUpFood;
                if (!worker.Busy)
                {                  
                    worker.CurrentTask = eatTask;
                }
                else
                {
                    worker.InteruptionTask = eatTask;
                    if (worker.IsFarmer)
                    {
                        if (worker.FarmRef.ReadyToCollect)
                        {
                            worker.CurrentTask = null;
                            worker.CurrentTask = worker.JobTask;
                            //worker.FarmRef.CheckToCollect(Store);
                        }
                            
                    }
                                             
                }

            }
            else
            {
                worker.Happy--;
                worker.SadImg.DOPlayForward();
                if (worker.IsLeaving)
                {
                    LeavingWorkers.Add(worker);
                    continue;
                }
            }
        }
        foreach (BaseWorker worker in LeavingWorkers)
        {
            Workers.Remove(worker);
        }
        SetHappiniessText();
    }
    public void FeedingTimeBois()
    {
        SetFakeAmounts?.Invoke(true);
        StartCoroutine(test());
        FeedingTimeText.SetActive(true);
        Tween t = FeedingTimeText.transform.DOMoveY(900, 2).OnComplete(() => { FeedingTimeText.SetActive(false); FeedingTimeText.transform.DOMoveY(1000, 2); });
    }

    public void SetFakes(bool justfood) => SetFakeAmounts?.Invoke(justfood);
    public bool CanBuild;
    PropertyID IDRef;
    public bool IsRotating;
    private bool HasMultiBuilt;
    private GameObject RotatingObj;
    private Tile tempTile;
    private Quaternion PreviousRot;
    public void BuySomething(PropertyID ID)
    {
        IDRef = ID;
        tempTile = SelectedTile;               
            
        if (ID.Property.PropertyName.ToString() == "StoneMiner")
        {
            if (!SelectedTile.CheckForStone() || !SelectedTile.CheckForRoad())
            {
                ErrorMessage("Property must be next to a road and stone");
                return;
            }                
            SelectedTile.BuyProperty(ID, false);
            if (CanBuild)
            {
                //BuyPanel.SetActive(false);
                BuyPanelManager.ClosePanel();
                //SelectedTile.Renderer.enabled = false;
                SelectedTile = null;
            }
        }
        else if (SelectedTile.Type == PropertyTypes.Empty)
        {
            if (!SelectedTile.CheckForRoad() && ID.Property.MustBenNextToRoad)
            {
                ErrorMessage("Property must be next to a road");
                return;
            }               
            else if (ID.Property.PropertyName == PropertyTypes.Road && !SelectedTile.CheckForKeep() && !SelectedTile.CheckForRoad())
            {
                ErrorMessage("Property must be next to a road or keep");
                return;
            }       
            else if(ID.Property.IsStorage)
            {
                List<Transform> storage = null;
                int MaxStore;
                string errorMessage = "";
                switch (ID.Property.PropertyName)
                {
                    case PropertyTypes.LogStorage:
                        storage = LogHolders;
                        errorMessage = "Max log storage reached";
                        break;
                    case PropertyTypes.StoneStorage:
                        storage = StoneHolders;
                        errorMessage = "Max stone storage reached";
                        break;
                    case PropertyTypes.FoodStorage:
                        storage = FoodHolders;
                        errorMessage = "Max food storage reached";
                        break;
                    case PropertyTypes.IronStorage:
                        storage = IronHolders;
                        errorMessage = "Max iron storage reached";
                        break;
                }
                if(storage.Count >= Keep.Instance.MaxStorage)
                {
                    ErrorMessage(errorMessage);
                    return;
                }
            }
            CanBuild = true;
            if(ID.Property.CheckResources())
            {
                IsRotating = true;
                SelectedTile.Boarder.SetActive(false);
                RotateUI.SetActive(true);
                
                foreach(GameObject building in SelectedTile.Properties)
                {
                    if(building.name == ID.Property.PropertyName.ToString())
                    {
                        building.SetActive(true);
                        RotatingObj = building;
                        if (HasMultiBuilt)
                        {
                            RotatingObj.transform.rotation = PreviousRot;
                        }                           
                        break;
                    }
                }
                BuyPanelManager.ClosePanel();
            }   
            else
                ErrorMessage("Not enough resources");
        }    
    }
    public void RotateLeft()
    {
        RotatingObj.transform.eulerAngles = new Vector3(0, RotatingObj.transform.eulerAngles.y - 90, 0);
    }
    public void RotateRight()
    {
        RotatingObj.transform.eulerAngles = new Vector3(0, RotatingObj.transform.eulerAngles.y + 90, 0);
    }
    public void FinishedRotating()
    {
        PlayerPrefs.SetFloat("PropertyRotation" + tempTile.TileIndex.ToString(), RotatingObj.transform.eulerAngles.y);
        IsRotating = false;       
        RotatingObj.SetActive(false);
        RotateUI.SetActive(false);
        tempTile.BuyProperty(IDRef, false);
        
        UIUnClick();
        if (IDRef.Property.BuildMultiple && IDRef.Property.CheckResources())
        {
            PreviousRot = RotatingObj.transform.rotation;
            HasMultiBuilt = true;
            tempTile.CheckMultiBuild(IDRef);
        }            
        else
        {
            //SelectedTile.Renderer.enabled = false;
            SelectedTile = null;
            tempTile = null;
        }                
    }
    public void CancelBuild()
    {
        IsRotating = false;
        RotateUI.SetActive(false);
        RotatingObj.SetActive(false);
        tempTile.Boarder.SetActive(true);
    }
    public void SwitchTile(Tile NewTile)
    {
        //SelectedTile.SetFloorMat();
        SelectedTile.Renderer.enabled = false;
        SelectedTile = NewTile;
    }
    public void SellProperty()
    {
        SchoolPanel.SetActive(false);
        SelectedTile.SellProperty();
        ProperyPanel.SetActive(false);
        SelectedTile = null;
    }
    public void Close()
    {
        SchoolPanel.SetActive(false);
        ProperyPanel.SetActive(false);
        if(SelectedTile != null)
        SelectedTile.SetFloorMat();
        SelectedTile = null;
    }
    public bool AssignTask(Task task, Transform Storage = null)
    {
        foreach(BaseWorker worker in Workers)
        {
            if(!worker.Busy && !worker.HasJob)
            {
                if (Storage != null)
                    worker.StoreRef = Storage;
                worker.CurrentTask = task;
                return true;
            }
        }
        return false;
    }
    public void TurnOnOff()
    {
        SelectedTile.TurnOnOff();
    }
    public void UpgradeProperty()
    {
        SelectedTile.UpgradeProperty();
    }
    IEnumerator SpawnTree()
    {
        yield return new WaitForSeconds(10);
        if (Trees.Count < MaxTreeAmount)
        {
            //BaseResourse tree = Instantiate(Tree, ExtentionMethods.RandomPos(), transform.rotation).GetComponent<BaseResourse>();
            Instantiate(Tree, ExtentionMethods.RandomPos(), transform.rotation);
            //Trees.Add(tree);
            TreeSpawn?.Invoke();
        }         
        StartCoroutine(SpawnTree());
    }
    IEnumerator SpawnStone()
    {
        yield return new WaitForSeconds(10);
        if (Rocks.Count < MaxRockAmount)
        {
            Instantiate(Rock, ExtentionMethods.RandomPos(), transform.rotation);
            //Trees.Add(tree);
            //TreeSpawn?.Invoke();
        }
        StartCoroutine(SpawnStone());
    }
    public void SetPropertyUI(BaseProperty property)
    {
        if (property == null)
        {
            ProperyPanel.SetActive(true);////////////////////////////////////////////////////////////////////////////////
            return;
        }           
        if (property.GetComponent<School>())
        {
            SchoolPanel.SetActive(true);
            SchoolName.text = property.PropertyName;
        }           
        else
        {
            ProperyPanel.SetActive(true);
            PropertyNameText.text = property.PropertyName;
            PropertyLevelText.text = "Level "+property.Level;
            UpgradeLogUI.gameObject.SetActive(false);
            UpgradeStoneUI.gameObject.SetActive(false);
            PropertyID propertyID = GetID(property.Tile);
            if (property.MaxLevel == 0 || property.Level == property.MaxLevel || (Keep.Instance.Level <= propertyID.Property.UpgradeAmounts[property.Level-1].RequiredKeepLevel && property.Tile.Type != PropertyTypes.Keep))
            {
                UpgradeButton.interactable = false;
            }               
            else
            {
                UpgradeButton.interactable = true;
                int amount = 0;
                amount = propertyID.Property.UpgradeAmounts[property.Level - 1].LogAmount;
                if (amount > 0)
                {
                    UpgradeLogUI.gameObject.SetActive(true);
                    UpgradeLogAmount.text = amount + "";
                    if (Logs < amount)
                        UpgradeLogAmount.color = Color.red;
                    else
                        UpgradeLogAmount.color = Color.black;
                }
                amount = propertyID.Property.UpgradeAmounts[property.Level - 1].StoneAmount;
                if (amount > 0)
                {
                    UpgradeStoneUI.gameObject.SetActive(true);
                    UpgradeStoneAmount.text = amount + "";
                    if (Stone < amount)
                        UpgradeStoneAmount.color = Color.red;
                    else
                        UpgradeStoneAmount.color = Color.black;
                }
            }               
        }        
    }
    private PropertyID GetID(Tile Property)
    {
        foreach(PropertyID id in AllIDs)
        {
            if (id.Property.PropertyName == Property.Type)
                return id;
        }
        return null;
    }
    public void SpawnPeople()
    {
        if (MaxWorkers >= housing) return;
        //int RandomNum = Random.Range(0,1);
        //if(RandomNum == 0)
       // {
            BaseWorker worker = Instantiate(WorkerRef, WorkerStartPos.position, transform.rotation).GetComponent<BaseWorker>();
            worker.Agent.SetDestination(ExtentionMethods.RandomPos());           
        //}
    }
    public void BuildButton()
    {
        if(!Build)
        {
            Build = true;
            ShowBoarders();
        }
        else
        {
            Build = false;
            BuyPanelManager.ClosePanel();
            HideBoarders();
        }
    }
    public void UIClick()
    {
        UIClicked = true;
    }
    public void UIUnClick()
    {
        UIClicked = false;
    }
    bool Speedd;
    public void SpeedUp()
    {
        if(!Speedd)
        {
            Time.timeScale = 4;
            Speedd = true;
        }
        else
        {
            Time.timeScale = 1;
            Speedd = false;
        }
    }
    public void AssignHome()
    {
        foreach(BaseWorker worker in Workers)
        {
            if (worker.Home != null)
            {
                if(!Houses.Contains(worker.Home))
                {
                    worker.Home = null;
                }
                else
                    continue;
            }                
            foreach(House home in Houses)
            {
                if(home.CurrentAmount < home.HouseSpace)
                {
                    worker.Home = home;
                    home.CurrentAmount++;
                    break;
                }
            }
            if(worker.Home == null)
            {
                worker.Home = null;
            }
        }
    }
    public void BuildMultiple(PropertyID ID, Tile tile)
    {
        SelectedTile = tile;
        BuySomething(ID);
    }

    Tween q;
    public void ErrorMessage(string Message)
    {
        if (q != null)
            q.Kill();
        ErrorText.transform.DOLocalMoveY(50, 0);
        ErrorText.gameObject.SetActive(true);
        ErrorText.text = Message;
        q = ErrorText.transform.DOLocalMoveY(100, 2).OnComplete(() => { ErrorText.gameObject.SetActive(false);});
    }
    public void deletesave()
    {
        PlayerPrefs.DeleteAll();
    }
}
