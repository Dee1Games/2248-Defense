using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

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
    private int currentCellValue, currentSumOfValues;
    private bool isConnecting = false;
    
    public bool CanConnect => !IsShifting;

    public bool IsShifting = false;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
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
                cell.Init(column, row, row == 3, cellNumber, (cellNumber>0?SoldierType.Normal:SoldierType.Bomber));
                cells[row].Add(cell);
                childIndex++;
            }
        }
    }

    public void StartConnecting(SoldierCell cell)
    {
        if (CanConnect && GameManager.Instance.insideEnemies.Count == 0)
        {
	        isConnecting = true;
	        int newCellValue = cell.currentSoldier.ValueNumber;
	        currentCellValue = newCellValue;
	        currentSumOfValues = newCellValue;
	        //UpdateSumOfValuesUI(Mathf.ClosestPowerOfTwo(currentSumOfValues));
	        connectedCells.Add(cell);
	        connectingLine.positionCount = 2;
	        Vector3 cellPosition = cell.transform.position;
	        SetLineRendererPosition(0, cellPosition);
            cell.currentSoldier.ShowConnectingAnimation();
            VibrationManager.Instance.DoLightVibration();
    	}
    }

    public void ConnectCell(SoldierCell cell) {
        if (GameManager.Instance.insideEnemies.Count > 0) {
            CancelConnecting();
        }
        else if (isConnecting)
        {
            int tempCount = connectedCells.Count;
            Vector3 newPoint = cell.transform.position;
            if (connectedCells.Contains(cell))
            {
                if (tempCount > 1 && connectedCells[tempCount - 2] == cell)
                {
                    currentSumOfValues -= connectedCells[tempCount - 1].currentSoldier.ValueNumber;
                    //UpdateSumOfValuesUI(Mathf.ClosestPowerOfTwo(currentSumOfValues));
                    currentCellValue = cell.currentSoldier.ValueNumber;
                    connectingLine.positionCount--;
                    connectedCells.RemoveAt(tempCount - 1);
                    Vector3 newPos = connectedCells[tempCount - 2].transform.position;
                    SetLineRendererPosition(tempCount - 2, newPos);
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
                        //UpdateSumOfValuesUI(Mathf.ClosestPowerOfTwo(currentSumOfValues));
                        SetLineRendererPosition(tempCount, newPoint);
                        connectedCells.Add(cell);
                        connectingLine.positionCount++;
                        cell.currentSoldier.ShowConnectingAnimation();
                        VibrationManager.Instance.DoLightVibration();
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
        if (GameManager.Instance.insideEnemies.Count > 0)
        {
            CancelConnecting();
        }
        else
        {
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
            tempSoldier.ValueNumber = (int) Mathf.Pow(2, Utils.GetRandomPower(GameManager.Instance.CurrentLevelData.MinSoldierPower, GameManager.Instance.CurrentLevelData.MaxSoldierPower));
            tempSoldier.IsShooter = false;
            tempSoldier.Init();
            tempSoldier.MoveSoldierToAnotherCell(columnCells[k][currentColumn]);
            await Task.Delay(1000);
        }  
    }

    public void CancelConnecting()
    {
        connectingLine.positionCount = 0;
        //UpdateSumOfValuesUI(0);
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
        
        if(allCellsFull)
            IsShifting = false;
    }

    private IEnumerator DoMergeVisuals()
    {
        int tempCount = connectedCells.Count;
        float duration = Database.GameConfiguration.SoldiersMergeTime;
        if (tempCount >= 2)
        {
            Transform mergingSoldier = connectedCells[0].currentSoldier.transform;
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
            StartCoroutine(DoMergeVisuals());
        }
        else
        {
            currentSumOfValues = Mathf.ClosestPowerOfTwo(currentSumOfValues);
            connectedCells[0].currentSoldier.ValueNumber = currentSumOfValues;
            ShiftSoldiers();
            connectedCells.Clear();
        }
    }
}
