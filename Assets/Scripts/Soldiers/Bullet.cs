using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float damage;
    private Rigidbody thisRigid;

    private void Start()
    {
        thisRigid = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
            HitEnemy(other.GetComponent<Enemy>());
    }

    public void SetBulletDamage(float damage)
    {
        this.damage = damage;
    }

    private void HitEnemy(Enemy enemy)
    {
        enemy.TakeDamage(damage);
        thisRigid.velocity = Vector3.zero;
        ObjectPool.DeSpawn(gameObject);
    }

    public void InvokeSelfDestruction() => Invoke(nameof(SelfDestruct), Database.GameConfiguration.BulletDestructionTime);

    private void SelfDestruct() {
        thisRigid.velocity = Vector3.zero;
        ObjectPool.DeSpawn(gameObject);
    }
}
