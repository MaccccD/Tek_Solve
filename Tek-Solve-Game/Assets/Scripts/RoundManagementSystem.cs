using UnityEngine;
using Mirror;

public class RoundManagementSystem : NetworkBehaviour
{
    public static RoundManagementSystem Instance { get; private set; }
    [SyncVar] public int currentRound = 1;
    [SyncVar] public int maxRounds = 5;  // max rounds in one level.
    [SyncVar] public int player1Wins = 0;
    [SyncVar] public int player2Wins = 0;

    //references to the other scripts:
    private GridSystem gridSystem;
    private CodeSystem codeSystem;
    private MovementSystem movementSystem;
    private TurnSystem turnSystem;
    private UISystem visualSystem;


    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        gridSystem = FindFirstObjectByType<GridSystem>();
        codeSystem = FindFirstObjectByType<CodeSystem>();
        movementSystem = FindFirstObjectByType<MovementSystem>();
        turnSystem = FindFirstObjectByType<TurnSystem>();
        visualSystem = FindFirstObjectByType<UISystem>();
        Debug.Log("Yay, scripts found!!");
    }


    [Server]
    public void PlayerWonRound(int playerID)
    {
        if(playerID == 1) // if a player wins a round after-cracking the code :
        {
            player1Wins++;
        }
        else
        {
            player2Wins++;
        }

        RpcAnnounceRoundWinner(playerID, player1Wins, player2Wins);
        

        //checking if someone won the match:
        if(player1Wins > maxRounds / 2 || player2Wins > maxRounds / 2)
        {
            Invoke(nameof(EndMatch), 3f);
            Debug.Log("Yayy, match is over, we have a winner!");
        }
        else
        {
            //begin the next round after the winner is announced:
            Invoke(nameof(CmdStartNextRound), 8f); // setting a delay so that it deson't immedtealey got the nest round when everything is resetted w a new grid!
        }
    }
    public void Restart()
    {
        CmdStartNextRound();
    }

    [Command(requiresAuthority =false)]
    public void CmdStartNextRound()
    {
        StartNextRoundLogic();
    }
    [Server]
    public void StartNextRoundLogic()
    {
        currentRound++; // the number of rpunds will increment accordingly each time a new round starts
        //the grid change:
        bool changeGrid = currentRound > 2 && Random.Range(0f, 1f) < 0.4f; // change the grid if the rounds reset , generating a new grid of numbers within the 4x4 grod size
        //reset all systems :
        gridSystem.ResetRound(changeGrid);
        codeSystem.ResetCodes();
        
        //reset plauer positions and moves:
        movementSystem.player1Position = new Vector2Int(2, 1);//so make the start at the centre for both players. (need to test out);
        movementSystem.player2Position = new Vector2Int(2, 0);
        movementSystem.player1LastMove = MovementSystem.MoveType.None;// set the movement back to none bc they would need to make the first move, not have a predefined one already.
        movementSystem.player2LastMove = MovementSystem.MoveType.None;
        //reset turn :
        turnSystem.ResetTurn();

        //client start a new round:
        RpcStartNewRound(currentRound, changeGrid);

        Debug.Log($"SERVER: Started round {currentRound}");
    }


    [Server]
    void EndMatch()
    {
        int winner = player1Wins > player2Wins ? 1 : 2; // comparing the number of wins between the two player so that the winner wouild be the one with more number of wins
        RpcAnnounceMatchWinner(winner, player1Wins, player2Wins);
    }


   [ClientRpc]
    void RpcAnnounceRoundWinner(int playerID, int p1Wins, int p2Wins)
    {
        
        //handle player wins :
        player1Wins = p1Wins;
        player2Wins = p2Wins;

        //show round win:
        int roundWin = player1Wins > player2Wins ? 1 : 2;
        if (roundWin == 1)
        {
            visualSystem.DeactivateDelayTime();
            visualSystem.roundWinPanel.gameObject.SetActive(true);
            visualSystem.roundWinText.gameObject.SetActive(true);
            visualSystem.StarsIncrementing();
            visualSystem.DeactivateRoundWin();
        }
        if(roundWin == 2)
        {
            visualSystem.DeactivateDelayTime();
            visualSystem.roundWinPanel.gameObject.SetActive(true);
            visualSystem.roundWinText.gameObject.SetActive(true);
            visualSystem.StarsIncrementing();
            visualSystem.DeactivateRoundWin();
        }
        
     

        Debug.Log($"Player : {playerID} wins this round!! Score : P1= {p1Wins}, P2={p2Wins}");
    }

    [ClientRpc]
    public void ChangeGrid()
    {
        currentRound++; // the number of rpunds will increment accordingly each time a new round starts
        //the grid change:
        bool changeGrid = currentRound > 2 && Random.Range(0f, 1f) < 0.4f; // change the grid if the rounds reset , generating a new grid of numbers within the 4x4 grod size
        //reset all systems :
        gridSystem.ResetRound(changeGrid);
    }

    [ClientRpc]
    void RpcStartNewRound(int roundNum, bool gridChanged)
    {
        //clear the UI text:
        visualSystem.ClearPlayerTexts();
        //reset codes on client:
        codeSystem.ResetCodes();
        //update round num:
        currentRound = roundNum;
        Debug.Log($"A new round {roundNum}, has started. Grid has changed :{gridChanged}");

        //applu blue effect:
        if(turnSystem != null)
        {
            turnSystem.ApplyBlurEffect();
        }
        Debug.Log("a new round started and the client is up to date!");

    }
    
    [ClientRpc]
    void RpcAnnounceMatchWinner(int playerID, int p1Wins, int p2Wins)
    {
        //update the wins
        player1Wins = p1Wins;
        player2Wins = p2Wins;
        //show match win Ui
        visualSystem.matchWinPanel.gameObject.SetActive(true);
        visualSystem.matchWinText.gameObject.SetActive(true);
        visualSystem.DeactivateMatchWin();

        //show both players their player stats:
        visualSystem.statsPanel.gameObject.SetActive(true);
        visualSystem.player1WinsText.gameObject.SetActive(true);
        visualSystem.player2WinsText.gameObject.SetActive(true);
        visualSystem.RestartGame.gameObject.SetActive(true);
        visualSystem.ExitGame.gameObject.SetActive(true);

        Debug.Log($"Congratulations to {playerID}.!!! The scores are P1: {p1Wins} and P2: {p2Wins}");
    }

   


}
