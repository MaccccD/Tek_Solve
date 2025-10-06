using UnityEngine;
using Mirror;

public class MovementSystem : NetworkBehaviour
{
    [SyncVar] public Vector2Int player1Position;
    [SyncVar] public Vector2Int player2Position;
    [SyncVar] public MoveType lastMoveType;


    [Command]

    void CmdMove(int playerID, Vector2Int direction)
    {
        if(ValidateMove(playerID, direction))
        {
            ExecuteMove(playerID, direction);
           
        }
    }
}
