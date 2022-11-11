using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierCell : MonoBehaviour
{
    public bool IsFull = true;
    public Soldier currentSoldier;

    private bool isShootingCell = false;
    private BoxCollider thisCol;
    int thisColumn, thisRow;
    
    public (int column,int row) Coordination
    {
        get
        {
            return (column : thisColumn, thisRow);
        }
    }

    public bool ColEnabled
    {
        set
        {
            thisCol.enabled = value;
        }
    }

    private void Start()
    {
        thisCol = GetComponent<BoxCollider>();
    }

    public void Init(int col, int row, bool isShootingCell, int valueNumber, SoldierType type)
    {
        thisColumn = col;
        thisRow = row;
        this.isShootingCell = isShootingCell;
        if(currentSoldier != null)
            Destroy(currentSoldier.gameObject);
        currentSoldier = (type==SoldierType.Bomber?SoldierCellMergeManager.Instance.BomberSoldierPool:SoldierCellMergeManager.Instance.NormalSoldierPool).Spawn(transform).GetComponent<Soldier>();
        currentSoldier.transform.localPosition = Vector3.zero;
        currentSoldier.ValueNumber = valueNumber;
        currentSoldier.Type = type;
        currentSoldier.IsShooter = isShootingCell;
        currentSoldier.curCol = thisColumn;
        currentSoldier.curRow = thisRow;
        currentSoldier.Init();
        currentSoldier.SetCell(this);
        IsFull = true;
    }

    // private void DetectClosestEnemy()
    // {
    //     if(isShootingCell && IsFull && currentSoldier.Type == SoldierType.Normal)
    //     {
    //         float nearestDistance = Mathf.Infinity;
    //         float lastDistance = 0;
    //         Vector3 chosenTarget = Vector3.zero;
    //         foreach (Enemy enemy in GameManager.Instance.CurrentEnemies)
    //         {
    //             lastDistance = Vector3.Distance(transform.position, enemy.transform.position);
    //             if (lastDistance < nearestDistance)
    //             {
    //                 nearestDistance = lastDistance;
    //                 chosenTarget = enemy.transform.position;
    //             }
    //         }
    //         if(chosenTarget != Vector3.zero)
    //             currentSoldier.Shoot(chosenTarget);
    //     }
    // }

    public void OnPointerDown()
    {
        if (currentSoldier.Type == SoldierType.Bomber)
            return;
        
        SoldierCellMergeManager.Instance.StartConnecting(this);
    }
    
    public void OnPointerEnter()
    {
        if (currentSoldier.Type == SoldierType.Bomber)
            return;
        
        SoldierCellMergeManager.Instance.ConnectCell(this);
    }

    // private void ShootDesicion()
    // {
    //     if(GameManager.Instance.insideEnemies.Count > 0)
    //     {
    //         foreach (KeyValuePair<(int, int), List<Enemy>> insideEnemy in GameManager.Instance.insideEnemies)
    //         {
    //             if(Mathf.Abs(insideEnemy.Key.Item1 - thisRow) <= 1 && Mathf.Abs(insideEnemy.Key.Item2 - thisColumn) <= 1)
    //             {
    //                 if (insideEnemy.Value.Count > 0)
    //                 {
    //                     currentSoldier.Shoot(insideEnemy.Value[0].transform);
    //                     break;
    //                 }
    //             }
    //         } 
    //     }
    //     else if(currentSoldier.IsShooter && IsFull)
    //     {
    //         float nearestDistance = Mathf.Infinity;
    //         float lastDistance;
    //         Transform chosenTarget = null;
    //         foreach (Enemy enemy in GameManager.Instance.CurrentEnemies)
    //         {
    //             lastDistance = Vector3.Distance(transform.position, enemy.transform.position);
    //             if (lastDistance < nearestDistance)
    //             {
    //                 nearestDistance = lastDistance;
    //                 chosenTarget = enemy.transform;
    //             }
    //         }
    //         if (chosenTarget != null)
    //         {
    //             if (currentSoldier.Type == SoldierType.Normal)
    //                 currentSoldier.Shoot(chosenTarget);
    //             else
    //             {
    //                 currentSoldier.GoBomb();
    //                 IsFull = false;
    //             }
    //         }
    //     }
    // }

    public void ClearCell()
    {
        ObjectPool.DeSpawn(currentSoldier.gameObject);
        IsFull = false;
    }

    public void TakeNewSolider(Soldier newSoldier) {
        newSoldier.transform.SetParent(transform);
        currentSoldier = newSoldier;
        currentSoldier.curCol = thisColumn;
        currentSoldier.curRow = thisRow;
        IsFull = true;
        currentSoldier.SetCell(this);
        if (isShootingCell)
        {
            currentSoldier.IsShooter = true;
            currentSoldier.GoToAttackState();
        }
    }

    public void GettingHit()
    {
        if(currentSoldier.Type == SoldierType.Bomber)
        {
            currentSoldier.Explode();
            ClearCell();
        }
        else
        if (currentSoldier.ValueNumber / 2 == 1)
        {
            currentSoldier.SetState(SoldierState.Dead);
            IsFull = false;
        }
        else
        {
            currentSoldier.ValueNumber /= 2;
        }
    }

/*    private void EnemyEntered(Enemy enemy)
    {
        Debug.Log("enemy entered");

        if (GameManager.Instance.CurrentEnemies.Contains(enemy))
            GameManager.Instance.CurrentEnemies.Remove(enemy);

        if (GameManager.Instance.insideEnemies.ContainsKey((thisRow, thisColumn)))
            GameManager.Instance.insideEnemies[(thisRow, thisColumn)].Add(enemy);
        else
        {
            List<Enemy> tempEnemy = new List<Enemy>() 
            {
                enemy
            };
            GameManager.Instance.insideEnemies.Add((thisRow, thisColumn), tempEnemy);
        }
        GameManager.Instance.OnEnemyEntered.Invoke();
        enemy.OnDiedInside = EnemyExited;
        
    }

    private void EnemyExited(Enemy enemy)
    {
        Debug.Log("enemy exited");

        if (GameManager.Instance.insideEnemies.ContainsKey((thisRow, thisColumn)))
            if (GameManager.Instance.insideEnemies[(thisRow, thisColumn)].Contains(enemy))
                GameManager.Instance.insideEnemies[(thisRow, thisColumn)].Remove(enemy);

        if (GameManager.Instance.insideEnemies[(thisRow, thisColumn)].Count == 0)
            GameManager.Instance.insideEnemies.Remove((thisRow, thisColumn));

        if (GameManager.Instance.insideEnemies.Count == 0)
            SoldierCellMergeManager.Instance.ShiftSoldiers();
    }*/

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && GameManager.Instance.IsInPlayMode)
            GameManager.Instance.OnEnemyEntered.Invoke();
    }

    /*private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
            EnemyExited(other.GetComponent<Enemy>());
    }*/
}
