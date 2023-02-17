using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.Examples;
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
        }
        get
        {
            return isActive;
        }
    }
    private bool isActive;
    
    [SerializeField] private GameObject stopLineGO;

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
        if (GameManager.Instance.insideEnemies.Count == 0)
        {
            foreach (SoldierCell cell in SoldierCellMergeManager.Instance.ShootingCells)
            {
                cell.currentSoldier.GoToAttackState();
            }
        }
    }
}
