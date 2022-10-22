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

    [SerializeField] private LineRenderer connectingLine;
    [SerializeField] private Transform soldiersCellContainer;
    [SerializeField] public ObjectPool NormalSoldierPool;
    [SerializeField] public ObjectPool BomberSoldierPool;
    [SerializeField] private TextMeshProUGUI sumOfValuesText;

    private List<List<SoldierCell>> cells;
    private List<SoldierCell> connectedCells = new List<SoldierCell>();
    private int currentCellValue, currentSumOfValues;
    private bool isConnecting = false;
    private GameObject sumOfValuesUI;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    void Start()
    {
        sumOfValuesUI = sumOfValuesText.transform.parent.gameObject;
        Init();
    }

    private void Update()
    {
        if (isConnecting)
        {
            Vector3 pointerPosition = GetPointerPosition();
            if(pointerPosition!=Vector3.zero)
                SetLineRendererposition(connectingLine.positionCount - 1, pointerPosition);
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
                cell.transform.localPosition = new Vector3(column + -2f, 0.1f, row + -1.5f);
                cell.Init(row == 3, GameManager.Instance.CurrentLevelData.GetCellNumber(column, 3-row), SoldierType.Normal);
                cells[row].Add(cell);

                childIndex++;
            }
        }
    }

    public void StartConnecting(SoldierCell cell)
    {
        isConnecting = true;
        int newCellValue = cell.currentSoldier.ValueNumber;
        currentCellValue = newCellValue;
        currentSumOfValues = newCellValue;
        UpdateSumOfValuesUI(Mathf.ClosestPowerOfTwo(currentSumOfValues));
        connectedCells.Add(cell);
        connectingLine.positionCount = 2;
        Vector3 cellPosition = cell.transform.position;
        SetLineRendererposition(0, cellPosition);
    }

    public void ConnectCell(SoldierCell cell) {
        if (isConnecting)
        {
            int tempCount = connectedCells.Count;
            Vector3 newPoint = cell.transform.position;
            if (connectedCells.Contains(cell))
            {
                if (tempCount > 1 && connectedCells[tempCount - 2] == cell)
                {
                    currentSumOfValues -= connectedCells[tempCount - 1].currentSoldier.ValueNumber;
                    UpdateSumOfValuesUI(Mathf.ClosestPowerOfTwo(currentSumOfValues));
                    currentCellValue = cell.currentSoldier.ValueNumber;
                    connectingLine.positionCount--;
                    connectedCells.RemoveAt(tempCount - 1);
                    Vector3 newPos = connectedCells[tempCount - 2].transform.position;
                    SetLineRendererposition(tempCount - 2, newPos);
                }
            }
            else
            {
                Vector3 lastPoint = connectedCells[tempCount - 1].transform.position;
                if (Mathf.Abs(newPoint.x - lastPoint.x) <= 1 && Mathf.Abs(newPoint.z - lastPoint.z) <= 1)
                {
                    int newCellValue = cell.currentSoldier.ValueNumber;
                    if (CorrectConnection(newCellValue))
                    {
                        currentCellValue = newCellValue;
                        currentSumOfValues += newCellValue;
                        UpdateSumOfValuesUI(Mathf.ClosestPowerOfTwo(currentSumOfValues));
                        SetLineRendererposition(tempCount, newPoint);
                        connectedCells.Add(cell);
                        connectingLine.positionCount++;
                    }
                }
            }
        }
    }

    private bool CorrectConnection(int newCellValue)
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
        connectingLine.positionCount = 0;
        for (int i = 0; i < connectedCells.Count - 1; i++)
            connectedCells[i].ClearCell();

        currentSumOfValues = Mathf.ClosestPowerOfTwo(currentSumOfValues);
        UpdateSumOfValuesUI(0);
        connectedCells[connectedCells.Count - 1].currentSoldier.ValueNumber = currentSumOfValues;
        ShiftSoldiers(cells);
        connectedCells.Clear();
        isConnecting = false;
    }

    private void ShiftSoldiers(List<List<SoldierCell>> cellCol)
    {
        int emptyLenght;
        for (int column = 0; column < cells[0].Count; column++)
        {
            emptyLenght = 0;
            for (int row = cellCol.Count - 1; row >= 0; row--)
            {
                if (cellCol[row][column].IsFull)
                {
                    if (emptyLenght > 0)
                        cellCol[row][column].currentSoldier.MoveSoldierToAnotherCell(cellCol[row + emptyLenght][column]);
                }
                else
                    emptyLenght++;
            }
            GenerateSoldier(cells, column, emptyLenght);
        }
    }

    private void UpdateSumOfValuesUI(int value) {
        if(value > 0)
        {
            sumOfValuesText.text = value.ToString();
            sumOfValuesUI.SetActive(true);
        }else
            sumOfValuesUI.SetActive(false);
    }

    private async Task GenerateSoldier(List<List<SoldierCell>> columnCells,int currentColumn,int emptyCells)
    {
        for (int k = emptyCells - 1; k >= 0; k--)
        {
            bool spawnBomber = false;
            
            
            Soldier tempSoldier = (spawnBomber?BomberSoldierPool:NormalSoldierPool).Spawn(transform).GetComponent<Soldier>();
            tempSoldier.Type = spawnBomber ? SoldierType.Bomber : SoldierType.Normal;
            tempSoldier.transform.localPosition = new Vector3(-2 + currentColumn, 0, -2.5f);
            tempSoldier.ValueNumber = (int) Mathf.Pow(2, Utils.GetRandomPower(1, 6));
            tempSoldier.Type = SoldierType.Normal;
            tempSoldier.MoveSoldierToAnotherCell(columnCells[k][currentColumn]);
            await Task.Delay(1000);
        }  
    }

    private Vector3 GetPointerPosition()
    {
        float touchX = (Input.mousePosition.x / Screen.width) * Screen.width;
        float touchY = (Input.mousePosition.y / Screen.height) * Screen.height;

        Ray ray = Camera.main.ScreenPointToRay(new Vector3(touchX, touchY));

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            Vector3 tempPos = GetLineRendererposition(connectingLine.positionCount - 2);
            if (Vector3.Distance(tempPos, hit.point) > Mathf.Sqrt(2))
                return (Vector3.Normalize(hit.point - tempPos) * Mathf.Sqrt(2)) + tempPos;
            else
                return hit.point;
        }
        return Vector3.zero;
    }

    public void SetLineRendererposition(int index, Vector3 pos)
    {
        connectingLine.SetPosition(index, new Vector3(pos.x, pos.z, -Database.GameConfiguration.LineOffsetFromGround));
    }
    
    public Vector3 GetLineRendererposition(int index)
    {
        Vector3 pos = connectingLine.GetPosition(index);
        pos.z = pos.y;
        pos.y = Database.GameConfiguration.LineOffsetFromGround;
        return pos;
    }
}
