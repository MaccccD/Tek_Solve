using UnityEngine;
using Mirror;
public class TurnSystem : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnTurnChanged))] // using the hook here allows us to immeditately sink the value istead of passing it under RPC. to the client
    public int currentPlayerTurn = 1;
    [SyncVar] public bool turnComplete = false;
    private UISystem visualSystem;
    private MovementSystem moveSystem;
    public static TurnSystem Instance { get; private set; }
  
  //  private float maxTurnTime = 30f;



    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    void OnTurnChanged(int oldTurn, int newTurn)
    {
        //when turn changes :
        visualSystem.turnSystemTxt.text = $"Player{currentPlayerTurn}'s Turn";
        Debug.Log($"turn changed from {oldTurn} to {newTurn}");
    }

    
    private void Start()
    {
        visualSystem = FindObjectOfType<UISystem>();
        moveSystem = FindObjectOfType<MovementSystem>();
    }

    [Server]
   public  void SwitchTurn()
    {

        currentPlayerTurn = currentPlayerTurn == 1 ? 2 : 1; // if player turn is 1 , switch to player 2 after 1 is done and so on .

        //Key Takeaway: Never read Sync Vars immediately after changing them in RPCs. Rather pass the values itslef as a paramater
       // RpcTurnChanged(currentPlayerTurn);

        //updating for the server 
       // visualSystem.turnSystemTxt.text = $"Player {currentPlayerTurn}'s Turn";

        
        // implement blur mechanic
       // Debug.Log("the turn has changed!!!");  // for my own peace of mind
    }

    [ClientRpc]
    void RpcTurnChanged(int newPlayer)
    {
        Debug.Log($"Turn has changed to player : {newPlayer}");
        //updating for the client
        visualSystem.turnSystemTxt.text = $"Player {currentPlayerTurn}'s Turn";
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
