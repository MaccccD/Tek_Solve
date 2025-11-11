using UnityEngine;
using Mirror;
using System.Collections.Generic;
public class TurnSystem : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnTurnChanged))] // using the hook here allows us to immeditately sink the value istead of passing it under RPC. to the client
    public int currentPlayerTurn = 1;
    [SyncVar] public bool turnComplete = false;
    private UISystem visualSystem;
    private MovementSystem moveSystem;
    private PlayerInput playerInput;
    private int cachedLocalPlayerId = 0;
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
        visualSystem.turnSystemPanel.gameObject.SetActive(true);
        visualSystem.DeactivateTurnSystemPanel();
        visualSystem.turnSystemTxt.text = $"Player {currentPlayerTurn}'s Turn.";
        Debug.Log($"turn changed from {oldTurn} to {newTurn}");
        ApplyBlurEffect();
    }

    
    private void Start()
    {
        visualSystem = FindObjectOfType<UISystem>();
        moveSystem = FindObjectOfType<MovementSystem>();
        playerInput = FindObjectOfType<PlayerInput>();

        if (isClient)
        {
            Invoke(nameof(ApplyBlurEffect), 1f); // allow the client to also experience the blur effect
        }
    }

   [Server]
   public  void SwitchTurn()
    {
        currentPlayerTurn = currentPlayerTurn == 1 ? 2 : 1; // if player turn is 1 , switch to player 2 after 1 is done and so on .
        
        Debug.Log($"Applying blur - Current Turn: {currentPlayerTurn}, Local Player: {isLocalPlayer}");

        //Key Takeaway: Never read Sync Vars immediately after changing them in RPCs. Rather pass the values itslef as a paramater
        // RpcTurnChanged(currentPlayerTurn);

        //updating for the server 
        // visualSystem.turnSystemTxt.text = $"Player {currentPlayerTurn}'s Turn";


        // implement blur mechanic here
        // Debug.Log("the turn has changed!!!");  // for my own peace of mind
   }

   
   public void ApplyBlurEffect()
    {
        if(visualSystem == null) 
    {
        Debug.LogWarning("VisualSystem is null in ApplyBlurEffect");
        return;
    }

    int localPlayerID = GetLocalPlayerId();
    Debug.Log($"ApplyBlurEffect - Current Turn: {currentPlayerTurn}, Local Player: {localPlayerID}");

    if (localPlayerID == 0)
    {
        Debug.LogWarning("Local player ID not found yet");
        return;
    }

    if(currentPlayerTurn == 1)
    {
        if (localPlayerID == 1)
        {
            // Player 1's turn - Player 1 sees: own info clear, opponent info blurred
            SetBlurState(blurPlayer2: true, blurPlayer1: false);
        }
        else if(localPlayerID == 2)
        {
            // Player 1's turn - Player 2 sees: own info clear, opponent info blurred  
            SetBlurState(blurPlayer2: false, blurPlayer1: true);
        }
    }
    else if(currentPlayerTurn == 2)
    {
        if(localPlayerID == 1)
        {
            // Player 2's turn - Player 1 sees: own info clear, opponent info blurred
            SetBlurState(blurPlayer2: true, blurPlayer1: false);
        }
        else if(localPlayerID == 2)
        {
            // Player 2's turn - Player 2 sees: own info clear, opponent info blurred
            SetBlurState(blurPlayer2: false, blurPlayer1: true);
        }
    }


    }

    private int GetLocalPlayerId()
    {
        if (cachedLocalPlayerId != 0)
            return cachedLocalPlayerId;



        //otherwise find the local player:
        PlayerInput[] allPlayers = FindObjectsOfType<PlayerInput>(); // trying to get all player to determine which is local :


        foreach(PlayerInput player  in allPlayers)
        {
            if (player.isLocalPlayer && player.myPlayerID != 0) 
            {
                cachedLocalPlayerId = player.myPlayerID;
                Debug.Log($"Cached Local Player: {cachedLocalPlayerId}");
                return cachedLocalPlayerId;
            }
        }
        Debug.Log("Not cached local player found!");
        return 0;
    }

    public void SetBlurState(bool blurPlayer1, bool blurPlayer2)
    {
        Debug.Log($"SetBlurState - Blur P1: {blurPlayer1}, Blur P2: {blurPlayer2}");

        // Handle Player 1 blur panels and pieces
        if (visualSystem.player1BlurPanel != null)
        {
            visualSystem.player1BlurPanel.SetActive(blurPlayer1);
            Debug.Log($"Player 1 Blur Panel: {blurPlayer1}");
        }

        if (visualSystem.player1CodePanel != null)
        {
            visualSystem.player1CodePanel.SetActive(!blurPlayer1);
        }

        if (visualSystem.player1Piece != null)
        {
            SetPieceVisibility(visualSystem.player1Piece, !blurPlayer1);
        }

        // Handle Player 2 blur panels and pieces
        if (visualSystem.player2BlurPanel != null)
        {
            visualSystem.player2BlurPanel.SetActive(blurPlayer2);
            Debug.Log($"Player 2 Blur Panel: {blurPlayer2}");
        }

        if (visualSystem.player2CodePanel != null)
        {
            visualSystem.player2CodePanel.SetActive(!blurPlayer2);
        }

        if (visualSystem.player2Piece != null)
        {
            SetPieceVisibility(visualSystem.player2Piece, !blurPlayer2);
        }
    }


    private void SetPieceVisibility(GameObject piece, bool visible)
    {
        if (piece == null)
        {
            Debug.LogWarning("The player piece is missing here!");
            return;
        }

        SpriteRenderer renderer = piece.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            Color color = renderer.color;
            color.a = visible ? 1f : 0.3f;
            renderer.color = color;
            Debug.Log($"Piece {piece.name} alpha: {color.a}");
        }
        else
        {
            Debug.LogWarning($"No SpriteRenderer found on {piece.name}");
        }
    }


    [ClientRpc]
    void RpcTurnChanged(int newPlayer)
    {
        Debug.Log($"Turn has changed to player : {newPlayer}");
        //updating for the client
        visualSystem.turnSystemTxt.text = $"Player  {currentPlayerTurn}'s Turn";
        
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
