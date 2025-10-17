using UnityEngine;
using Mirror;
public class TurnSystem : NetworkBehaviour
{
    [SyncVar] public int currentPlayerTurn = 1;
    [SyncVar] public bool turnComplete = false;

    public static TurnSystem Instance { get; private set; }
  
    private float maxTurnTime = 30f;


    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

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
   public  void SwitchTurn()
    {
        currentPlayerTurn = currentPlayerTurn == 1 ? 2 : 1; // if player turn is 1 , switch to player 2 after 1 is done and so on .
        RpcTurnChanged(currentPlayerTurn);
        Debug.Log("the turn has changed!!!"); // for my own peace of mind!
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
        currentPlayerTurn = 1;
        turnComplete = false;
        return;
    }
}
