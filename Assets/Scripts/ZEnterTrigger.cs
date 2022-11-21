using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZEnterTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && GameManager.Instance.IsInPlayMode)
            GameManager.Instance.OnEnemyEntered.Invoke();
    }
}
