using UnityEngine;
using Mirror;
public class MovementValidationSystem : NetworkBehaviour
{
    public enum MoveType  {Adjacent, Diagonal }; // determines the last move system logic entral to the emergent gameplay
    [SyncVar] public MoveType player1Move;
    [SyncVar] public MoveType player2Move;
   

    [Command]
    void  CmdMove(int playerID, int numpadKey )
    {
        Vector2Int direction = NumpadKeyDirection(numpadKey); // mapping the direction the player goes according to the diagonal /adjacent move types
       // MoveType moveType = GetMoveType(direction);
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
            _ => Vector2Int.zero 
        };
    }
}
