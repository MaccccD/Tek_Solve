using UnityEngine;
using Mirror;

public class MovementSystem : NetworkBehaviour
{
    public enum MoveType { None, Adjacent, Diagonal } // the singleton pattern
    //player starting positions
    [SyncVar] public Vector2Int player1Position = new Vector2Int(1,1); 
    [SyncVar] public Vector2Int player2Position = new Vector2Int(1,1);
    // player pieces 
    [SyncVar] public GameObject player1Piece;
    [SyncVar] public GameObject player2Piece;
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
       // if (!isLocalPlayer) return;
        CmdMove(playerId, numpadKey);
        Debug.Log($"A key has been pressed: {playerId}, {numpadKey}");

        // Vector2Int direction = NumpadKeyDirection(numpadKey); // mapping the direction the player goes according to the diagonal / adjacent move types

    }

    [Command(requiresAuthority = false)]
    void CmdMove(int playerID, int numpadKey)
    {
        Vector2Int direction = NumpadKeyDirection(numpadKey); // mapping the direction the player goes according to the diagonal / adjacent move types
        if (direction == Vector2Int.zero)
        {
            RpcMoveRejected(playerID, "Invalid Numpad Key!!");
            return;
        }

        //grabbing the next required move:
        MoveType moveType = GetMoveType(direction);

        //current player's turn info:
        Vector2Int currentPos = playerID == 1 ? player1Position : player2Position;
        MoveType lastMove = playerID == 1 ? player1LastMove : player2LastMove;

        //the new position's move:
        Vector2Int newPos = currentPos + direction;

        //check if the move made by player is valid:
        if (!ValidateMove(playerID, newPos, moveType, lastMove))
        {
            return;
            
        }

        //then you make the move(where it will show now):
        ExecuteMove(playerID, newPos, moveType, numpadKey);
        Debug.Log("Yayy, the numpad key pressed on grid is the one being returned");

    }
    public Vector2Int GetPlayerPosition(int playerID) // getting the current position of a player:
    {
        return playerID == 1 ? player1Position : player2Position;
        
    }

    public MoveType GetRequiredMoveType(int playerID) // this method is the one that conrtols the last move systems where it keeps track of the fact that each player is not kaing the same move twice on their turn
    {
        MoveType lastMove = playerID == 1 ? player1LastMove : player2LastMove;
        Debug.Log($"The last move is being tracked on which player made the last move: {lastMove}");

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
            return MoveType.Adjacent; // make the move adjacent if i made a diagonal move initially
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
        if(newPos.x < 0 || newPos.x > 4 || newPos.y < 0 || newPos.y > 4)
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

    public void ExecuteMove(int playerID, Vector2Int newPos, MoveType moveType, int numpadKey)
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
        // Informing the  CodeSystem to add this number to player's code
        FindObjectOfType<CodeSystem>().AddToCode(playerID, gridNumber);

        // Notify all clients of successful move
        RpcMoveExecuted(playerID, newPos, gridNumber, moveType);

        //the switch player turn :
        // FindObjectOfType<TurnSystem>().SwitchTurn();

    }

    [ClientRpc]
     //return on the all the cleints:
    void RpcMoveRejected(int playerID, string reason)
      {
        Debug.LogWarning($"Player {playerID} move has been rejected because {reason}");
        //show error message here for UI purposes.
      }

    [ClientRpc]
    void RpcMoveExecuted(int playerID, Vector2Int newPos, int gridNumber, MoveType moveType)
    {
        Debug.Log($"Player {playerID} moved to {newPos} and collected number: {gridNumber}({moveType} move");
        //visual feedback of the move made with the player piece moving 
        UISystem visualSystem = FindObjectOfType<UISystem>();

       if(visualSystem != null)
        {
            visualSystem.UpdatePlayerPiecePositions(playerID, newPos);
            Debug.Log($"Player piece has moved accordingly. {playerID}, and the position {newPos}");
        }
    }

}
