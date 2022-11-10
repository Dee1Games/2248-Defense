using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierAnimationFunctions : MonoBehaviour
{
    [SerializeField] private Soldier soldier;

    public void GetReadyForShooting()
    {
        soldier.GetReadyForShooting();
    }

    public void Shoot()
    {
        soldier.Shoot();
    }
}
