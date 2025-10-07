using UnityEngine;
using System.Collections.Generic;
using Mirror;


public class GridSystem : NetworkBehaviour
{
    private int[,] gridNumbers = new int[4, 4]; // the actual 4 x 4 grid of numbers.
    [SyncVar] public int targetNumber;
    [SyncVar] private string gridData; // seriized grid for network sync so that when the size increases , it syncs between both players


    void Start()
    {
        if (isServer)
        {
            GenerateNewGrid();
            GenerateTargetNumber();
            Debug.Log("Yayy, the grid numbers abd the target number are being generated!!");
        }
    }

    [Server]
    public void GenerateNewGrid() // so creating te grid with the numbers generated randomly between 1 - 9
    {
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                gridNumbers[x, y] = Random.Range(1, 10); // numbers from 1 - 9.

            }
        }

        SerializeGrid(); // to sync across the network.

        RpcSyncGrid(gridData); // to tell the client of the grid that has been created and and synced to them 
    }

    [Server]
    void GenerateTargetNumber()
    {
        targetNumber = Random.Range(15, 36);// the target number wil be randomly genreated for each round but is between this range for the firt 5 rounds of the game.

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

    void DeserializeGrid(string data)
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


    [ClientRpc]
    void RpcSyncGrid(string data)
    {
        DeserializeGrid(data);
        Debug.LogWarning("grid has been synced to client!");
    }


    // this basically captures the numbers the player presses on the numpad :
    public int GetNumberAt(Vector2Int position)
    {
        // so if the number the player pressed is within the bounds of the grid size:
        if (position.x >= 0 && position.x < 4 && position.y >= 0 && position.y < 4)
        {
            return gridNumbers[position.x, position.y]; // so return the number at that grid position that the player clicked on
        }
        return 0;// if the player clicks the wrong key thats not part of the numpad
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
