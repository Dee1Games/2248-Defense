using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance
    {
        get
        {
            return _instance;
        }
    }
    private static TutorialManager _instance;

    public bool IsActive;
    
    public TutorialPath Path;
    private int currentCellIndex;

    [SerializeField] private Transform hand;
    [SerializeField] private float handSpeed;
    [SerializeField] private float offsetFromGround;
    [SerializeField] private float finishWaitTime;
    private bool finished = false;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    void Update()
    {
        if(Path==null || Path.Order.Count<2 || SoldierCellMergeManager.Instance.IsShifting || SoldierCellMergeManager.Instance.IsMerging)
            return;

        float t = PlayerPrefsManager.SeenTutorial ? 3f : 1f;
        if (GameManager.Instance.IdleTime > t)
        {
            if (!IsActive)
                Activate(Path);
        }
        else
        {
            Deactivate();
            return;
        }
        
        if(finished)
            return;

        float step = handSpeed * Time.deltaTime;
        Vector2 pos = Path.Order[currentCellIndex];
        Vector3 target = new Vector3(pos.y, offsetFromGround,  pos.x-3.8f);
        if (Vector3.Distance(hand.transform.position, target) > step)
        {
            hand.transform.position = Vector3.MoveTowards(hand.transform.position, target, step);
        }
        else
        {
            hand.transform.position = target;
            currentCellIndex++;
            if (currentCellIndex >= Path.Order.Count)
            {
                finished = true;
                Invoke("ResetPosition", finishWaitTime);
            }
        }
    }

    public void Help()
    {
        if (PlayerPrefsManager.SeenTutorial)
        {
            TutorialPath _path = GameManager.Instance.CurrentLevelData.Tutorials[GameManager.Instance.CurrentTutorialIndex];
            if(_path!=null)
                TutorialManager.Instance.Activate(_path);
        }
        else
        {
            TutorialPath _path = Database.TutorialConfiguration.Data[GameManager.Instance.CurrentTutorialIndex].Path;
            if(_path!=null)
                TutorialManager.Instance.Activate(_path);
        }
    }

    private void Activate(TutorialPath path)
    {
        Path = new TutorialPath();
        Path.Order = new List<Vector2>(path.Order.Count);
        for (int i = 0; i < path.Order.Count; i++)
        {
            Path.Order.Add(path.Order[i]);
        }
        hand.gameObject.SetActive(true);
        IsActive = true;
        ResetPosition();
    }
    
    public void Deactivate()
    {
        hand.gameObject.SetActive(false);
        IsActive = false;
    }

    public void End()
    {
        Path.Order.Clear();
        Deactivate();
    }

    private void ResetPosition()
    {
        if (!IsActive)
            return;
        
        finished = false;
        Vector2 pos = Path.Order[0];
        Vector3 target = new Vector3(pos.y, offsetFromGround, pos.x-3.8f);
        hand.transform.position = target;
        currentCellIndex = 1;
    }
}
