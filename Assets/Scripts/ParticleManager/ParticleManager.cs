using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance
    {
        get
        {
            return _instance;
        }
    }

    [SerializeField] private Particle[] allParticles;
    [SerializeField] private Transform runningParticlesParent;

    private static ParticleManager _instance;

    private Dictionary<Particle_Type, ObjectPool> organizedParticles = new Dictionary<Particle_Type, ObjectPool>();
    
    private void Awake()
    {
        if (_instance == null)
            _instance = this;

        InitParticles();
    }

    private void InitParticles()
    {
        foreach(Particle p in allParticles)
        {
            if (organizedParticles.ContainsKey(p.particleType))
                organizedParticles[p.particleType] = p.particlePool;
            else
                organizedParticles.Add(p.particleType, p.particlePool);
        }
    }

    public void PlayParticle(Particle_Type type, Vector3 pos, Vector3 dir, Transform parent = null, float delay = 0f)
    {
        StartCoroutine(Play(type, pos, dir, parent, delay));
    }

    private IEnumerator Play(Particle_Type type, Vector3 pos, Vector3 dir, Transform parent = null, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);
        ParticleSystem ps = organizedParticles[type].Spawn(parent==null?runningParticlesParent:parent).GetComponent<ParticleSystem>();
        ps.transform.position = pos;
        ps.transform.rotation = Quaternion.LookRotation(dir);
        ps.Play();
        yield return new WaitForSeconds(ps.main.duration);
        ObjectPool.DeSpawn(ps.gameObject);
    }
}
