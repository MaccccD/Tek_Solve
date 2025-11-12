using UnityEngine;
using Mirror;
using System;
using System.Linq;
using System.Collections.Generic;

public class CodeSystem : NetworkBehaviour
{
    public static CodeSystem Instance { get; private set; }
    [SerializeField] private GridSystem gridSystem;
    [SerializeField] private RoundManagementSystem roundsSystem;
    [SerializeField] private UISystem visualSytem;
    public readonly SyncList<int> player1Code = new SyncList<int>(); // what the first player inputs
    public readonly SyncList<int> player2Code = new SyncList<int>();  // what the second player inputs
    [SyncVar] public int player1Progress = 0;
    [SyncVar] public int player2Progress = 0;


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
        roundsSystem = FindFirstObjectByType<RoundManagementSystem>();
        visualSytem = FindFirstObjectByType<UISystem>();
    }
    bool ValidateCode(SyncList<int> code)
    {
        return code.Sum() == gridSystem.targetNumber && code.Count == 4; //here i'm basically checking if the 4-digit code users have inputted equals to the target numbers they were given.
    }


    [Server]
    public void AddToCode(int playerID, int number)
    {
        var code = playerID == 1 ? player1Code : player2Code;
        code.Add(number); // here i'm allowing each player on their turn to add a number or a single digit of the 4-digit code required.
        Debug.Log("Add to code is working");

        //updating player progress:
        if(playerID == 1)
        {
            player1Progress = code.Count;
        }
        else if(playerID == 2)
        {
            player2Progress = code.Count;
        }
        //right after :
        RpcUpdateDigitDisplay(playerID, code.ToArray()); //update the digits being typed in on each player's turn;
        RpcUpdatePlayerProgress(playerID, code.Count, CalculateCurrentSum(code));

        // checking if the 4 digit code is complete:
        if (code.Count == 4)
        {
            CheckCodeSubmission(playerID, code);
            Debug.Log("The 4 digit code is being checked!");
        }
    }

    [Server]
    void CheckCodeSubmission(int playerID, SyncList<int> code)
    {
        int sum = CalculateCurrentSum(code);
        int target = gridSystem.targetNumber;
        if(sum == target)
        {
            Console.WriteLine("Yayy, someone cracked the code!");
            RpcCodeAccepted(playerID, code.ToList());
            visualSytem.correctCodeSound.Play();
            visualSytem.DeactivateAccepetedSound();
            // player who got it wins the round and the board state changes for the next round.
        }

        else
        {
            code.RemoveAt(3);// it's 3 bc a list starts from index 0 not 1 so then the 4th digit becomes index 3;
            if(playerID == 1)
            {
                player1Progress = 3; // remove 4th digit if code doesn't equal to target
            }
            else
            {
                player2Progress = 3;

                RpcCodeRejected(playerID, sum, target);
                //visualSytem.incorrectCodeSound.Play();
               // visualSytem.DeactivateRejectedCodeSound();
            }


            RpcUpdateDigitDisplay(playerID, code.ToArray()); //update the digits being typed in on each player.

        }
    }


    int CalculateCurrentSum(SyncList<int>code)
    {
        return code.Sum(); // so each time a player adds a new digits, it calculates the current sum of the numbers inputted via the keypad.
        

    }

    //getting the current player's code as they type it in on their turn(s):

    public List<int> GetPlayerCode(int playerID)
    {
        return playerID == 1 ? player1Code.ToList() : player2Code.ToList(); // herei'm basically returning the current player code as it is being stored in the list and retrieving it from there as it is typed.  
    }
    

    [Server]
    //resetting all codes when a new round starts :
    public void ResetCodes()
    {
        player1Code.Clear();
        player2Code.Clear();
        player1Progress = 0;
        player2Progress = 0;
        RpcClearDigitsDisplay(1);
        RpcClearDigitsDisplay(2);
        RpcResetUI();
        visualSytem.InitiateRound();
        Debug.Log("Codes resetted");
    }

    // all the client feedback from interactions within the server:

    [ClientRpc]

    void RpcCodeAccepted(int playerID, List<int> winningCode)
    {
        roundsSystem.PlayerWonRound(playerID);
        visualSytem.correctCodeSound.Play();
        visualSytem.DeactivateAccepetedSound();
        Debug.Log($"Player : {playerID} WON the round with code : {string.Join("+", winningCode)}  =  {winningCode.Sum()}");
    }

    [ClientRpc]
    void RpcCodeRejected(int playerID, int attemptedSum, int targetSum)
    {
        visualSytem.incorrectCodePanel.gameObject.SetActive(true);
        visualSytem.gameScreenPanel.gameObject.SetActive(false);
        visualSytem.P1CurrentSum.text = "Current Sum: ";
        visualSytem.P2CurrentSum.text = "Current Sum: ";
        visualSytem.p1NeedTxt.text = "Needs: ";
        visualSytem.p2NeedTxt.text = "Needs: ";
        //Time.timeScale = 0f; //pause the game!
        Debug.Log($"Player: {playerID} code has been REJECTED!. Got {attemptedSum}, and the correct sum is : {targetSum}");
        // trigger visual feedback such as a screen shake or error message
    }

    [ClientRpc]
    void RpcUpdatePlayerProgress(int playerID, int progress, int currentSum)
    {
        progress = gridSystem.targetNumber - currentSum; // the diff you need 

        if(playerID == 1)
        {
            visualSytem.P1CurrentSum.text = "Current Sum: " + currentSum.ToString();
            visualSytem.p1NeedTxt.text = "Needs: " + progress.ToString();
           
        }
        else if(playerID == 2)
        {
            visualSytem.P2CurrentSum.text ="Current Sum: " + currentSum.ToString();
            visualSytem.p2NeedTxt.text = "Needs: " + progress.ToString();

        }
        Debug.Log($"Player {playerID}, Progress : {progress}, Current Sum of numbers inputted: {currentSum}");
    }
    [ClientRpc]

    void RpcUpdateDigitDisplay(int playerID, int[] codeArray)
    {
        List<int> codeList = new List<int>(codeArray); // here im converting the list from an array back to the list
        visualSytem.UpdateDigitsDisplay(playerID, codeList);
        Debug.Log($"it works, look : {codeList}");
        

    }
    [ClientRpc]
    void RpcClearDigitsDisplay(int playerID)
    {
        visualSytem.ClearDisplays(playerID);
        Debug.Log("All player digits resetted back to ? bc the round restarted");
    }
    [ClientRpc]
    void RpcResetUI()
    {
        //clearing UI displays
        player1Code.Clear();
        player2Code.Clear();
        player1Progress = 0;
        player2Progress = 0;
        RpcClearDigitsDisplay(1); 
        RpcClearDigitsDisplay(2);
        visualSytem.InitiateRound();
        Debug.Log("all UI has been cleared !");
    }
}