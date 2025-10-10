using UnityEngine;
using Mirror;
public class TurnSystem : NetworkBehaviour
{
    [SyncVar] public int CurrentPlayerTurn = 1;
    [SyncVar] public bool turnComplete = false;


    private float maxTurnTime = 30f;


    private void Update()
    {
        if (!isServer)
        {
            return;
        }
        if (turnComplete)
        {
            SwitchTurn();
        }

        
    }


    [Server]
    void SwitchTurn()
    {
        CurrentPlayerTurn = CurrentPlayerTurn == 1 ? 2 : 1; // if player turn is 1 , switch to player 2 after 1 is done and so on .
        RpcTurnChanged(CurrentPlayerTurn);
        Debug.Log("the turn has changed!!!");
    }

    [ClientRpc]
    void RpcTurnChanged(int newPlayer)
    {
        Debug.Log($"Turn has changed to player : {newPlayer}");
        turnComplete = true;
        //trigger blur effect for the other player's screen
    }

    [Server]
    public void ResetTurn()
    {
        CurrentPlayerTurn = 1;
        turnComplete = false;
        return;
    }
}
