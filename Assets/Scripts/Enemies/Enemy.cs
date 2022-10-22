using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

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
    
    public EnemyData Data;

    [SerializeField] private SkinnedMeshRenderer renderer;
    
    private NavMeshAgent navmeshAgent;
    private ThirdPersonCharacter characterCotroller;
    private CapsuleCollider collider;

    public void Init(EnemyData data)
    {
        navmeshAgent = GetComponent<NavMeshAgent>();
        characterCotroller = GetComponent<ThirdPersonCharacter>();
        collider = GetComponent<CapsuleCollider>();
        this.Data.Copy(data);
        SetState(EnemyState.Running);
        transform.eulerAngles = new Vector3(0f, 180f, 0f);
        navmeshAgent.updateRotation = false;
        renderer.materials[0].color = Data.GetColor();
        characterCotroller.Init();
        SetDestination(new Vector3(Data.X, 0f, 0f));
    }
    
    void Update()
    {
        if (IsAlive && navmeshAgent.remainingDistance > navmeshAgent.stoppingDistance)
        {
            characterCotroller.Move(navmeshAgent.desiredVelocity, false, false);
        }
        else
        {
            characterCotroller.Move(Vector3.zero, false, false);
        }
    }

    public void SetDestination(Vector3 pos)
    {
        navmeshAgent.SetDestination(pos);
    }

    public void TakeDamage(float damage)
    {
        Data.Health -= damage;
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
            case EnemyState.Running:
                collider.isTrigger = false;
                navmeshAgent.enabled = true;
                break;
            case EnemyState.Attacking:
                collider.isTrigger = false;
                navmeshAgent.enabled = true;
                break;
            case EnemyState.Dead:
                collider.isTrigger = true;
                GameManager.Instance.CurrentEnemies.Remove(this);
                navmeshAgent.enabled = false;
                characterCotroller.Die();
                break;
        }
    }
}
