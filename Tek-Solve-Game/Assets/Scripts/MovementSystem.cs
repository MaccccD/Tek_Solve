using UnityEngine;
using Mirror;

public class MovementSystem : NetworkBehaviour
{
    public enum MoveType { None, Adjacent, Diagonal } // the singleton pattern
    //player starting positions
    [SyncVar] public Vector2Int player1Position = new Vector2Int(2,1); 
    [SyncVar] public Vector2Int player2Position = new Vector2Int(2,2);
    // player pieces 
    public GameObject player1Piece;
    public GameObject player2Piece;
    //player starting moves:
    [SyncVar] public MoveType player1LastMove = MoveType.None; // player can make an starting move they want
    [SyncVar] public MoveType player2LastMove = MoveType.None; // same as above
    public static MovementSystem Instance { get; private set; }
   
    //the reference to the grid :
    private GridSystem gridSystem;
    private CodeSystem codeSystem;
    private UISystem visualSystem;
    private TurnSystem turnSystem;

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
        visualSystem = FindObjectOfType<UISystem>();
        codeSystem = FindObjectOfType<CodeSystem>();
        turnSystem = FindObjectOfType<TurnSystem>();

        //setting the player piece postions:
        if (isServer)
        {
            player1Piece.SetActive(true);
            player2Piece.SetActive(true);

            player1Position = new Vector2Int(2, 1);
            player2Position = new Vector2Int(2, 2);

            RpcInitializePlayerPieces(player1Position, player2Position);

            Debug.Log("Player pieces have been initialized in Movement Systems!");
        }
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
        ExecuteMove(playerID, newPos, moveType);
        Debug.Log("Yayy, the numpad key pressed on grid is the one being returned");

    }
    public Vector2Int GetPlayerPosition(int playerID) // getting the current position of a player:
    {
        return playerID == 1 ? player1Position : player2Position;
        
    }

    public MoveType GetRequiredMoveType(int playerID) // this method is the one that controls the last move systems where it keeps track of the fact that each player is not kaing the same move twice on their turn
    {
        MoveType lastMove = playerID == 1 ? player1LastMove : player2LastMove;
        Debug.Log($"The last move is being tracked on which player made the last move: {lastMove}");

      //  visualSystem.lastMoveTxt.text = lastMove.ToString();

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
           7 => new Vector2Int(-1, -1),  // Diagonal-Up-Left (grid: left + up)
           8 => new Vector2Int(0, -1),   // Up (grid: up)
           9 => new Vector2Int(1, -1),   // Diagonal-Up-Right (grid: right + up)
           4 => new Vector2Int(-1, 0),   // Left (grid: left)
           6 => new Vector2Int(1, 0),    // Right (grid: right)
           1 => new Vector2Int(-1, 1),   // Diagonal-Down-Left (grid: left + down)
           2 => new Vector2Int(0, 1),    // Down (grid: down)
           3 => new Vector2Int(1, 1),    // Diagonal-Down-Right (grid: right + down)
            _ => Vector2Int.zero
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
        if (newPos.x < 0 || newPos.x > 3 || newPos.y < 0 || newPos.y > 3)
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

        // Calculate NEXT player before switching
        int nextPlayer = playerID == 1 ? 2 : 1;
        MoveType nextRequiredMove = GetRequiredMoveType(nextPlayer);
        
        //add to code:
        codeSystem.AddToCode(playerID, gridNumber);
        //  Debug.Log($"The grid number should be the one registered now {gridNumber}");

        // Visual feedback
        RpcMoveExecuted(playerID, newPos, gridNumber, moveType,nextPlayer,nextRequiredMove);

        Debug.Log("okay this works!");

        //the switch player turn :
        turnSystem.SwitchTurn();

        
      

    }

    [ClientRpc]
     //return on the all the clients:
    void RpcMoveRejected(int playerID, string reason)
      {
        visualSystem.outofBoundsTxt.gameObject.SetActive(true);
        visualSystem.DisableWarningText();
        Debug.LogWarning($"Player {playerID} move has been rejected because {reason}");
     
      }

    [ClientRpc]
    void RpcInitializePlayerPieces(Vector2Int p1Start, Vector2Int p2Start)
    {
        player1Piece.SetActive(true);
        player2Piece.SetActive(true);

        visualSystem.UpdatePlayerPiecePositions(1, p1Start);
        visualSystem.UpdatePlayerPiecePositions(2, p2Start);
    }

    [ClientRpc]
    void RpcMoveExecuted(int playerID, Vector2Int newPos, int gridNumber, MoveType moveType, int nextPlayer, MoveType nextRequiredMove)
    {
        Debug.Log($"Player {playerID} moved to {newPos} and collected number: {gridNumber}({moveType} move");
        //the place piece moves where the grid number is:
        visualSystem.UpdatePlayerPiecePositions(playerID, newPos);

        //here i'm just updating the next move based on the current player's turn
       

        if(nextRequiredMove != MoveType.None)
        {
            visualSystem.lastMoveTxt.text = nextRequiredMove.ToString(); ;
        }
        else
        {
            visualSystem.lastMoveTxt.text = "Any";
        }

        Debug.Log("Okay so the move type should work now bc you're seeing this!");
       

    }

}
