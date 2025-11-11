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
            Invoke(nameof(ApplyBlurEffect), 0.5f); // allow the client to also expeir e the blur effect
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

    private void ApplyBlurEffect()
    {
        if(visualSystem == null) return ;

        int localPlayerID = GetLocalPlayerId();

        if(currentPlayerTurn == 1)
        {
            //on player 1's turn, hide the player 2's panel and player piece.
            if (localPlayerID == 1)
            {
                SetBlurState(true, false); //on player 1, blur player 2 info
            }
            else if(localPlayerID == 2)
            {
                SetBlurState(false, true); //on player 2, blur player 1 info 
            }
        }
        else if(currentPlayerTurn == 2)
        {
            if(localPlayerID == 1)
            {
                SetBlurState(false, true);// on player 2 turn, blur player 1 info
            }
            else if(localPlayerID == 2)
            {
                SetBlurState(true, false); // on player 2 , blur player 2 info
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

    private void SetBlurState(bool blurOpponentCode, bool blurOpponentPiece)
    {
        //blur opponent panel code :
        if (visualSystem.player1CodePanel != null)
        {
            visualSystem.player1CodePanel.SetActive(!blurOpponentCode);
        }
        if (visualSystem.player2CodePanel != null)
        {
            visualSystem.player2CodePanel.SetActive(!blurOpponentCode);
        }

        //blur opponent player piece
        if(visualSystem.player1Piece != null)
        {
            SetPieceVisibility(visualSystem.player1Piece, !blurOpponentPiece);
        }
        if(visualSystem.player2Piece != null)
        {
            SetPieceVisibility(visualSystem.player2Piece, !blurOpponentPiece);
        }

        //enable or disbale player blur panel overlays:
        if(visualSystem.player1BlurPanel != null)
        {
            visualSystem.player1BlurPanel.SetActive(blurOpponentCode); //so enable the blur or make it show.
        }
        if(visualSystem.player2BlurPanel != null)
        {
            visualSystem.player2BlurPanel.SetActive(blurOpponentCode);
        }
    }


    private void SetPieceVisibility(GameObject piece, bool visible)
    {
        if(piece == null)
        {
            Debug.LogWarning("The player piece is missing here!");
            return;
        }

        if(piece != null)
        {
            SpriteRenderer renderer = piece.GetComponent<SpriteRenderer>();

            if(renderer != null)
            {
                Color color = renderer.color; // setting the colour of the piece to being transpararent:
                color.a = visible ? 1f : 0.3f; // so decrese the alpha value  by lowering the opacity:
                renderer.color = color;
            }
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
