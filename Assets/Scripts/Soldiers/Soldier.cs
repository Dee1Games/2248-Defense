using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Soldier : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer thisMeshRenderer;
    [SerializeField] private Animator thisAnimator;
    [SerializeField] private TextMeshProUGUI valueText;
    private ObjectPool bulletPool;

    public float bulletSpeed = 5;
    public float fireRate = 0.2f;

    private SoldierType type;
    private int valueNumber;
    private Color soldierColor;

    [HideInInspector] public Vector3 shootTarget;
    
    public enum SOLDIER_STATE { Idle, Running }

    public SoldierType Type {
        set
        {
            //change soldier completely
            type = value;
        }
        get
        {
            return type;
        }
    }

    public SOLDIER_STATE SoldierState
    {
        set
        {
            if (value == SOLDIER_STATE.Idle)
                thisAnimator.SetFloat("Forward", 0);
            else if(value == SOLDIER_STATE.Running)
                thisAnimator.SetFloat("Forward", 1);
        }
    }

    public int ValueNumber
    {
        set {
            valueText.text = value.ToString();
            valueNumber = value;
            SoldierColor = Utils.GetColorByNumber(ValueNumber);
        }
        get { return valueNumber; }
    }

    public Color SoldierColor
    {
        set {
            soldierColor = value;
            thisMeshRenderer.material.color = value;
        }
        get { return soldierColor; }
    }

    private void Start()
    {
        bulletPool = GameObject.Find("BulletPool").GetComponent<ObjectPool>();
    }

    public void Init(int valueNumber, SoldierType type)
    {
        ValueNumber = valueNumber;
        Type = type;

        ObjectPoolRefrence poolRefrence = GetComponent<ObjectPoolRefrence>();
        if (poolRefrence == null)
        {
            poolRefrence = gameObject.AddComponent<ObjectPoolRefrence>();
        }
        poolRefrence.pool = ((Type == SoldierType.Normal)
            ? SoldierCellMergeManager.Instance.NormalSoldierPool
            : SoldierCellMergeManager.Instance.BomberSoldierPool);
    }

    public void MoveSoldierToAnotherCell(SoldierCell targetCell)
    {
        StartCoroutine(IEMoveSoldier(targetCell));
    }

    private IEnumerator IEMoveSoldier(SoldierCell targetCell, float duration = 1)
    {
        SoldierState = SOLDIER_STATE.Running;
        float counter = 0.0f;
        int distance = (int)(targetCell.transform.position.z - transform.position.z);
        Vector3 startPos = transform.position;
        Vector3 toPosition = transform.position + new Vector3(0, 0, distance);
        duration *= distance;
        while (counter < duration)
        {
            counter += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, toPosition, counter / duration);
            yield return null;
        }
        transform.position = toPosition;
        SoldierState = SOLDIER_STATE.Idle;
        targetCell.TakeNewSolider(this);
    }
    
    public void Shoot(Vector3 target)
    {
        target.y = Database.GameConfiguration.BulletOffsetFromGround;
        Bullet newBullet = bulletPool.Spawn(transform).GetComponent <Bullet>() ;
        newBullet.transform.localPosition = new Vector3(0,Database.GameConfiguration.BulletOffsetFromGround,0);
        newBullet.SetBulletDamage(valueNumber);
        newBullet.InvokeSelfDestruction();
        newBullet.GetComponent<Rigidbody>().velocity = (target - newBullet.transform.position) * bulletSpeed;
    }
}
