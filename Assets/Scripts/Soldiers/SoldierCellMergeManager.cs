using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoldierCellMergeManager : MonoBehaviour
{
    public static SoldierCellMergeManager Instance
    {
        get
        {
            return _instance;
        }
    }
    private static SoldierCellMergeManager _instance;

    [SerializeField] private Transform[] soldierBases;
    [SerializeField] private LineRenderer connectingLine;
    [SerializeField] private Transform soldiersCellContainer;
    [SerializeField] private Transform enteredEnemiesContainer;
    [SerializeField] public ObjectPool NormalSoldierPool;
    [SerializeField] public ObjectPool BomberSoldierPool;
    [SerializeField] private TextMeshProUGUI sumOfValuesText;
    [SerializeField] private float connectionCooldown = 5;

    private List<List<SoldierCell>> cells;
    private List<SoldierCell> connectedCells = new List<SoldierCell>();
    private int currentCellValue, currentSumOfValues, currentPowerAddition;
    private bool isConnecting = false;
    private float currentPitch, pitchDifference;
    
    public bool CanConnect => !IsShifting;

    public bool IsShifting = false;
    public bool IsMerging = false;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        pitchDifference = Database.GameConfiguration.ConnectPitchDifference;
    }

    private void Update()
    {
        if (isConnecting)
        {
            Vector3 pointerPosition = GetPointerPosition();
            if(pointerPosition!=Vector3.zero)
                SetLineRendererPosition(connectingLine.positionCount - 1, pointerPosition);
        }

        if (!Input.GetMouseButton(0) && isConnecting)
        {
            FinishedConnecting();
        }
    }

    public void Init()
    {
        cells = new List<List<SoldierCell>>();
        int childIndex = 0;
        for (int row = 0; row < 4; row++)
        {
            cells.Add(new List<SoldierCell>());
            for (int column = 0; column < 5; column++)
            {
                SoldierCell cell = soldiersCellContainer.GetChild(childIndex).GetComponent<SoldierCell>();
                cell.transform.name = "Cell " + row + " " + column;
                cell.transform.localPosition = new Vector3(column, 0.065f, row);
                int cellNumber = GameManager.Instance.CurrentLevelData.GetCellNumber(column, 3 - row);
                currentPowerAddition = (int) GameManager.Instance.currentCoefficient / 2;
                cellNumber *= (int)Mathf.Pow(2, currentPowerAddition);
                cell.Init(column, row, row == 3, cellNumber, (cellNumber>0?SoldierType.Normal:SoldierType.Bomber));
                cells[row].Add(cell);
                childIndex++;
            }
        }
    }
    
    public void InitTutorial(TutorialData tutorialData)
    {
        for (int row = 0; row < 4; row++)
        {
            for (int column = 0; column < 5; column++)
            {
                cells[row][column].gameObject.SetActive(false);
            }
        }
        foreach (var cellData in tutorialData.CellsData)
        {
            cells[cellData.X][cellData.Y].gameObject.SetActive(true);
            cells[cellData.X][cellData.Y].Init(cellData.Y, cellData.X, cellData.X == 3, cellData.N, (cellData.N>0?SoldierType.Normal:SoldierType.Bomber));
        }
    }
    
    public void ShowAllCells()
    {
        for (int row = 0; row < 4; row++)
        {
            for (int column = 0; column < 5; column++)
            {
                cells[row][column].gameObject.SetActive(true);
            }
        }
    }
    
    

    public void StartConnecting(SoldierCell cell)
    {
        if (CanConnect && GameManager.Instance.insideEnemies.Count == 0)
        {
	        isConnecting = true;
            currentPitch = 1;
	        int newCellValue = cell.currentSoldier.ValueNumber;
	        currentCellValue = newCellValue;
	        currentSumOfValues = newCellValue;
	        connectedCells.Add(cell);
	        connectingLine.positionCount = 2;
	        Vector3 cellPosition = cell.transform.position;
	        SetLineRendererPosition(0, cellPosition);
            cell.currentSoldier.ShowConnectingAnimation();
            cell.currentSoldier.SoldierCircle = true;
            VibrationManager.Instance.DoLightVibration();
            SoundManager.Instance.PlayByASComponent(Sound.SoldierConnet, cell.currentSoldier.gameObject, pitch: currentPitch);
    	}
    }

    public void ConnectCell(SoldierCell cell) {
        if (GameManager.Instance.insideEnemies.Count > 0) {
            CancelConnecting();
        }
        else if(isConnecting)
        {
            int tempCount = connectedCells.Count;
            Vector3 newPoint = cell.transform.position;
            if (connectedCells.Contains(cell))
            {
                if (tempCount > 1 && connectedCells[tempCount - 2] == cell)
                {
                    currentSumOfValues -= connectedCells[tempCount - 1].currentSoldier.ValueNumber;
                    currentCellValue = cell.currentSoldier.ValueNumber;
                    connectingLine.positionCount--;
                    connectedCells[tempCount - 1].currentSoldier.SoldierCircle = false;
                    connectedCells.RemoveAt(tempCount - 1);
                    Vector3 newPos = connectedCells[tempCount - 2].transform.position;
                    SetLineRendererPosition(tempCount - 2, newPos);
                    currentPitch -= pitchDifference;
                }
            }
            else
            {
                Vector3 lastPoint = connectedCells[tempCount - 1].transform.position;
                if (Mathf.Abs(newPoint.x - lastPoint.x) <= 1 && Mathf.Abs(newPoint.z - lastPoint.z) <= 1)
                {
                    int newCellValue = cell.currentSoldier.ValueNumber;
                    if (IsCorrectConnection(newCellValue))
                    {
                        currentCellValue = newCellValue;
                        currentSumOfValues += newCellValue;
                        SetLineRendererPosition(tempCount, newPoint);
                        connectedCells.Add(cell);
                        connectingLine.positionCount++;
                        cell.currentSoldier.ShowConnectingAnimation();
                        cell.currentSoldier.SoldierCircle = true;
                        VibrationManager.Instance.DoLightVibration();
                        currentPitch += pitchDifference;
                        SoundManager.Instance.PlayByASComponent(Sound.SoldierConnet, cell.currentSoldier.gameObject, pitch: currentPitch);
                    }
                }
            }
        }
    }

    private bool IsCorrectConnection(int newCellValue)
    {
        if (connectedCells.Count < 2 && newCellValue == currentCellValue)
            return true;
        else if (connectedCells.Count >= 2 && (newCellValue == currentCellValue || newCellValue == 2 * currentCellValue))
            return true;
        else
            return false;
    }

    public void FinishedConnecting()
    {
        if (!PlayerPrefsManager.SeenTutorial)
        {
            TutorialPath path = TutorialManager.Instance.Path;
            if (path != null)
            {
                bool isCorrectPath = true;

                if (path.Order.Count != connectedCells.Count)
                {
                    isCorrectPath = false;
                }
                else
                {
                    // for (int i = 0; i < path.Order.Count; i++)
                    // {
                    //     if (path.Order[i].x != connectedCells[i].Coordination.row ||
                    //         path.Order[i].y != connectedCells[i].Coordination.column)
                    //     {
                    //         isCorrectPath = false;
                    //     }
                    // }
                }

                if (!isCorrectPath)
                {
                    CancelConnecting();
                    VibrationManager.Instance.DoMediumVibration();
                    return;
                }
                else
                {
                    TutorialManager.Instance.End();
                }
            }
        }
        
        currentPitch = 1;
        if (connectedCells.Count <= 1 || GameManager.Instance.insideEnemies.Count > 0)
        {
            connectedCells[0].currentSoldier.SoldierCircle = false;
            CancelConnecting();
        }
        else
        {
            GameManager.Instance.CurrentTutorialIndex++;
            connectingLine.positionCount = 0;
            isConnecting = false;
            IsShifting = true;
            StartCoroutine(DoMergeVisuals());
            VibrationManager.Instance.DoMediumVibration();
        }
    }

    public void ShiftSoldiers()
    {
        int emptyLenght;
        for (int column = 0; column < cells[0].Count; column++)
        {
            emptyLenght = 0;
            for (int row = cells.Count - 1; row >= 0; row--)
            {
                if (cells[row][column].IsFull)
                {
                    if (emptyLenght > 0)
                    {
                        cells[row][column].currentSoldier.MoveSoldierToAnotherCell(cells[row + emptyLenght][column]);
                    }
                }
                else
                {
                    emptyLenght++;
                }
            }
            GenerateSoldier(cells, column, emptyLenght);
        }
    }

    private async Task GenerateSoldier(List<List<SoldierCell>> columnCells,int currentColumn,int emptyCells)
    {
        for (int k = emptyCells - 1; k >= 0; k--)
        {
            bool spawnBomber = Random.Range(0f, 1f)<GameManager.Instance.CurrentLevelData.BomberSoldierSpawnProbability;
            Soldier tempSoldier = (spawnBomber?BomberSoldierPool:NormalSoldierPool).Spawn(transform).GetComponent<Soldier>();
            tempSoldier.Type = spawnBomber ? SoldierType.Bomber : SoldierType.Normal;
            tempSoldier.transform.localPosition = new Vector3(-2 + currentColumn, 0, -2.5f);
            tempSoldier.ValueNumber = (int) Mathf.Pow(2, Utils.GetRandomPower
                (
                GameManager.Instance.CurrentLevelData.MinSoldierPower + currentPowerAddition,
                GameManager.Instance.CurrentLevelData.MaxSoldierPower + currentPowerAddition
                ));
            tempSoldier.IsShooter = false;
            tempSoldier.Init();
            tempSoldier.MoveSoldierToAnotherCell(columnCells[k][currentColumn]);
            await Task.Delay((int)(1000f / Database.GameConfiguration.SoldierSpeedNormal));
        }  
    }

    private IEnumerator BlinkLine(Action onEnd = null)
    {
        float dur = 1f;
        int n = 3;
        float dur2 = dur / n;

        Color color1 = connectingLine.startColor;
        Color color2 = connectingLine.endColor;

        connectingLine.startColor = Color.red;
        connectingLine.endColor = Color.red;
        yield return new WaitForSeconds(dur2);
        connectingLine.startColor = color1;
        connectingLine.endColor = color2;
        yield return new WaitForSeconds(dur2);
        connectingLine.startColor = Color.red;
        connectingLine.endColor = Color.red;
        yield return new WaitForSeconds(dur2);
        connectingLine.startColor = color1;
        connectingLine.endColor = color2;

        if (onEnd != null)
        {
            onEnd.Invoke();
        }
    }

    public void CancelConnecting()
    {
        connectingLine.positionCount = 0;
        foreach (SoldierCell cell in connectedCells)
            cell.currentSoldier.SoldierCircle = false;
        connectedCells.Clear();
        isConnecting = false;
    }

    private Vector3 GetPointerPosition()
    {
        float touchX = (Input.mousePosition.x / Screen.width) * Screen.width;
        float touchY = (Input.mousePosition.y / Screen.height) * Screen.height;

        Ray ray = Camera.main.ScreenPointToRay(new Vector3(touchX, touchY));

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            Vector3 tempPos = GetLineRendererPosition(connectingLine.positionCount - 2);
            if (Vector3.Distance(tempPos, hit.point) > Mathf.Sqrt(2))
                return (Vector3.Normalize(hit.point - tempPos) * Mathf.Sqrt(2)) + tempPos;
            else
                return hit.point;
        }
        return Vector3.zero;
    }

    public void SetLineRendererPosition(int index, Vector3 pos)
    {
        connectingLine.SetPosition(index, new Vector3(pos.x, pos.z, -Database.GameConfiguration.LineOffsetFromGround));
    }
    
    public Vector3 GetLineRendererPosition(int index)
    {
        Vector3 pos = connectingLine.GetPosition(index);
        pos.z = pos.y;
        pos.y = Database.GameConfiguration.LineOffsetFromGround;
        return pos;
    }
    
    public Vector3 GetLastLineRendererPosition()
    {
        return GetLineRendererPosition(connectingLine.positionCount-1);
    }

    public SoldierCell GetFirstCellInTheWay(Enemy enemy)
    {
        //Transform tempTransformParent = enemy.transform.parent;
        enemy.transform.parent = enteredEnemiesContainer;
        int x = Mathf.Clamp(Mathf.RoundToInt(enemy.transform.localPosition.x), 0, 4);
        int z = Mathf.Clamp(Mathf.FloorToInt(enemy.transform.localPosition.z),0 , 3);

        if (!SoldierCellMergeManager.Instance.IsShifting)
        {
            while (!cells[z][x].IsFull && z>0)
            {
                z = z - 1;
            }
            if (cells[z][x].IsFull)
            {
                return cells[z][x];
            }
            else
            {
                return null;
            }
        }
        else
        {
            return cells[z][x];
        }
    }
    
    public Vector3 GetEnemyBaseInTheWay(Enemy enemy)
    {
        //Transform tempTransformParent = enemy.transform.parent;
        enemy.transform.parent = enteredEnemiesContainer;
        int x = Mathf.Clamp(Mathf.RoundToInt(enemy.transform.localPosition.x), 0, 4);
        return soldierBases[x].position;
    }

    public void EndShifting()
    {
        bool allCellsFull = true;
        foreach (var cellRow in cells)
        {
            foreach (var cell in cellRow)
            {
                if(!cell.IsFull)
                    allCellsFull = false;
            }
        }

        if (allCellsFull)
        {
            if (PlayerPrefsManager.SeenTutorial)
            {
                GameManager.Instance.InitCurrentTutorial();
            }
            IsShifting = false;
        }
    }

    private IEnumerator DoMergeVisuals()
    {
        IsMerging = true;
        foreach (SoldierCell cell in connectedCells)
            cell.currentSoldier.SoldierCircleColor = Color.black;
        int tempCount = connectedCells.Count;
        float duration = Database.GameConfiguration.SoldiersMergeTime;
        if (tempCount >= 2)
        {
            Transform mergingSoldier = connectedCells[0].currentSoldier.transform;
            connectedCells[0].currentSoldier.SoldierCircleColor = Color.white;
            connectedCells[0].currentSoldier.SoldierCircle = false;
            Vector3 startPos = mergingSoldier.position;
            Vector3 toPosition = connectedCells[1].currentSoldier.transform.position;
            float counter = 0.0f;
            while (counter < duration)
            {
                counter += Time.deltaTime;
                mergingSoldier.position = Vector3.Lerp(startPos, toPosition, counter / duration);
                yield return null;
            }
            
            connectedCells[0].ClearCell();
            connectedCells.RemoveAt(0);
            SoundManager.Instance.Play(Sound.SoldierCombine);
            StartCoroutine(DoMergeVisuals());
        }
        else
        {
            currentSumOfValues = Mathf.ClosestPowerOfTwo(currentSumOfValues);
            connectedCells[0].currentSoldier.ValueNumber = currentSumOfValues;
            connectedCells[0].currentSoldier.SoldierCircleColor = Color.white;
            connectedCells[0].currentSoldier.SoldierCircle = false;
            SoundManager.Instance.Play(Sound.SoldierMerge);
            ParticleManager.Instance.PlayParticle(Particle_Type.SoldierMerge, connectedCells[0].transform.position+(Vector3.up*0.5f), Vector3.up);
            if (PlayerPrefsManager.SeenTutorial)
            {
                ShiftSoldiers();
            }
            else
            {
                GameManager.Instance.InitCurrentTutorial();
            }
            connectedCells.Clear();
            IsMerging = false;
            IsShifting = false;
        }
    }
}
