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
        if(Path==null || Path.Order.Count<2 || SoldierCellMergeManager.Instance.IsShifting || SoldierCellMergeManager.Instance.IsMerging || UIManager.Instance.waitingForInput)
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

    public void SetPath(TutorialPath path)
    {
        Path = new TutorialPath();
        Path.Order = new List<Vector2>(path.Order.Count);
        for (int i = 0; i < path.Order.Count; i++)
        {
            Path.Order.Add(path.Order[i]);
        }
    }

    public void ClearPath()
    {
        Path.Order.Clear();
        Deactivate();
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

    public TutorialPath GetRandomPath(List<List<SoldierCell>> cells)
    {
        TutorialPath tutorialPath = new TutorialPath();
        tutorialPath.Order = new List<Vector2>();
        Stack<Vector2> readyToCheck = new Stack<Vector2>();
        for (int i = cells.Count - 1; i >= 0; i--)
        {
            for (int j = cells[0].Count - 1; j >= 0; j--)
            {
                tutorialPath.Order.Clear();
                tutorialPath.Order.Add(new Vector2(i, j));
                int firstNodeValue = cells[i][j].currentSoldier.ValueNumber;
                List<Vector2> nextStepCandidates = GetAroundCellsIndex(cells, i, j, tutorialPath.Order);
                if (nextStepCandidates.Count > 0)
                {
                    foreach (Vector2 candidate in nextStepCandidates)
                        if (firstNodeValue == cells[(int)candidate.x][(int)candidate.y].currentSoldier.ValueNumber)
                            readyToCheck.Push(candidate);
                    while (readyToCheck.Count > 0)
                    {
                        Vector2 nextNode = readyToCheck.Pop();
                        tutorialPath.Order.Add(nextNode);
                        nextStepCandidates = GetAroundCellsIndex(cells, (int)nextNode.x, (int)nextNode.y, tutorialPath.Order);
                        if (nextStepCandidates.Count > 0)
                        {
                            foreach (Vector2 candidate in nextStepCandidates)
                                readyToCheck.Push(candidate);
                        }
                        else
                        {
                            if (tutorialPath.Order.Count > 2)
                                return tutorialPath;
                            else
                                tutorialPath.Order.Remove(nextNode);
                        }
                    }
                    if (tutorialPath.Order.Count > 2)
                        return tutorialPath;
                }
                else
                    tutorialPath.Order.Remove(new Vector2(i, j));
            }
        }
        return null;
    }
    private List<Vector2> GetAroundCellsIndex(List<List<SoldierCell>> cells, int a, int b, List<Vector2> forbiddens)
    {
        int targetValue = cells[a][b].currentSoldier.ValueNumber;
        List<Vector2> aroundCandidates = new List<Vector2>();
        for (int i = a - 1; i <= a + 1; i++)
        {
            if (i < 0 || i > cells.Count - 1)
                continue;
            for (int j = b - 1; j <= b + 1; j++)
            {
                if (j < 0 || j > cells[0].Count - 1)
                    continue;
                if (i == a && j == b)
                    continue;
                int closeCellValue = cells[i][j].currentSoldier.ValueNumber;
                if (closeCellValue == targetValue || closeCellValue == targetValue * 2)
                {
                    if (!forbiddens.Contains(new Vector2(i, j)))
                        aroundCandidates.Add(new Vector2(i, j));
                }
            }
        }
        return aroundCandidates;
    }
    /*public List<Vector2> GetRandomPath(List<List<SoldierCell>> cells)
    {
        for (int i = cells.Count - 1; i >= 0; i--)
        {
            for (int j = cells[0].Count - 1; j >= 0 ; j--)
            {
            List<Vector2> path = GetAroundCellsIndex(cells, i, j);
                if (path.Count > 1)
                {
                    path.Insert(1, new Vector2(i, j));
                    return path;
                }
            }
        }
        return null;
    }
    private List<Vector2> GetAroundCellsIndex(List<List<SoldierCell>> cells, int a, int b)
    {
        int targetValue = cells[a][b].currentSoldier.ValueNumber;
        List<Vector2> pathIndexes = new List<Vector2>();
        Stack<Vector2> sameValue = new Stack<Vector2>();
        Stack<Vector2> multipliedValue = new Stack<Vector2>();
        for (int i = a - 1; i <= a + 1; i++)
        {
            if (i < 0 || i > cells.Count - 1)
                continue;
            for (int j = b - 1; j <= b + 1; j++)
            {
            if (j < 0 || j > cells[0].Count - 1)
                continue;
                if (i == a && j == b)
                continue;
            int closeCellValue = cells[i][j].currentSoldier.ValueNumber;
            if (closeCellValue == targetValue)
                sameValue.Push(new Vector2(i, j));
            if (closeCellValue == targetValue * 2)
                multipliedValue.Push(new Vector2(i, j));
            }
        }
        if (sameValue.Count > 0)
        {
            while (pathIndexes.Count < 2 && sameValue.Count > 0)
            {
                pathIndexes.Add(sameValue.Pop());
            }
            while (pathIndexes.Count < 2 && multipliedValue.Count > 0)
            {
                pathIndexes.Add(multipliedValue.Pop());
            }
        }
        return pathIndexes;
    }*/
}