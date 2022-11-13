using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class DamageUIManager : MonoBehaviour
{
    [SerializeField] private ObjectPool numbersPool;
    [SerializeField] private Vector2 minNumberPos = new Vector2 (-0.2f,1.3f), maxNumberPos = new Vector2(0.2f, 1.7f);
    private Dictionary<int, LastDamageInfo> lastDamageInfoForParent;

    private const string DAMAGE_TEXT_PREFIX = "-";
    
    public static DamageUIManager Instance
    {
        get
        {
            return _instance;
        }
    }

    private static DamageUIManager _instance;
    
    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        
        lastDamageInfoForParent = new Dictionary<int, LastDamageInfo>();
    }
    
    public void ShowDamageUI(Transform parent, float damageAmount , Color textColor, float offsetY = 0f)
    {
        //StartCoroutine(ShowDamageUI_CO(parent, yOffset, damageAmount));
        /*int parentID = parent.GetInstanceID();
        if (!lastDamageInfoForParent.ContainsKey(parentID))
        {
            lastDamageInfoForParent.Add(parentID, new LastDamageInfo(float.MinValue, 0f, null));
        }
        if (Time.time - lastDamageInfoForParent[parentID].time > 0.5f)
        {*/
        Transform damageCanvas = numbersPool.Spawn(parent).transform;
        TextMeshProUGUI damageText = damageCanvas.GetChild(0).GetComponent<TextMeshProUGUI>();
        //lastDamageInfoForParent[parentID].txt = damageText;
        //lastDamageInfoForParent[parentID].damageSum = damageAmount;
        //damageText.text = DAMAGE_TEXT_PREFIX + lastDamageInfoForParent[parentID].damageSum;
        damageText.text = DAMAGE_TEXT_PREFIX + damageAmount;
        damageText.color = textColor;
        Vector3 pos = Vector3.zero;
        pos.x = Random.Range(minNumberPos.x, maxNumberPos.x); 
        pos.y = Random.Range(minNumberPos.y, maxNumberPos.y);
        pos.y += offsetY;
        damageCanvas.localPosition = pos;
        StartCoroutine(ChangeAlphaAndMove(damageCanvas, damageText, () =>
        {
            ObjectPool.DeSpawn(damageCanvas.gameObject);
        }));
        /*}
        else
        {
            lastDamageInfoForParent[parentID].damageSum += damageAmount;
            lastDamageInfoForParent[parentID].txt.text = DAMAGE_TEXT_PREFIX + Utils.GetShortenNumText(lastDamageInfoForParent[parentID].damageSum).ToString();
        }
        lastDamageInfoForParent[parentID].time = Time.time;*/
    }
    
    private IEnumerator ChangeAlphaAndMove(Transform obj, TextMeshProUGUI txt, Action onEnd = null)
    {
        txt.alpha = 1f;
        obj.localScale = Vector3.one;
        while(txt.alpha > 0)
        {
            txt.alpha -= Time.deltaTime;
            obj.localPosition += 0.5f * Time.deltaTime * Vector3.up;
            obj.localScale -= 0.1f * Time.deltaTime * Vector3.one;
            yield return new WaitForEndOfFrame();
        }
        if(onEnd != null)
            onEnd.Invoke();
    }
    
    class LastDamageInfo
    {
        public float time;
        public float damageSum;
        public TextMeshProUGUI txt;

        public LastDamageInfo(float time, float damageSum, TextMeshProUGUI txt)
        {
            this.time = time;
            this.damageSum = damageSum;
            this.txt = txt;
        }
    }
}
