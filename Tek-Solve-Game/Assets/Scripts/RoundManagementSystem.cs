using UnityEngine;
using Mirror;

public class RoundManagementSystem : NetworkBehaviour
{
    [SyncVar] public int currentRound = 1;
    [SyncVar] public int maxRounds = 5;  // max rounds in one level.
    [SyncVar] public int player1Wins = 0;
    [SyncVar] public int player2Wins = 0;

    //references to the other scripys:
    private GridSystem gridSystem;
    private CodeSystem codeSystem;
    private MovementSystem movementSystem;
    private TurnSystem turnSystem;


    private void Start()
    {
        gridSystem = FindObjectOfType<GridSystem>();
        codeSystem = FindObjectOfType<CodeSystem>();
        movementSystem = FindObjectOfType<MovementSystem>();
        turnSystem = FindObjectOfType<TurnSystem>();
    }


    [Server]
    public void PlayerWonRound(int playerID)
    {
        if(playerID == 1) // if a player wins a round after cracking the code :
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
    }

    [Server]
    void StartNextRound()
    {
        currentRound++;
        //the grid change:
        bool changeGrid = currentRound > 2 && Random.Range(0f, 1f) < 0.4f;
        //reset all systems :
        gridSystem.ResetRound(changeGrid);
        

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
        //Show visual winning screen 
        Debug.Log($"Player : {playerID} wins this round!! Score : P1= {p1Wins}, P2={p2Wins}");
    }

    [ClientRpc]
    void RpcStartNewRound(int roundNum, bool gridChanged)
    {
        //show feedback when a new  round starts as well as the number
        Debug.Log($"A new round {roundNum}, has started. Grid has chenged :{gridChanged}");
    }
    
    [ClientRpc]
    void RpcAnnounceMatchWinner(int playerID, int p1Wins, int p2Wins)
    {
        // match victory screen , stats and replay:
        Debug.Log($"Congratulations to {playerID}.!!! The scores are P1: {p1Wins} and P2: {p2Wins}");
    }


}
