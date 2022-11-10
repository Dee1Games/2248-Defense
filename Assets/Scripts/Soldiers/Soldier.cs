using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

public class Soldier : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer thisMeshRenderer;
    [SerializeField] private Animator thisAnimator;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private Transform bulletExitPoint, modelTransform;
    private NavMeshAgent navmeshAgent;
    private CapsuleCollider collider;    
    private ObjectPool bulletPool;
    private Enemy bomberTarget;
    private Enemy shooterTarget;
    
    private int valueNumber;
    
    private Color soldierColor;

    [HideInInspector] public Vector3 shootTarget;
    [HideInInspector] public int curCol, curRow;

    public bool IsShooter;
    public SoldierType Type;

    public SoldierState State
    {
        set
        {
            soldierState = value;
        }
        get
        {
            return soldierState;
        }
    }

    private SoldierState soldierState;

    public int ValueNumber
    {
        set {
            if (Type == SoldierType.Normal && value>0)
            {
                valueText.text = Utils.GetShortenNumText(value);
                valueNumber = value;
                SoldierColor = Utils.GetColorByNumber(ValueNumber);
            }
        }
        get { return valueNumber; }
    }

    public Color SoldierColor
    {
        set {
            soldierColor = value;
            thisMeshRenderer.materials[0].color = value;
            thisMeshRenderer.materials[1].color = value;
        }
        get { return soldierColor; }
    }

    private SoldierCell cell;

    private void OnEnable()
    {
        GameManager.Instance.OnNewEnemySpawned += CheckShootingCondition;
        GameManager.Instance.OnEnemyEntered += CheckShootingCondition;
        bulletPool = GameObject.Find("BulletPool").GetComponent<ObjectPool>();
        navmeshAgent = GetComponent<NavMeshAgent>();
        collider = GetComponent<CapsuleCollider>();
    }
    
    private void OnDisable()
    {
        GameManager.Instance.OnNewEnemySpawned -= CheckShootingCondition;
        GameManager.Instance.OnEnemyEntered -= CheckShootingCondition;
    }

    public void Init()
    {
        ObjectPoolRefrence poolRefrence = GetComponent<ObjectPoolRefrence>();
        if (poolRefrence == null)
        {
            poolRefrence = gameObject.AddComponent<ObjectPoolRefrence>();
        }
        poolRefrence.pool = ((Type == SoldierType.Normal)
            ? SoldierCellMergeManager.Instance.NormalSoldierPool
            : SoldierCellMergeManager.Instance.BomberSoldierPool);

        if (Type == SoldierType.Bomber)
        {
            valueText.text = "";
            valueNumber = 0;
            SoldierColor = Database.GameConfiguration.BomberColor;
        }
        navmeshAgent.speed = (Type == SoldierType.Normal ? Database.GameConfiguration.SoldierSpeedNormal:Database.GameConfiguration.SoldierSpeedBomber);

        if (Type == SoldierType.Normal)
        {
            SetState(SoldierState.Shooting);
            //SetState(IsShooter?SoldierState.Shooting:SoldierState.Idle);
        }
        else
        {
            SetState(SoldierState.Idle);
        }
    }

    public void SetCell(SoldierCell cell)
    {
        this.cell = cell;
    }

    void Update()
    {
        if (Type == SoldierType.Bomber && State == SoldierState.Bombing)
        {
            if (bomberTarget != null && bomberTarget.IsAlive)
            {
                if (navmeshAgent.remainingDistance > Database.GameConfiguration.MinDistanceToExplode)
                {
                    SetDestination(bomberTarget.transform.position);
                }
                else
                {
                    Explode();
                }
            }
            else
            {
                if (bomberTarget == null || !bomberTarget.IsAlive) {
                    bomberTarget = GameManager.Instance.GetClosestEnemyTo(transform.position);
                    SetDestination(bomberTarget.transform.position);
                }
                
                if (bomberTarget == null || !bomberTarget.IsAlive) {
                    Explode();
                }
            }
        }
    }

    public void GoToAttackState()
    {
        if (Type == SoldierType.Normal)
        {
            SetState(SoldierState.Shooting);
        }
        else
        {
            GoBomb();
        }
    }

    public void SetState(SoldierState state)
    {
        State = state;
        switch (State)
        {
            case SoldierState.Idle:
                collider.isTrigger = false;
                navmeshAgent.enabled = false;
                thisAnimator.SetBool("walk", false);
                if(Type==SoldierType.Normal)
                    thisAnimator.SetBool("shoot", false);
                else
                    thisAnimator.SetBool("attack", false);
                transform.rotation = Quaternion.LookRotation(Vector3.forward);
                break;
            case SoldierState.Shooting:
                collider.isTrigger = false;
                navmeshAgent.enabled = false;
                thisAnimator.SetBool("walk", false);
                if(Type==SoldierType.Normal)
                    thisAnimator.SetBool("shoot", true);
                else
                    thisAnimator.SetBool("attack", false);
                break;
            case SoldierState.Running:
                collider.isTrigger = false;
                navmeshAgent.enabled = false;
                thisAnimator.SetBool("walk", true);
                if(Type==SoldierType.Normal)
                    thisAnimator.SetBool("shoot", false);
                else
                    thisAnimator.SetBool("attack", false);
                transform.rotation = Quaternion.LookRotation(Vector3.forward);
                break;
            case SoldierState.Bombing:
                collider.isTrigger = false;
                navmeshAgent.enabled = true;
                thisAnimator.SetBool("walk", false);
                if(Type==SoldierType.Normal)
                    thisAnimator.SetBool("shoot", false);
                else
                    thisAnimator.SetBool("attack", true);
                transform.rotation = Quaternion.LookRotation(Vector3.forward);
                break;
            case SoldierState.Dead:
                collider.isTrigger = true;
                navmeshAgent.enabled = false;
                thisAnimator.SetBool("walk", false);
                if(Type==SoldierType.Normal)
                    thisAnimator.SetBool("shoot", false);
                else
                    thisAnimator.SetBool("attack", false);
                transform.rotation = Quaternion.LookRotation(Vector3.forward);
                cell.ClearCell();
                break;
        }
    }

    public void MoveSoldierToAnotherCell(SoldierCell targetCell)
    {
        StartCoroutine(MoveSoldier_CO(targetCell));
    }

    private IEnumerator MoveSoldier_CO(SoldierCell targetCell, float duration = 1)
    {
        SetState(SoldierState.Running);
        float counter = 0.0f;
        int distance = (int)(targetCell.transform.position.z - transform.position.z);
        Vector3 startPos = transform.position;
        Vector3 toPosition = transform.position + new Vector3(0, 0, distance);
        duration *= distance;
        while (counter < duration)
        {
            counter += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, toPosition, counter / duration);
            yield return null;
        }
        transform.position = toPosition;
        SetState(SoldierState.Idle);
        targetCell.TakeNewSolider(this);
    }

    private void CheckShootingCondition()
    {
        if (Type == SoldierType.Normal)
        {
            ChooseTargetToShoot();
            if (shooterTarget != null)
            {
                SetState(SoldierState.Shooting);
            }
        }
    }

    public void GetReadyForShooting()
    {
        if (Type != SoldierType.Normal)
            return;
        
        ChooseTargetToShoot();
        if (shooterTarget != null)
        {
            SetState(SoldierState.Shooting);
            Vector3 currentPos = modelTransform.position;
            Vector3 currentTarget = shooterTarget.transform.position;
            modelTransform.rotation = Quaternion.LookRotation(currentTarget-currentPos);
        }
        else
        {
            SetState(SoldierState.Idle);
        }
    }
    
    public void Shoot()
    {
        if (Type != SoldierType.Normal)
            return;

        if (shooterTarget != null)
        {
            SetState(SoldierState.Shooting);
            Bullet newBullet = bulletPool.Spawn().GetComponent <Bullet>();
            newBullet.transform.position = bulletExitPoint.position;
            newBullet.BulletColor = soldierColor;
            newBullet.SetBulletDamage(valueNumber);
            newBullet.SetTarget(shooterTarget.transform);
            newBullet.InvokeSelfDestruction();
        }
        else
        {
            SetState(SoldierState.Idle);
        }
    }

    private void ChooseTargetToShoot()
    {
        if (GameManager.Instance.insideEnemies.Count > 0)
        {
            foreach (Enemy insideEnemy in GameManager.Instance.insideEnemies)
            {
                Debug.Log(Vector3.Distance(transform.position, insideEnemy.transform.position) + " " + cell.name + " " + insideEnemy.name); ;
                if (Vector3.Distance(transform.position,insideEnemy.transform.position) <= Database.GameConfiguration.InsideShootingRadius)
                {
                    shooterTarget = insideEnemy;
                    return;
                }
            }
        }

        float nearestDistance = Mathf.Infinity;
        float lastDistance;
        Enemy chosenTarget = null;

        if (IsShooter)
        {
            foreach (Enemy enemy in GameManager.Instance.outsideEnemies)
            {
                lastDistance = Vector3.Distance(transform.position, enemy.transform.position);
                if (lastDistance < nearestDistance)
                {
                    nearestDistance = lastDistance;
                    chosenTarget = enemy;
                }
            }   
        }
        shooterTarget = chosenTarget;
    }

    public void GoBomb()
    {
        SetState(SoldierState.Bombing);
        bomberTarget = null;
        cell.IsFull = false;
        //SoldierCellMergeManager.Instance.ShiftSoldiers(this);
        Invoke(nameof(ShiftAfterBomberGone), 0.1f);
    }

    private void ShiftAfterBomberGone() => SoldierCellMergeManager.Instance.ShiftSoldiers();

    public void Explode()
    {
        GameManager.Instance.Explosion(transform.position, Database.GameConfiguration.BomberExplosionRadius, Database.GameConfiguration.BomberExplosionDamage);
        SetState(SoldierState.Idle);
        bomberTarget = null;
        ObjectPool.DeSpawn(gameObject);
    }
    
    public void SetDestination(Vector3 pos)
    {
        navmeshAgent.SetDestination(pos);
    }
}
