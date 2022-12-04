using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public bool IsAlive
    {
        get
        {
            return State != EnemyState.Dead;
        }
    }

    public EnemyState State
    {
        private set;
        get;
    }

    public float Speed
    {
        get
        {
            return (this.Data.Type == EnemyType.SimpleEnemy ? Database.GameConfiguration.EnemySpeedSimple:Database.GameConfiguration.EnemySpeedGiant);
        }
    }
    
    public EnemyData Data;
    [SerializeField] private TextMeshPro healthText;
    //[SerializeField] private SkinnedMeshRenderer renderer;
    [SerializeField] private Animator animator;
    
    private NavMeshAgent navmeshAgent;
    private CapsuleCollider collider;
    private SoldierCell attackingCell;
    private bool enteredSoldierArea;
    private bool InMove;
    private bool destinationIsBase = false;
    private float lastTimeAttacked;
    public delegate void DiengInside(Enemy enemy);
    public DiengInside OnDiedInside;

    private bool inited = false;
    
    void OnEnable()
    {
        inited = false;
        GameManager.Instance.OnEnemyReachedSildierbase += GoIdle;
        float randomTime = Random.Range(2, 5);
        InvokeRepeating(nameof(Shout), 0, randomTime);
    }
    
    void OnDisable()
    {
        inited = false;
        GameManager.Instance.OnEnemyReachedSildierbase -= GoIdle;
        CancelInvoke(nameof(Shout));
    }

    public void Init(EnemyData data)
    {
        enteredSoldierArea = false;
        navmeshAgent = GetComponent<NavMeshAgent>();
        collider = GetComponent<CapsuleCollider>();
        this.Data.Copy(data);
        healthText.text = Data.Health.ToString();
        healthText.color = Color.red;
        SetState(EnemyState.Running);
        transform.eulerAngles = new Vector3(0f, 180f, 0f);
        //renderer.materials[0].color = Data.GetColor();
        navmeshAgent.speed = Speed;
        SetDestination(new Vector3(Data.X, 0f, 0f));
        inited = true;
    }
    
    void Update()
    {     
        if (!IsAlive || !inited)
            return;

        if (Vector3.Distance(navmeshAgent.destination, transform.position) > navmeshAgent.stoppingDistance)
        {
            InMove = true;
        }
        else
        {
            ArrivedAtDestination();
        }

        if (State == EnemyState.Attacking && (Time.timeSinceLevelLoad-lastTimeAttacked>Database.GameConfiguration.EnemyAttackRate))
        {
            AttackCell();
            CheckCellAttackResult();
            lastTimeAttacked = Time.timeSinceLevelLoad;
        }
    }

    public void SetDestination(Vector3 pos)
    {
        pos = pos + (Vector3.forward*0.5f);
        navmeshAgent.SetDestination(pos);
    }

    public void TakeDamage(float damage)
    {
        Data.Health -= damage;
        healthText.text = Data.Health.ToString();
        if (Data.Health <= 0)
        {
            SetState(EnemyState.Dead);
        }
    }

    public void SetState(EnemyState state)
    {
        State = state;

        switch (State) 
        {
            case EnemyState.Idle:
                collider.isTrigger = false;
                navmeshAgent.enabled = true;
                animator.SetBool("walk", false);
                animator.SetBool("attack", false);
                animator.speed = 1f;
                break;
            case EnemyState.Running:
                collider.isTrigger = false;
                navmeshAgent.enabled = true;
                animator.SetBool("walk", true);
                animator.SetBool("attack", false);
                animator.speed = Speed / Database.GameConfiguration.EnemyDefaultAnimationSpeed;
                break;
            case EnemyState.Attacking:
                collider.isTrigger = false;
                navmeshAgent.enabled = true;
                animator.SetBool("walk", false);
                animator.SetBool("attack", true);
                animator.speed = Speed / Database.GameConfiguration.EnemyDefaultAnimationSpeed;
                lastTimeAttacked = Time.timeSinceLevelLoad;
                break;
            case EnemyState.Dead:
                GameManager.Instance.CurrentKills++;
                UIManager.Instance.UpdateKillCount();
                //OnDiedInside?.Invoke(this);
                if (GameManager.Instance.outsideEnemies.Contains(this))
                    GameManager.Instance.outsideEnemies.Remove(this);
                else
                {
                    GameManager.Instance.insideEnemies.Remove(this);
                    GameManager.Instance.CheckIfInsideZombiesDied();
                }
                collider.isTrigger = true;
                navmeshAgent.enabled = false;
                animator.SetBool("walk", false);
                animator.SetBool("attack", false);
                animator.speed = 1f;
                ParticleManager.Instance.PlayParticle(Data.Type==EnemyType.SimpleEnemy ?Particle_Type.SimpleZDeath:Particle_Type.GiantZDeath, transform.position, Vector3.up);
                ObjectPool.DeSpawn(gameObject);
                GameManager.Instance.CheckIfAllZombiesDied();
                if (Data.Type == EnemyType.SimpleEnemy)
                    SoundManager.Instance.Play(Sound.SimpleZDeath);
                else
                    SoundManager.Instance.Play(Sound.GiantZDeath);
                break;
        }
    }

    private void GoAttack()
    {
        if (!GameManager.Instance.IsInPlayMode)
            return;
        
        SetState(EnemyState.Running);
        attackingCell = SoldierCellMergeManager.Instance.GetFirstCellInTheWay(this);
        if (attackingCell != null)
        {
            SetDestination(attackingCell.transform.position);
        }
        else
        {
            destinationIsBase = true;
            Vector3 basePosition = SoldierCellMergeManager.Instance.GetEnemyBaseInTheWay(this);
            SetDestination(basePosition);
        }
    }

    private void ArrivedAtDestination()
    {
        if (!InMove || !GameManager.Instance.IsInPlayMode)
            return;
        
        InMove = false;
        
        if (!enteredSoldierArea)
        {
            enteredSoldierArea = true;
            GameManager.Instance.insideEnemies.Add(this);
            GameManager.Instance.outsideEnemies.Remove(this);
            GoAttack();
        }
        else
        {
            StartCoroutine(SetStateToAttack());
        }
    }

    private IEnumerator SetStateToAttack()
    {
        float idleTime = 0f;
        while (attackingCell!=null && !attackingCell.IsFull && SoldierCellMergeManager.Instance.IsShifting && idleTime<3f)
        {
            SetState(EnemyState.Idle);
            idleTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        if (State != EnemyState.Attacking)
        {
            SetState(EnemyState.Attacking);
        }
    }

    public void AttackCell()
    {
        if (!GameManager.Instance.IsInPlayMode)
            return;
        
        if (destinationIsBase)
        {
            GameManager.Instance.OnEnemyReachedSildierbase.Invoke();
            VibrationManager.Instance.DoHeavyVibration();
        }
        else if (attackingCell != null && !InMove)
        {
             if (attackingCell.IsFull)
            {
                attackingCell.GettingHit();
                VibrationManager.Instance.DoMediumVibration();
            }
        }
    }
    
    public void CheckCellAttackResult()
    {
        if (attackingCell != null && !InMove)
        {
            if (!attackingCell.IsFull)
            {
                GoAttack();
            }
        } else if (attackingCell == null)
        {
            GoIdle();
        }
    }

    private void GoIdle()
    {
        SetState(EnemyState.Idle);
    }

    private void Shout()
    {
        if (Data.Type == EnemyType.SimpleEnemy)
            SoundManager.Instance.Play(Sound.SimpleZVoice, 0.8f);
        else
            SoundManager.Instance.Play(Sound.GiantZVoice, 0.8f);
    }
}
