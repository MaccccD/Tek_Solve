using UnityEngine;
using System.Collections.Generic;
using Mirror;


public class GridSystem : NetworkBehaviour
{
    public static GridSystem Instance { get; private set; } // singleton pattern here.
    private int[,] gridNumbers = new int[4, 4]; // the actual 4 x 4 grid of numbers.

    [SyncVar(hook =nameof(OnTargetNumberChanged))]
    public int targetNumber;


    [SyncVar] private string gridData; // serialized grid for network sync so that when the size increases , it syncs between both players
    private UISystem visualSystem;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this; // Singleton Pattern to ensure that the script ies evoked when the game runs
        }
    }
    public override void OnStartClient()
    {
        base.OnStartClient();

        // For late joiners - sync the current grid
        if (gridData != null && gridData.Length > 0)
        {
            DeserializeGrid(gridData);
           // Debug.Log('okay grid has been synced to the client as well!');
            UpdateGridUI();
        }
    }
    void Start()
    {
        visualSystem = FindObjectOfType<UISystem>();

        Debug.Log("UI system found by the grid system script!");


        if (isServer)
        {
            GenerateNewGrid();
            GenerateTargetNumber();
            Debug.Log("Yayy, the grid numbers and the target number are being generated!!");
        }
    }


    void OnTargetNumberChanged(int oldValue, int newValue)
    {
        if(visualSystem != null)
        {
            visualSystem.targetNumberTxt.text ="Target Number: " +  newValue.ToString();
        }
        
    }

    [Server]
    public void GenerateNewGrid() // so creating the grid with the numbers generated randomly between 1 - 9
    {
        Debug.Log(" GENERATING NEW GRID:");
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                gridNumbers[x, y] = Random.Range(1, 10); // numbers from 1 - 9.
                Debug.Log($"Set gridNumbers[{x}, {y}] = {gridNumbers[x, y]}");

            }
        }

        SerializeGrid(); // to sync across the network.

        GenerateTargetNumber();

        DebugGridComparison();

        UpdateGridUI();// so update the numbers for the host first

        DeserializeGrid(gridData);
        //RpcSyncGrid(); // to tell the client of the grid that has been created and and synced to them 
    }

    [Server]
    void GenerateTargetNumber()
    {
        targetNumber = Random.Range(15, 36);// the target number wil be randomly generated for each round but is between this range for the firt 5 rounds of the game.

        if(Random.Range(0f,1f) < 0.3f)
        {
            targetNumber += Random.Range(5, 10);
        }

    }
    void SerializeGrid() // this is for the sync of the grid generated so that both host and client get the generation of the same grid 
    {
        List<int> flatGrid = new List<int>();
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                {
                    flatGrid.Add(gridNumbers[x, y]);
                }
            }

            gridData = string.Join(",", flatGrid); // here i'm joing the flat grid via the string that will be synced across all clients
        }
    }

    void DeserializeGrid(string data)// this method converts the seialized grod that has een synced across the network into a readable object.
    {
        string[] values = data.Split(",");
        int index = 0;

        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                gridNumbers[x, y] = int.Parse(values[index]);
                index++; // here  i'm doing the opposite of adding the grid numbers to the flat grid. here im returnig the numbers as parsed decimal values for each number within the range .
            }
        }
    }

    void UpdateGridUI()
    {
        if(visualSystem != null)
        {
            visualSystem.DisplayGridNumbers(gridNumbers);
            visualSystem.targetNumberTxt.text = "Target Number: " + targetNumber.ToString();
        }
    }

    [ClientRpc]
    void RpcSyncGrid()
    {
        visualSystem.DisplayGridNumbers(gridNumbers);
       
        Debug.LogWarning("grid has been synced to client!");

        UpdateGridUI(); // update the UI for clients
    }


    // this basically captures the numbers the player presses on the numpad :
    public int GetNumberAt(Vector2Int position)
    {
        Debug.Log($"GetNumberAt - Position: {position}");

        if (position.x >= 0 && position.x < 4 && position.y >= 0 && position.y < 4)
        {
            int retrievedNumber = gridNumbers[position.y, position.x];

            // ADD THESE DEBUG LINES:
            Debug.Log($"Grid Access: gridNumbers[{position.x}, {position.y}] = {retrievedNumber}");

            return retrievedNumber;
        }

        Debug.LogError($" GetNumberAt - Invalid position: {position}");
        return 0;
    }
    public void DebugGridComparison()
    {
        Debug.Log("=== GRID COMPARISON: Data vs Visual ===");

        for (int y = 0; y < 4; y++)
        {
            string dataRow = "";
            string visualRow = "";

            for (int x = 0; x < 4; x++)
            {
                // Data from gridNumbers
                dataRow += gridNumbers[x, y] + " ";

                // Visual from gridNumberTxts (using your column-major calculation)
                int cellIndex = (x * 4) + y;
                if (cellIndex < visualSystem.gridNumberTxts.Length && visualSystem.gridNumberTxts[cellIndex] != null)
                {
                    visualRow += visualSystem.gridNumberTxts[cellIndex].text + " ";
                }
                else
                {
                    visualRow += "?";
                }
            }

            Debug.Log($"Row {y} - Data:  {dataRow}");
            Debug.Log($"Row {y} - Visual: {visualRow}");
        }
        Debug.Log("=== END COMPARISON ===");
    }

    public int[,] GetGrid()
    {
        return gridNumbers; // for UI purposes and display
    }

    [Server]
    public void ResetRound(bool changeGrid = false) //when one of the players cracks the code, the grid numbers will re-arrange randomly again and players get a new target number.
    {
        if (changeGrid) 
        {
            GenerateNewGrid();
        }
        GenerateTargetNumber();
        Debug.Log("Round resetted w newly arranged numbers and the a new target number!!");
    }
}
