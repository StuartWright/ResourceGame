using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class BaseResourse : MonoBehaviour, IDamagable, IWorkable
{

    public enum resourseType
    {
        NA,
        Tree,
        Stone
    }

    public Task Task;   
    private Animator Anim;
    [SerializeField]private int health;
    public BaseWorker Worker;
    public GameObject Drop;
    private bool HasAssinged;
    public GameObject Canvas;
    public resourseType Type;
    private ResourseType DroppedType;
    public int Health
    {
        get { return health; }
        set
        {
            health = value;
        }
    }
    void Start()
    {
        Anim = GetComponent<Animator>();
        if(Type == resourseType.Tree)
        {
            GameManager.Instance.Trees.Add(this);
            transform.DOScale(new Vector3(1,1,1), 2);
            DroppedType = ResourseType.Logs;
        }
        else if(Type == resourseType.Stone)
        {
            GameManager.Instance.Rocks.Add(this);
            transform.DOScale(new Vector3(1, 1, 1), 2);
            DroppedType = ResourseType.Stone;
        }
    }

    public virtual void TakeDamage(int Damage)
    {
        Health -= Damage;
        if (Anim != null)
            Anim.SetTrigger("TreeShake");
        if(Health <= 0)
        {
            Destroyed();
        }
    }

    public virtual void Destroyed()
    {
        Worker.CurrentTask = null;
        if(Anim != null)
        Anim.SetTrigger("TreeFall");
        PickUp pickup = Instantiate(Drop, transform.position + new Vector3(UnityEngine.Random.Range(-3, 3), .3f, UnityEngine.Random.Range(-3, 3)),  Quaternion.Euler(0,0,90)).GetComponent<PickUp>();
        if (ExtentionMethods.CheckHolders(pickup.ResourseType, Worker, pickup))
            Worker.CurrentTask = pickup.Task;
        else
        {
            GameManager.Instance.AddRougue(pickup, true);
            if (Worker.HasJob)
            {
                if (Worker.IsLogger && GameManager.Instance.Trees.Count > 0)
                {
                    Worker.CurrentTask = GameManager.Instance.Trees[0].Task;
                    GameManager.Instance.Trees.Remove(GameManager.Instance.Trees[0]);
                }
                else
                    Worker.CurrentTask = Worker.JobTask;
            }
        }
        AddResourse(pickup.ResourseType);       
        StartCoroutine(WaitDestroy());
        if(Canvas != null)
        Canvas.SetActive(false);
    }
    
    private void AddResourse(ResourseType type)
    {
        switch (type)
        {
            case ResourseType.Logs:
                GameManager.Instance.Logs++;
                break;
            case ResourseType.Stone:
                GameManager.Instance.Stone++;
                break;
        }
    }
    IEnumerator WaitDestroy()
    {
        yield return new WaitForSeconds(5);
        Destroy(gameObject);
    }
    private void OnMouseDown()
    {
        if (!GameManager.Instance.CanInteract || GameManager.Instance.UIClicked) return;
        if(!HasAssinged)
        {
            if (!ExtentionMethods.CheckHolders(DroppedType, null))
            {
                int amount = 0;
                switch (DroppedType)
                {
                    case ResourseType.Logs:
                        amount = GameManager.Instance.Logs;
                        break;
                    case ResourseType.Stone:
                        amount = GameManager.Instance.Stone;
                        break;
                }
                if (amount >= 5)
                {
                    GameManager.Instance.ErrorMessage("Nowhere to store resource");
                    return;
                }
            }
           
            if (Type == resourseType.Tree)
            {
                if (GameManager.Instance.Trees.Contains(this))
                    GameManager.Instance.Trees.Remove(this);
            }
            else if (Type == resourseType.Stone)
            {
                if (GameManager.Instance.Rocks.Contains(this))
                    GameManager.Instance.Rocks.Remove(this);
            }
            if (!GameManager.Instance.AssignTask(Task))
                GameManager.Instance.QueuedTasks.Add(Task);
            HasAssinged = true;
            GetComponent<BoxCollider>().enabled = false;
            if (Canvas != null)
                Canvas.SetActive(true);
        }      
    }
    public void SetWorker(BaseWorker sender)
    {
        Worker = sender;
    }
}
