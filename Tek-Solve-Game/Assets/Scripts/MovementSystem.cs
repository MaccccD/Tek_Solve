using UnityEngine;
using Mirror;

public class MovementSystem : NetworkBehaviour
{
    public enum MoveType { None, Adjacent, Diagonal }
    //player starting positions
    [SyncVar] public Vector2Int player1Position = new Vector2Int(1, 1); 
    [SyncVar] public Vector2Int player2Position = new Vector2Int(1, 1);
    //player starting moves:
    [SyncVar] public MoveType player1Move = MoveType.None; // player can make an startring move they want
    [SyncVar] public MoveType player2Move = MoveType.None; // same as above.
    //the refrence to the grid :
    private GridSystem gridSystem;

    private void Start()
    {
        gridSystem = FindObjectOfType<GridSystem>(); // grabbing the grid system
    }


    public void AttemptMove(int playerId, int numpadKey)
    {
        Vector2Int direction = NumpadKeyDirection(numpadKey); // mapping the direction the player goes according to the diagonal/adjacent move types
    }



    Vector2Int NumpadKeyDirection(int key)
    {
        return key switch // in here i'm using Vector2 Int to map out  the numpad keys.
        {
            7 => new Vector2Int(-1, 1),  // Diagonal-Up-Left
            8 => new Vector2Int(0, 1),   // Up
            9 => new Vector2Int(1, 1),   // Diagonal-Up-Right
            4 => new Vector2Int(-1, 0),  // Left
            6 => new Vector2Int(1, 0),   // Right
            1 => new Vector2Int(-1, -1), // Diagonal-Down-Left
            2 => new Vector2Int(0, -1),  // Down
            3 => new Vector2Int(1, -1),  // Diagonal-Down-Right
            _ => Vector2Int.zero // any invalid key press.
        };
    }

    MoveType GetMoveType(Vector2Int direction)
    {
        //check if move made is adjacent. this is only horizontal and vertical
        if(direction.x == 0 || direction.y == 0)
        {
            return MoveType.Adjacent;
        }

        else
        {
            //when the move is diagonal, all the axes changes and thus by default:
            return MoveType.Diagonal;
        }

        bool ValidateMove(int playerID, Vector2Int newPos,MoveType moveType, MoveType lastMove)
        {
            // checking the grid boundaries first:
            if(newPos.x < 0 || newPos.x > 3 || newPos.y < 0 || newPos.y > 3)
            {
                RpcMoveRejected(playerID, $"Move Out of bounds!");
                return false;
            }


            //checking if the person mkaes two moves that are the same and they don't alternate
            if(lastMove != MoveType.None && moveType == lastMove)
            {
                string moveTypeName = moveType == MoveType.Adjacent ? "adjacent" : "diagonal"; // conditional rendering here where the move type is initially adjacent or else it becomes diagonal if its not the first.
                RpcMoveRejected(playerID, $"You cannot make two {moveTypeName} in a row that are same! Must alternate.");
                return false;
            }

            return true;
        }


        [ClientRpc]
        //return on the all the cleints:
        void RpcMoveRejected(int playerID, string reason)
        {
            Debug.LogWarning($"Player {playerID} move has been rejected because {reason}");
            //show error message here for UI purposes.
        }
    }

}
