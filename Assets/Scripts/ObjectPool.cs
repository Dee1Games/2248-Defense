using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int num;

    void Start()
    {
        Init();
    }

    public void Init()
    {
        int childCount = transform.childCount;
        for (int i = 0; i < num - childCount; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.transform.SetParent(transform);
            obj.SetActive(false);
        }
    }

    public GameObject Spawn(Transform parent = null)
    {
        Transform obj;
        if (transform.childCount > 0) {
            obj = transform.GetChild(0);
        } else {
            obj = Instantiate(prefab).transform;
        }
        
        if (parent != null) {
            obj.SetParent(parent);
        } else {
            obj.SetParent(transform);
        }

        ObjectPoolRefrence poolRefrence = obj.gameObject.GetComponent<ObjectPoolRefrence>();
        if (poolRefrence == null)
        {
            poolRefrence = obj.gameObject.AddComponent<ObjectPoolRefrence>();
        }
        poolRefrence.pool = this;
        
        obj.gameObject.SetActive(true);
        return obj.gameObject;
    }

    private void DeSpawnObject(GameObject obj)
    {
        obj.transform.SetParent(transform);
        obj.SetActive(false);
    }

    private void ClearParent()
    {
        Transform obj;
        while (transform.childCount > 0)
        {
            obj = transform.GetChild(0);
            obj.SetParent(transform);
            obj.gameObject.SetActive(false);
        }
    }

    public static void DeSpawn(GameObject obj)
    {
        ObjectPoolRefrence poolRefrence = obj.GetComponent<ObjectPoolRefrence>();
        poolRefrence.pool.DeSpawnObject(obj);
    }
}
