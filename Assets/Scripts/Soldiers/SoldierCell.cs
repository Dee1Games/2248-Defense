using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierCell : MonoBehaviour
{
    public bool IsFull = true;
    public Soldier currentSoldier;
    private bool isShootingCell = false;
    
    public void Init(bool isShootingCell, int valueNumber, SoldierType type)
    {
        this.isShootingCell = isShootingCell;
        InvokeRepeating(nameof(DetectClosestEnemy), 0, Database.GameConfiguration.SoldiersFireRate);
        currentSoldier.Init(valueNumber, type);
    }

    public void ClearCell()
    {
        ObjectPool.DeSpawn(currentSoldier.gameObject);
        IsFull = false;
    }

    public void TakeNewSolider(Soldier newSoldier) {
        newSoldier.transform.SetParent(transform);
        currentSoldier = newSoldier;
        IsFull = true;
    }

    private void DetectClosestEnemy()
    {
        if(isShootingCell && IsFull)
        {
            float nearestDistance = Mathf.Infinity;
            float lastDistance = 0;
            Vector3 chosenTarget = Vector3.zero;
            foreach (Enemy enemy in GameManager.Instance.CurrentEnemies)
            {
                lastDistance = Vector3.Distance(transform.position, enemy.transform.position);
                if (lastDistance < nearestDistance)
                {
                    nearestDistance = lastDistance;
                    chosenTarget = enemy.transform.position;
                }
            }
            if(chosenTarget != Vector3.zero)
                currentSoldier.Shoot(chosenTarget);
        }
    }

    public void OnPointerDown()
    {
        SoldierCellMergeManager.Instance.StartConnecting(this);
    }
    
    public void OnPointerEnter()
    {
        SoldierCellMergeManager.Instance.ConnectCell(this);
    }
 
}
