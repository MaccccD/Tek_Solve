using UnityEngine;
using Mirror;

public class MovementSystem : NetworkBehaviour
{
    public enum MoveType { None, Adjacent, Diagonal }
    //player starting positions
    [SyncVar] public Vector2Int player1Position = new Vector2Int(1,1); 
    [SyncVar] public Vector2Int player2Position = new Vector2Int(1,1);
    //player starting moves:
    [SyncVar] public MoveType player1LastMove = MoveType.None; // player can make an starting move they want
    [SyncVar] public MoveType player2LastMove = MoveType.None; // same as above
    public static MovementSystem Instance { get; private set; }
   
    //the reference to the grid :
    private GridSystem gridSystem;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        gridSystem = FindObjectOfType<GridSystem>(); // grabbing the grid system;
       
    }


    public void AttemptMove(int playerId, int numpadKey)
    {
        Vector2Int direction = NumpadKeyDirection(numpadKey); // mapping the direction the player goes according to the diagonal / adjacent move types
     
    }

    public Vector2Int GetPlayerPosition(int playerID) // getting the current position of a player:
    {
        return playerID == 1 ? player1Position : player2Position;
        
    }

    public MoveType GetRequiredMoveType(int playerID) // this method is the one that conrtols the last move systems where it keeps track of the fact that each player is not kaing the same move twice on their turn
    {
        MoveType lastMove = playerID == 1 ? player1LastMove : player2LastMove;

        if(lastMove == MoveType.None)
        {
            return MoveType.None;
        }
        else if (lastMove == MoveType.Adjacent)
        {
            return MoveType.Diagonal; // make the move the opposite.
        }
        else
        {
            return MoveType.Adjacent; // make the move adjacent
        }
    }

    Vector2Int NumpadKeyDirection(int key)
    {
        return key switch // in here i'm using Vector2 Int to map out  the numpad keys.
        {
            7 => new Vector2Int(-1,1),  // Diagonal-Up-Left
            8 => new Vector2Int(0,1),   // Up
            9 => new Vector2Int(1,1),   // Diagonal-Up-Right
            4 => new Vector2Int(-1,0),  // Left
            6 => new Vector2Int(1, 0),   // Right
            1 => new Vector2Int(-1,-1), // Diagonal-Down-Left
            2 => new Vector2Int(0,-1),  // Down
            3 => new Vector2Int(1,-1),  // Diagonal-Down-Right
            _ => Vector2Int.zero // any invalid key press.
        };
    }

    MoveType GetMoveType(Vector2Int direction)
    {
        //check if move made is adjacent. this is only horizontal and vertical
        if (direction.x == 0 || direction.y == 0)
        {
            return MoveType.Adjacent;
        }

        else
        {
            //when the move is diagonal, all the axes changes and thus by default:
            return MoveType.Diagonal;
        }
    }
    public bool ValidateMove(int playerID, Vector2Int newPos,MoveType moveType, MoveType lastMove)
    {
      // checking the grid boundaries first:( the 4 x 4 grid);
        if(newPos.x < 0 || newPos.x > 3 || newPos.y < 0 || newPos.y > 3)
         {
           RpcMoveRejected(playerID, $"Move Out of bounds!");
          
           return false;
         }


       //checking if the person makes two moves that are the same and they don't alternate
        if(lastMove != MoveType.None && moveType == lastMove)
         {
          string moveTypeName = moveType == MoveType.Adjacent ? "adjacent" : "diagonal"; // conditional rendering here where the move type is initially adjacent or else it becomes diagonal if its not the first.
          RpcMoveRejected(playerID, $"You cannot make two {moveTypeName} in a row that are same! Must alternate.");
          return false;
          }

          return true;
        }

    public void ExecuteMove(int playerID, Vector2Int newPos, MoveType moveType)
    {
        //keeping track of each player's move:
        if (playerID == 1)
        {
            player1Position = newPos;
            player1LastMove = moveType;

        }
        else
        {
            player2Position = newPos;
            player2LastMove = moveType;
        }

        //getting the number at this grid pos:
         int gridNumber = gridSystem.GetNumberAt(newPos);
        // Notify all clients of successful move
        RpcMoveExecuted(playerID, newPos, gridNumber, moveType);
        // Informing the  CodeSystem to add this number to player's code
         FindObjectOfType<CodeSystem>().AddToCode(playerID, gridNumber);
        }

    [ClientRpc]
     //return on the all the cleints:
    void RpcMoveRejected(int playerID, string reason)
      {
        Debug.LogWarning($"Player {playerID} move has been rejected because {reason}");
        //show error message here for UI purposes.
      }

    [ClientRpc]
    void RpcMoveExecuted(int playerID, Vector2Int newPos, int number, MoveType moveType)
    {
        Debug.LogWarning($"Player {playerID} moved to {newPos} and chose the number : {number}");
        //visual feedback of the move made with the player piece moving 
    }

}
