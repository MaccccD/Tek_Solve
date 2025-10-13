using UnityEngine;
using Mirror;

public class RoundManagementSystem : NetworkBehaviour
{
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

        

        //checking if someone won the round:
        if(player1Wins > maxRounds / 2 || player2Wins > maxRounds / 2)
        {
            EndMatch();
            Debug.Log("Yayy, match is over, we have a winner!");
        }
        else
        {
            //begin the next round after the winner is announced:
            Invoke(nameof(StartNextRound), 8f); // setting a delay so that it deson't immedtealey got the nest round when everything is resetted w a new grid!
        }
    }

    [Server]
    public void StartNextRound()
    {
        currentRound++; // the numbe rof rpunds will increment accordingly each time a new round starts
        //the grid change:
        bool changeGrid = currentRound > 2 && Random.Range(0f, 1f) < 0.4f; // change the grid if the rounds reset , generating a new grid of numbers within the 4x4 grod size
        //reset all systems :
        gridSystem.ResetRound(changeGrid);
        codeSystem.ResetCodes();
        player1Wins = 0;
        player2Wins = 0;
        movementSystem.player1Position = new Vector2Int(1, 1);//so make the start at the centre for both players. (need to test out);
        movementSystem.player2Position = new Vector2Int(1, 1);
        movementSystem.player1LastMove = MovementSystem.MoveType.None;// set the movement back to none bc they would need to make the first move, not have a predefined one already.
        movementSystem.player2LastMove = MovementSystem.MoveType.None;
        turnSystem.ResetTurn();
        RpcStartNewRound(currentRound, changeGrid);
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
        visualSystem.roundWinPanel.gameObject.SetActive(true);
        visualSystem.roundWinText.gameObject.SetActive(true);
        visualSystem.DeactivateRoundWin();
        Debug.Log($"Player : {playerID} wins this round!! Score : P1= {p1Wins}, P2={p2Wins}");
    }

    [ClientRpc]
    void RpcStartNewRound(int roundNum, bool gridChanged)
    {
       //ui feedback already taken cared off
        Debug.Log($"A new round {roundNum}, has started. Grid has chenged :{gridChanged}");
    }
    
    [ClientRpc]
    void RpcAnnounceMatchWinner(int playerID, int p1Wins, int p2Wins)
    {
        visualSystem.matchWinPanel.gameObject.SetActive(true);
        visualSystem.matchWinText.gameObject.SetActive(true);
        visualSystem.DeactivateMatchWin();
        Debug.Log($"Congratulations to {playerID}.!!! The scores are P1: {p1Wins} and P2: {p2Wins}");
    }


}
