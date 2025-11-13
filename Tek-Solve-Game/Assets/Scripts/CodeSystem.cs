using UnityEngine;
using Mirror;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

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

        // Calculate current sum
        int currentSum = CalculateCurrentSum(code);
        //right after :
        RpcUpdateDigitDisplay(playerID, code.ToArray()); //update the digits being typed in on each player's turn;
        RpcUpdatePlayerProgress(playerID, code.Count, currentSum);

        int targetNumber = gridSystem.targetNumber;

        if(currentSum >= targetNumber && code.Count < 4)
        {
            Debug.Log($"Player {playerID} reached target {gridSystem.targetNumber} with only {code.Count} digits!");
            RpcShowExceedingPanel(playerID, currentSum, code.Count);
            return; // Don't check for win yet
        }

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

            // SERVER handles the round transition
            roundsSystem.PlayerWonRound(playerID); // ← THIS IS THE KEY LINE!
            // player who got it wins the round and the board state changes for the next round.
        }
        else if (sum > gridSystem.targetNumber)
        {
            // Player exceeded target with 4 digits
            Debug.Log($"Player {playerID} exceeded target with {sum}!");
            RpcShowExceedingPanel(playerID, sum, 4);
        }
        else
        {
            RpcCodeRejected(playerID, sum, target);
           
            //update the digits display:
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
      //  player1Progress = 0;
       // player2Progress = 0;
        visualSytem.P1CurrentSum.text = "Current Sum: ";
        visualSytem.P2CurrentSum.text = "Current Sum: ";
        visualSytem.p1NeedTxt.text = "Remaining: ";
        visualSytem.p2NeedTxt.text = "Remaining: ";
        RpcClearDigitsDisplay(1);
        RpcClearDigitsDisplay(2);
        RpcResetUI();
     //   visualSytem.InitiateRound();
        Debug.Log("Codes resetted");
    }

    // all the client feedback from interactions within the server:

    [ClientRpc]
    void RpcCodeAccepted(int playerID, List<int> winningCode)
    {
        visualSytem.correctCodeSound.Play();
        visualSytem.DeactivateAccepetedSound();
        Debug.Log($"Player : {playerID} WON the round with code : {string.Join("+", winningCode)}  =  {winningCode.Sum()}");
    }

    [ClientRpc]
    void RpcShowExceedingPanel(int playerID, int currentSum, int digitCount)
    {
        if(visualSytem.exceedingTargetPanel != null)
        {
            visualSytem.exceedingTargetPanel.gameObject.SetActive(true);

        }
        visualSytem.DisableExceedingTargetPanel();

    }
    private int pendingClearPlayerID = 0;

    [ClientRpc]
    void RpcCodeRejected(int playerID, int attemptedSum, int targetSum)
    {
        visualSytem.incorrectCodePanel.gameObject.SetActive(true);
        visualSytem.gameScreenPanel.gameObject.SetActive(false);
        visualSytem.incorrectCodeSound.Play();
        visualSytem.DeactivateRejectedCodeSound();
        
        pendingClearPlayerID = playerID;
        //clear ui
        visualSytem.P1CurrentSum.text = "Current Sum: ";
        visualSytem.P2CurrentSum.text = "Current Sum: ";
        visualSytem.p1NeedTxt.text = "Remaining: ";
        visualSytem.p2NeedTxt.text = "Remaining: ";

        // Auto-continue after 2 seconds
        StartCoroutine(AutoContinueRound());
        //Time.timeScale = 0f; //pause the game!
        Debug.Log($"Player: {playerID} code has been REJECTED!. Got {attemptedSum}, and the correct sum is : {targetSum}");
        // trigger visual feedback such as a screen shake or error message
    }
    private IEnumerator AutoContinueRound()
    {
        yield return new WaitForSeconds(2f);

        visualSytem.incorrectCodePanel.gameObject.SetActive(false);
        visualSytem.gameScreenPanel.gameObject.SetActive(true);
        
        // ✅ Call Command to clear code on server
        CmdClearPlayerCode(pendingClearPlayerID);
    }
    [Command(requiresAuthority = false)]
    void CmdClearPlayerCode(int playerID)
    {
        if (playerID == 1)
        {
            player1Code.Clear();
            player1Progress = 0;
        }
        else
        {
            player2Code.Clear();
            player2Progress = 0;
        }

        // Update UI on all clients
        RpcClearPlayerCodeUI(playerID);

        Debug.Log($"SERVER: Player {playerID} code cleared after rejection");
    }

    [ClientRpc]
    void RpcClearPlayerCodeUI(int playerID)
    {
        // Clear visual displays
        visualSytem.ClearDisplays(playerID);

        // Reset progress text
        if (playerID == 1)
        {
            visualSytem.P1CurrentSum.text = "Current Sum: ";
            visualSytem.p1NeedTxt.text = "Remaining: ";
        }
        else
        {
            visualSytem.P2CurrentSum.text = "Current Sum: ";
            visualSytem.p2NeedTxt.text = "Remaining: ";
        }

        Debug.Log($"CLIENT: Player {playerID} reset and ready for new attempt");
    }


    [ClientRpc]
    void RpcUpdatePlayerProgress(int playerID, int progress, int currentSum)
    {
        progress = gridSystem.targetNumber - currentSum; // the diff you need 


        if(playerID == 1)
        {
            visualSytem.P1CurrentSum.text = "Current Sum: " + currentSum.ToString();
            visualSytem.p1NeedTxt.text = "Remaining: " + progress.ToString();
           
        }
        else if(playerID == 2)
        {
            visualSytem.P2CurrentSum.text ="Current Sum: " + currentSum.ToString();
            visualSytem.p2NeedTxt.text = "Remaining: " + progress.ToString();

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
        visualSytem.P1CurrentSum.text = "Current Sum: ";
        visualSytem.P2CurrentSum.text = "Current Sum: ";
        visualSytem.p1NeedTxt.text = "Remaining: ";
        visualSytem.p2NeedTxt.text = "Remaining: ";
        RpcClearDigitsDisplay(1);
        RpcClearDigitsDisplay(2);
        Debug.Log("all UI has been cleared !");
    }
}