using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
public enum Tasks
{ 
    None,
    ChopWood,
    PickUp,
    DropOffResource,
    Mining,
    PickUpFood,
    GoTime,
    Plow,
    PutOutFire
}
public class BaseWorker : MonoBehaviour, IDamagable
{
    public delegate void Attack(BaseWorker npc);
    public event Attack EnemyDead;
    public NavMeshAgent Agent;
    public Transform Target, StoreRef;
    private Animator Anim;
    private string PreviousAnim;
    public Tasks TaskToComplete;
    public bool Busy, Wait, HasJob, Idle, IsLogger, IsFarmer, CanBeInterupted = true, StopUpdate, IsLeaving, IsPuttingOutFire, IsCarrying;
    private bool Dropping;
    public Farm FarmRef;
    public Transform HoldPoint;
    public Task StoreResourse, JobTask;
    public Task InteruptionTask, TaskBeforeInteruption;
    private GameObject CurrentHeldResourse;
    public GameObject Axe, PickAxe, Plow;
    private GameManager GM;
    public DOTweenAnimation HappyImg, SadImg, AttackedImg, NoHouseImg;
    public int MaxHappiness = 10;
    [SerializeField] House home;
    public House Home
    {
        get { return home; }
        set
        {
            home = value;
            if (Home == null)
            {
                NoHouseImg.gameObject.SetActive(true);
                NoHouseImg.DORewindAndPlayNext();
            }
             else
                NoHouseImg.gameObject.SetActive(false);
        }
    }
    [SerializeField] int happy = 10;
    public int Happy
    {
        get { return happy; }
        set
        {
            happy = value;
            if (IsLeaving) return;
            if (happy > MaxHappiness)
                happy = MaxHappiness;
            if (happy <= 0)
            {
                Task ImaLeaveTask = new Task();
                ImaLeaveTask.Target = GM.WorkerStartPos;
                ImaLeaveTask.task = Tasks.GoTime;
                ImaLeaveTask.LeavingTask = true;
                ImaLeaveTask.StoppingDistance = 2;
                GM.MaxWorkers--;
                
                if (!Busy)
                    CurrentTask = ImaLeaveTask;
                else
                {
                    InteruptionTask = ImaLeaveTask;
                }
                if(HasJob)
                {
                    JobTask.Property.CloseProperty();
                }               
                /*
                CurrentTask = null;
                Busy = true;
                GM.AvaliableWorkers--;
                GM.MaxWorkers--;
                GM.Workers.Remove(this);
                IsLeaving = true;
                Agent.stoppingDistance = 2;
                StartCoroutine(test());
                Agent.destination = GM.WorkerStartPos.position;
                */
            }

            GM.SetHappiniessText();
        }
    }
    public GameObject GraveStone;
    [SerializeField]
    private int health;
    public int Health
    {
        get { return health; }
        set
        {
            health = value;
            if(Health <= 0)
            {
                GM.MaxWorkers--;
                if (HasJob)
                {
                    JobTask.Property.CloseProperty();
                }
                
                EnemyDead?.Invoke(this);
                GM.Workers.Remove(this);
                Instantiate(GraveStone, transform.position, transform.rotation);
                GM.SetHappiniessText();
                Destroy(gameObject);
            }
        }
    }
    [SerializeField] private Task currentTask;
    public Task CurrentTask
    {
        get { return currentTask; }
        set
        {          
            currentTask = value;           
            Busy = true;
            StopUpdate = false;
            if (CurrentTask == null)
            {
                
                GM.AvaliableWorkers++;
                if(IsCarrying)
                {
                    Task dropObject = new Task();
                    dropObject.task = Tasks.DropOffResource;
                    dropObject.Target = transform;
                    Wait = true;
                    CurrentTask = dropObject;
                    Dropping = true;
                    return;
                }
                Dropping = false;
                if(SetAnim() != "")
                Anim.SetBool(SetAnim(false), false);
                CanBeInterupted = true;
                TaskToComplete = Tasks.None;
                Target = null;
                Busy = false;
                Wait = true;
                /*
                if (FeedingTime && !FoodInit)
                {
                    FoodInit = true;
                    GoEat();
                }
                */
                return;
            }
            if (CurrentTask.FireTask)
                IsPuttingOutFire = true;
            if (CanBeInterupted && InteruptionTask != null && InteruptionTask.task != Tasks.None && !InteruptionTask.InitInteruption && !IsCarrying)
            {
                InteruptionTask.InitInteruption = true;
                TaskBeforeInteruption = CurrentTask;
                CurrentTask = null;/////////////////////////////////////
                CurrentTask = InteruptionTask;
                InteruptionTask = null;
            }
            if (CurrentTask.buildingToBuild != null && !CurrentTask.buildingToBuild.InitiallyBuilt)
            {
                CurrentTask.buildingToBuild.Worker = this;
                CurrentTask.buildingToBuild.InitiallyBuilt = true;
                if(!CurrentTask.buildingToBuild.CheckResourses())
                {
                    GM.QueuedTasks.Add(CurrentTask);
                    CurrentTask.buildingToBuild.InitiallyBuilt = false;
                    CurrentTask = null;
                    return;
                }
            }
            if(CurrentTask.Property != null && !IsPuttingOutFire)
            {
                CurrentTask.Property.Worker = this;
                HasJob = true;
                JobTask = CurrentTask.Property.PropertyTask;
                if (CurrentTask.Property.IsLoggery)
                {
                    IsLogger = true;
                    Busy = false;
                    //Idle = true;
                }    
                if(!CurrentTask.Property.Init)
                {
                    CurrentTask.Property.HasActivated();
                }
            }
            if (CurrentTask.LeavingTask)
            {
                IsLeaving = true;
            }
            StartCoroutine(test());
            GM.AvaliableWorkers--;
            TaskToComplete = CurrentTask.task;
            Target = currentTask.Target;
            
            if (Target.GetComponent<IWorkable>() != null)
                Target.GetComponent<IWorkable>().SetWorker(this);
            Agent.stoppingDistance = CurrentTask.StoppingDistance;
            Agent.destination = Target.position;
            
          }
    }
    public void PutOutFire()
    {
        IsPuttingOutFire = false;
        CurrentTask.Property.PutOutFire();
        CurrentTask = null;
    }
    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Anim = GetComponent<Animator>();
        GM = GameManager.Instance;
        GM.Workers.Add(this);
        //PlayerPrefs.SetInt("WorkerAmount", GM.Workers.Count - 2);
        GM.MaxWorkers++;
        GM.AvaliableWorkers++;
        PlayerPrefs.SetInt("Workers", GM.Workers.Count);
    }
    void Start()
    {     
        StartCoroutine(CheckForTasks());
        GM.AssignHome();
    }

    private string SetAnim(bool ShowTool = true)
    {
        //Anim.SetBool("Walking", false);
        switch (TaskToComplete)
        {
            case Tasks.ChopWood:
                if (ShowTool)
                    Axe.SetActive(true);
                else
                    Axe.SetActive(false);
                return "ChopWood";
            case Tasks.PickUp:
                return "PickUp";
            case Tasks.DropOffResource:
                return "DropOffResource";
            case Tasks.Mining:
                if (ShowTool)
                    PickAxe.SetActive(true);
                else
                    PickAxe.SetActive(false);
                return "Mining";
            case Tasks.PickUpFood:
                return "PickUpFood";
            case Tasks.Plow:
                if (ShowTool)
                    Plow.SetActive(true);
                else
                    Plow.SetActive(false);
                return "Plow";
            case Tasks.PutOutFire:
                return "PutOutFire";
            default:
                return "";
        }   
    }
   
    void LateUpdate()
    {
        //if (Idle) return;
        Anim.SetFloat("Speed", Agent.velocity.magnitude / Agent.speed);
        if (!Busy || Wait || StopUpdate) return;
        if (Agent.remainingDistance <= Agent.stoppingDistance)
        {
            if (IsLeaving)
                Destroy(gameObject);
            if(SetAnim() != "")
            {
                StopUpdate = true;
                Agent.velocity /= 2;
                Anim.SetBool(SetAnim(), true);
                CanBeInterupted = false;
            }           
        }
        else if(Target != null)
        {
            Agent.destination = Target.position;
        }
           // Anim.SetFloat("Speed", Agent.velocity.magnitude / Agent.speed);
        //Anim.SetBool("Walking", true);
    }

    public void TakeDamage(int Damage)
    {
        Health -= Damage;
        AttackedImg.DOPlayForward();
    }
    public void DealDamage()
    {
        Target.GetComponent<IDamagable>().TakeDamage(1);
    }
    public void PickUpResourse()
    {
        if (CurrentTask == null) return;       
        if (currentTask.FromHolder)
        {
            // Storage storage = CurrentTask.Target.GetComponent<Storage>();// target is null
            if(StoreRef == null)
                print("storefre = null");
            if (StoreRef.GetComponent<Storage>() == null)
                print("store ref com == null");

            Storage storage = StoreRef.GetComponent<Storage>();
            if (CurrentTask == null)
                print("currenttask == null");   
            
           // if (CurrentTask.Target == null)
               // print("currenttask.target == null");//

            if (CurrentTask.Target.GetComponent<Storage>() == null)
                print("currenttask.target.com == null");

            CurrentTask.Target = CurrentTask.Target.GetComponent<Storage>().SpawnResourse();
            storage.RemoveResourse();

        }
        else if (IsFarmer)///////////////////////////////////////////////////////////////////
        {
            //JobTask.Property.CollectedCrops();
            CurrentTask.Target = JobTask.Property.CollectedCrops();
            GM.Food++;

        }
        CurrentTask.Target.transform.position = HoldPoint.position;
        CurrentTask.Target.transform.rotation = HoldPoint.rotation;
        CurrentTask.Target.transform.parent = HoldPoint;
        CurrentHeldResourse = CurrentTask.Target.gameObject;
        if (IsFarmer)
            CurrentHeldResourse.transform.rotation = Quaternion.Euler(0,-55,0);
        if (CurrentTask.Target.GetComponent<PickUp>() == null)
            print("noooooooooooooo2");
        ResourseType pickUp = CurrentTask.Target.GetComponent<PickUp>().ResourseType;
        if(pickUp == ResourseType.Iron)
            CurrentHeldResourse.transform.rotation = Quaternion.Euler(0, 90, 0);
        StoreResourse = new Task();
        if (!CurrentTask.IsBuilding)
        {
            if (StoreRef == null)
                StoreRef = CurrentTask.StorRef;
            CurrentTask = null;          
            if (StoreRef == null)
                ExtentionMethods.CheckHolders(StoreResourse.ResourseType, this);
            StoreResourse.Target = StoreRef;
            if (IsFarmer)
            {
                StoreRef.GetComponent<Storage>().SaveResourse(CurrentHeldResourse.GetComponent<PickUp>());
                //FarmRef.CheckToCollect();
            }
                
            //StoreRef.GetComponent<Storage>().StorageAmount++;////////////////////////////////////////////////////////////////////////////////////////////

            /*
            switch (pickUp)
            {
                case ResourseType.Logs:

                    for (int i = 0; i < GameManager.Instance.LogHolders.Count; i++)
                    {
                        if (!GameManager.Instance.LogHolders[i].GetComponent<Storage>().IsFull)
                        {
                            GameManager.Instance.LogHolders[i].GetComponent<Storage>().StorageAmount++;
                            StoreResourse.Target = GameManager.Instance.LogHolders[i];
                            break;
                        }
                    }
                    break;
                case ResourseType.Stone:
       
                    for (int i = 0; i < GameManager.Instance.StoneHolders.Count; i++)
                    {
                        if (!GameManager.Instance.StoneHolders[i].GetComponent<Storage>().IsFull)
                        {
                            GameManager.Instance.StoneHolders[i].GetComponent<Storage>().StorageAmount++;
                            StoreResourse.Target = GameManager.Instance.StoneHolders[i];
                            break;
                        }
                    }
                    break;
                case ResourseType.Food:
      
                    for (int i = 0; i < GameManager.Instance.FoodHolders.Count; i++)
                    {
                        if (!GameManager.Instance.FoodHolders[i].GetComponent<Storage>().IsFull)
                        {
                            GameManager.Instance.FoodHolders[i].GetComponent<Storage>().StorageAmount++;
                            StoreResourse.Target = GameManager.Instance.FoodHolders[i];
                            break;
                        }
                    }
                    break;
            }
            */
            StoreResourse.StoppingDistance = 2;
        }
        else
        {
            StoreResourse.Target = CurrentTask.buildingToBuild.transform;
            StoreResourse.StoppingDistance = 4;
            CurrentTask = null;
        }
        IsCarrying = true;
        StoreResourse.task = Tasks.DropOffResource;         
        CurrentTask = StoreResourse;



    }
    public void GiveResourse()
    {
        if(CurrentTask.Target.GetComponent<Storage>())
        {
            if(CurrentTask.Target.GetComponent<Storage>().IsFull && CurrentTask.Target.GetComponent<Storage>().ResourseType != ResourseType.Food)
            {
                GM.AddRougue(CurrentHeldResourse.GetComponent<PickUp>(), true);
                Dropping = true;
            }
        }
        if (Dropping)
        {
            //Destroy(CurrentHeldResourse);
            CurrentHeldResourse.transform.parent = null;
            CurrentHeldResourse.transform.position = new Vector3(transform.position.x, .3f, transform.position.z);
            IsCarrying = false;
            CurrentTask = null;
            /*
            if(FeedingTime)
            {
                GoEat();
            }
            */
            return;
        }
        IsCarrying = false;
        if(CurrentTask.Target.GetComponent<IDropOff>() != null)
        CurrentTask.Target.GetComponent<IDropOff>().RecieveResourse(this);
        //CurrentTask = null;
        Destroy(CurrentHeldResourse);
        //if (FeedingTime) Busy = false;
        if(HasJob && !Busy)
        {
            //if(IsLogger && JobTask.Property.Init && GameManager.Instance.Trees.Count > 0)
            if(IsLogger && GameManager.Instance.Trees.Count > 0)
            {
                if (GameManager.Instance.Trees[0] == null)
                    print("BAD TREE");
                CurrentTask = GameManager.Instance.Trees[0].Task;
                GameManager.Instance.Trees.Remove(GameManager.Instance.Trees[0]);                
            }
            else
            {
                CurrentTask = JobTask;
            }
            
        }
    }
    IEnumerator test()
    {
        Wait = true;
        yield return new WaitForSeconds(1);
        Wait = false;
    }
    public IEnumerator CheckForTasks()
    {
        if (IsLeaving)
             yield return 0;
        yield return new WaitForSeconds(1);
        /*
        if (InteruptionTask != null && InteruptionTask.task != Tasks.None && !InteruptionTask.InitInteruption)
        {
            CurrentTask = InteruptionTask;
        }
        */
        if (GM.QueuedTasks.Count > 0 && !Busy)
        {           
            for(int i = 0; i < GM.QueuedTasks.Count; i++)
            {
                if (GM.QueuedTasks[i].task == Tasks.PickUp)
                {
                    
                    if(GM.QueuedTasks[i].CanActivatePickupTask(this))
                    {
                        CurrentTask = GM.QueuedTasks[i];
                        GM.QueuedTasks.Remove(GM.QueuedTasks[i]);
                        break;
                    }
                    /*
                    if (GM.RougueResourses.Count == 0 && !ExtentionMethods.CheckEmpty(GM.QueuedTasks[i].ResourseType, this))
                    {
                        CurrentTask = GM.QueuedTasks[i];
                        if (CurrentTask.StorRef != null)
                            StoreRef = CurrentTask.StorRef;
                        GM.QueuedTasks.Remove(GM.QueuedTasks[i]);
                        break;
                    }
                    else
                    {
                        for(int j = 0; j < GM.RougueResourses.Count; j++)
                        {
                            if (GM.RougueResourses.Count > 0 && ExtentionMethods.CheckHolders(GM.RougueResourses[j].ResourseType, this))
                            {
                                CurrentTask = GM.QueuedTasks[i];
                                StoreRef = CurrentTask.StorRef;
                                if (StoreRef != null)
                                    StoreRef.GetComponent<Storage>().SaveResourse(GM.RougueResourses[j]);
                                GM.QueuedTasks.Remove(GM.QueuedTasks[i]);
                                GM.AddRougue(CurrentTask.Target.GetComponent<PickUp>(), false);
                                BreakOut = true;
                                break;
                            }
                        }
                        if (BreakOut)
                            break;
                    }                                     
                        continue;
                    */
                }
                else
                {
                    CurrentTask = GM.QueuedTasks[i];
                    GM.QueuedTasks.Remove(GM.QueuedTasks[i]);
                    break;
                }
            }
            
            //CurrentTask = GM.QueuedTasks[0];
            //GM.QueuedTasks.Remove(GM.QueuedTasks[0]);
            //GM.RougueResourses.Remove(GM.RougueResourses[0]);


        }
        if (!HasJob)
        StartCoroutine(CheckForTasks());
    }
    public void CheckAgain()
    {
        if(this)
        StartCoroutine(CheckForTasks());
    }
    public void CropsGrown()
    {
        //if(ExtentionMethods.CheckHolders(ResourseType.Food, this))
        //{
             CurrentTask = null;
            Task task = new Task();
            task.task = Tasks.PickUp;
            task.Target = JobTask.Target;
            CurrentTask = task;
        //}
        
    }
    public void FoodPickup()
    {
        Anim.SetBool(SetAnim(), false);
        Storage storage = CurrentTask.Target.GetComponent<Storage>();
        storage.RemoveResourse();
        GM.Food--;
        Happy++;
        HappyImg.DOPlayForward();
        InteruptionTask = null;
        CurrentTask = null;
        if(TaskBeforeInteruption.Target != null)
        CurrentTask = TaskBeforeInteruption;
        if (IsFarmer)
            FarmRef.CheckToCollect();
    }
    public void EnemyDied(BaseNpc enemy)
    {
        enemy.EnemyDead -= EnemyDied;
        InteruptionTask = null;
        CurrentTask = null;
        if (TaskBeforeInteruption.Target != null)
            CurrentTask = TaskBeforeInteruption;
    }
}
