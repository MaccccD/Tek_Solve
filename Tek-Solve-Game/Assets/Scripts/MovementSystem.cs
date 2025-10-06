using UnityEngine;
using Mirror;

public class MovementSystem : NetworkBehaviour
{
    [SyncVar] public Vector2Int player1Position;
    [SyncVar] public Vector2Int player2Position;
    public enum MoveType { Adjacent, Diagonal}


    [Command]

    void CmdMove(int playerID, Vector2Int direction)
    {
       // needs to track the player ;
    }
}
