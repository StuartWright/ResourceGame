 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class BaseNpc : MonoBehaviour, IDamagable
{
    public Task AttackTask;
    public delegate void Attack(BaseNpc enemy);
    public event Attack EnemyDead;
    private Animator Anim;
    private NavMeshAgent Agent;
    bool HasRoamLocation;
    bool HasTarget;
    Collider[] Hit;
    Transform Target;
    Collider MyCollider;
    //protected int layerMask = 1 << 8; //Layer 8
    protected int layerMaskNine = 1 << 9; 
    protected int layerMaskTen = 1 << 10; 
    [SerializeField]
    private int health;
    public int Health
    {
        get { return health; }
        set
        {
            health = value;
            if (Health <= 0)
            {
                EnemyDead?.Invoke(this);
                Destroy(gameObject);
            }
                
        }
    }
    private void Start()
    {
        Anim = GetComponent<Animator>();
        Agent = GetComponent<NavMeshAgent>();
        MyCollider = GetComponent<BoxCollider>();
    }

    void LateUpdate()
    {
        if(!HasRoamLocation)
        {
            Roaming();
        }
        if(!HasTarget)
        {
            Hit = Physics.OverlapSphere(transform.position, 20);
            foreach (Collider target in Hit)
            {              
                if (target.gameObject.layer == 9 || target.gameObject.layer == 10)
                {
                    if (target.gameObject != this.gameObject)
                    {
                        if(target.GetComponent<BaseWorker>())
                        {
                            MyCollider.enabled = false;
                            //Debug.DrawRay(transform.position + new Vector3(0, 1, 0), (target.transform.position - transform.position) * 20, Color.red);
                            RaycastHit hit;
                            if (Physics.Raycast(transform.position + new Vector3(0,1,0), (target.transform.position - transform.position), out hit, 20))
                            {
                                
                                if(hit.collider == target)
                                {
                                    BaseWorker worker = target.GetComponent<BaseWorker>();
                                    Target = target.transform;
                                    EnemyDead += Target.GetComponent<BaseWorker>().EnemyDied;

                                    if (worker.CurrentTask != null && worker.CurrentTask.task == Tasks.None)
                                        worker.CurrentTask = AttackTask;
                                    else
                                        worker.InteruptionTask = AttackTask;
                                    worker.EnemyDead += EnemyDied;
                                    Agent.destination = Target.position;
                                    HasTarget = true;
                                    MyCollider.enabled = true;
                                    break;
                                }                               
                            }                            
                        }
                        /*
                        else if(target.GetComponent<BaseProperty>())
                        {
                            BaseProperty property = target.GetComponent<BaseProperty>();
                            property.Destroyed += PropertyDestroyed;
                            Target = target.transform;
                            Agent.destination = Target.position;
                            HasTarget = true;
                            break;
                        }
                        */
                    }
                }               
            }
        }
        Anim.SetFloat("Speed", Agent.velocity.magnitude / Agent.speed);
        if (Agent.remainingDistance <= Agent.stoppingDistance)
        {
            Agent.velocity /= 2;           
            if(Target != null)
            {
                Anim.SetBool("Attacking", true);
            }
            else
            {
                HasRoamLocation = false;
                HasTarget = false;
                Anim.SetBool("Attacking", false);
            }
        }
        else if(Target != null)
            Agent.destination = Target.position;
        //Anim.SetFloat("Speed", Agent.velocity.magnitude / Agent.speed);
    }
    private void Roaming()
    {
        Agent.destination = ExtentionMethods.RandomPos();
        HasRoamLocation = true;
    }
    public void EnemyDied(BaseWorker enemy)
    {
        enemy.EnemyDead -= EnemyDied;
        Target = null;
    }
    public void PropertyDestroyed(BaseProperty property)
    {
        property.Destroyed -= PropertyDestroyed;
        Target = null;
    }
    public void TakeDamage(int Damage)
    {
        Health -= Damage;
    }
    public void DealDamage()
    {
        Target.GetComponent<IDamagable>().TakeDamage(1);
    }
}
