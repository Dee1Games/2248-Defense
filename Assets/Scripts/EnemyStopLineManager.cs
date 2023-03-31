using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStopLineManager : MonoBehaviour
{
    public static EnemyStopLineManager Instance
    {
        get
        {
            return _instance;
        }
    }
    
    private static EnemyStopLineManager _instance;

    public bool IsActive
    {
        set
        {
            isActive = value;
            stopLineGO.SetActive(value);
            SpawnManager.Instance.SetStopLineForCurrentTheme(value);
            if (value)
                Shake = false;
        }
        get
        {
            return isActive;
        }
    }
    private bool isActive;
    
    public GameObject stopLineGO;
    
    private bool shake;

    public bool Shake
    {
        set
        {
            shake = value;
            SpawnManager.Instance.ShakeStopLineForCurrentTheme(value);
        }
        get
        {
            return shake;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    public void Enable()
    {
        IsActive = true;
    }

    public void Disable()
    {
        IsActive = false;
        PlayDisappearAnimation();
        if (GameManager.Instance.insideEnemies.Count == 0)
        {
            foreach (SoldierCell cell in SoldierCellMergeManager.Instance.ShootingCells)
            {
                cell.currentSoldier.GoToAttackState();
            }
        }
    }

    public void PlayDisappearAnimation()
    {
        ParticleManager.Instance.PlayParticle(Particle_Type.Smoke, stopLineGO.transform.position, Vector3.up);
        ParticleManager.Instance.PlayParticle(Particle_Type.Smoke, stopLineGO.transform.position + (Vector3.left), Vector3.up);
        ParticleManager.Instance.PlayParticle(Particle_Type.Smoke, stopLineGO.transform.position + (Vector3.left*2), Vector3.up);
        ParticleManager.Instance.PlayParticle(Particle_Type.Smoke, stopLineGO.transform.position + (Vector3.right), Vector3.up);
        ParticleManager.Instance.PlayParticle(Particle_Type.Smoke, stopLineGO.transform.position + (Vector3.right*2), Vector3.up);
    }
}
