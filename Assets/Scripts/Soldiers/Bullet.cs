using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private MeshRenderer thisMesh;
    [SerializeField] private TrailRenderer thisTrail;

    private float damage;
    private Rigidbody thisRigid;
    private Transform target;

    public Color BulletColor
    {
        set
        {
            thisMesh.material.color = value;
            thisTrail.startColor = value;
            Color c = value;
            c.a = 0f;
            thisTrail.endColor = c;
        }
        get
        {
            return thisMesh.material.color;
        }
    }

    private void Start()
    {
        thisRigid = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (target != null && target.gameObject.activeInHierarchy)
        {
            Vector3 targetPos = target.position;
            targetPos.y = Database.GameConfiguration.BulletOffsetFromGround;
            thisRigid.velocity = (targetPos - transform.position).normalized * Database.GameConfiguration.BulletSpeed;
        }
        else
        {
            thisRigid.velocity = Vector3.zero;
            ObjectPool.DeSpawn(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
            HitEnemy(other.GetComponent<Enemy>());
        else if (other.CompareTag("Wall") && Database.GameConfiguration.WallsStopBullet)
        {
            SoundManager.Instance.Play(Sound.BulletDestroy, 0.5f);
            ParticleManager.Instance.PlayParticle(Particle_Type.BulletDestroy, transform.position, transform.forward);
            ObjectPool.DeSpawn(gameObject);
        }
    }

    public void SetBulletDamage(float damage)
    {
        this.damage = damage;
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    private void HitEnemy(Enemy enemy)
    {
        enemy.TakeDamage(damage);
        thisRigid.velocity = Vector3.zero;
        ParticleManager.Instance.PlayParticle(Particle_Type.BulletHit, transform.position, transform.forward, enemy.transform);
        DamageUIManager.Instance.ShowDamageUI(enemy.transform, damage, BulletColor, enemy.Data.Type==EnemyType.SimpleEnemy?0f:0.5f);
        ObjectPool.DeSpawn(gameObject);
    }

    public void InvokeSelfDestruction() => Invoke(nameof(SelfDestruct), Database.GameConfiguration.BulletDestructionTime);

    private void SelfDestruct() {
        //thisRigid.velocity = Vector3.zero;
        //ObjectPool.DeSpawn(gameObject);
    }
}
