using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class UISystem : MonoBehaviour
{
    [Header("Game Screen UI")]
     public Text targetNumberTxt;
     public Text lastMoveTxt;
     public Text roundsNumberTxt;
     public GameObject turnSystemPanel;
     public Text turnSystemTxt;
     public GameObject incorrectCodePanel;
     public GameObject gameScreenPanel;

    [Header("Player 1 UI")]
     public Text[] p1DigitsDisplay;
     public Text P1CurrentSum;
     public Text p1NeedTxt;

    [Header("Player 2 UI")]
     public Text[] p2DigitsDisplay;
     public Text P2CurrentSum;
     public Text p2NeedTxt;

    [Header("Player Victory UI")]
     public GameObject roundWinPanel;
     public GameObject[] stars;
     public Text roundWinText;
     public GameObject matchWinPanel;
     public Text matchWinText;
     public Image trophyImage;

    [Header("Player Stats")]
     public GameObject statsPanel;
     public Text player1WinsText;
     public Text player2WinsText;
     public Button RestartGame;
     public Button ExitGame;

    [Header("Grid System Info")]
     public Text[] gridNumberTxts;
     public GameObject gridPanel;

    [Header("Player Pieces")]
     public GameObject player1Piece;
     public GameObject player2Piece;
     public RectTransform gridPanell;

    [Header("Warning Msgs")]
    public Text outofBoundsTxt;

    [Header("Blur Mechanic Stuff")]
    public GameObject player1BlurPanel;    // Blurs Player 2's view of P1 info
    public GameObject player2BlurPanel;    // Blurs Player 1's view of P2 info
    public GameObject player1CodePanel;    // Reference to Player 1's code display
    public GameObject player2CodePanel;    // Reference to Player 2's code display
   


    [Header("Game Audios")]
     private AudioSource incorrectCodeSound;
     private AudioSource correctCodeSound;
     private AudioSource backgroundMusic;


    [Header("Script References")]
     private RoundManagementSystem roundSystem;
     private GridSystem gridSystem;
    private CodeSystem codeSystem;
     private MovementSystem movementSystem;
     private TurnSystem turnSystem;




    private void Start()
    {
        StartCoroutine(WaitForSystems());
        Debug.Log($"UISystem Start - gridNumberTxts length: {gridNumberTxts?.Length}");
        Debug.Log($"Player1 piece: {player1Piece != null}, Player2 piece: {player2Piece != null}");

        if (gridNumberTxts == null || gridNumberTxts.Length == 0)
        {
            Debug.LogError("gridNumberTxts array is not assigned in inspector!");
        }
        Debug.Log($"CLIENT: UISystem Start - isClient: {true}");
        Debug.Log($"CLIENT: gridNumberTxts length: {gridNumberTxts?.Length}");
        Debug.Log($"CLIENT: Player1 piece: {player1Piece != null}, Player2 piece: {player2Piece != null}");

        if (player1Piece == null || player2Piece == null)
        {
            Debug.LogError("CLIENT: Player piece references are missing in inspector!");
        }

        //Key insight: using 'findfirstobjectbytype' in her overwrites what was asisgned in the inspector if it was declared as a public variable.
        // roundSystem = FindObjectOfType<RoundManagementSystem>();
        //  gridSystem = FindObjectOfType<GridSystem>();
        //  movementSystem = FindObjectOfType<MovementSystem>();
        //  turnSystem = FindObjectOfType<TurnSystem>();

        // Checking if they are found:
        //   if (roundSystem == null) Debug.LogError("RoundManagementSystem not found!");
        //   if (gridSystem == null) Debug.LogError("GridSystem not found!");
        //   if (movementSystem == null) Debug.LogError("MovementSystem not found!");
        //   if (turnSystem == null) Debug.LogError("TurnSystem not found!");

        //   Debug.Log("yayy, scripts found");
        //   InitiateRound();
        //    //console output for my own peace of mind:
        //   Debug.Log("Initialization successful");
    }

    public void DebugGridNumberTxtsPositions()
    {
        Debug.Log("=== gridNumberTxts Array Order ===");
        for (int i = 0; i < gridNumberTxts.Length; i++)
        {
            if (gridNumberTxts[i] != null)
            {
                Debug.Log($"Index {i}: {gridNumberTxts[i].name} - Text: '{gridNumberTxts[i].text}' - Position: {gridNumberTxts[i].transform.position}");
            }
        }
        Debug.Log("=== End of Array ===");
    }

    private IEnumerator WaitForSystems()
    {
        // Wait until systems are ready. Using "Instance" bc of the single-ton pattern that makes the script awake immediately when the game runs and makes the accessible globally.
        while (GridSystem.Instance == null ||
               MovementSystem.Instance == null ||
               TurnSystem.Instance == null ||
               CodeSystem.Instance == null ||
               RoundManagementSystem.Instance == null)
        {
            yield return null; //don't wait 3 secs, instread one frame
        } 

        gridSystem = GridSystem.Instance; // applying the singleton pattern here so that all systems accessible
        movementSystem = MovementSystem.Instance;
        turnSystem = TurnSystem.Instance;
        codeSystem = CodeSystem.Instance;
        roundSystem = RoundManagementSystem.Instance;

        Debug.Log("Yayyy, All Systems have been found!");

        yield return new WaitForSeconds(1f); // this is to allow the sync to occur and complete between the client and host

        DisplayGridNumbers(gridSystem.GetGrid());// showing the grid numbers on grid
        targetNumberTxt.text = gridSystem.targetNumber.ToString();
        Debug.Log($"Target number is showing on both the client and the host{targetNumberTxt}");
       
        InitiateRound();
       
    }
    public void InitiateRound()
    {
       // roundSystem.StartNextRound();//references to the grid and all the movement positions.(server only )
        roundsNumberTxt.text = "Round Number: " +  roundSystem.currentRound.ToString();// show the number of rounds.
        targetNumberTxt.text = "Target Number: " + gridSystem.targetNumber.ToString();// show the target number.
       // turnSystemPanel.gameObject.SetActive(true);
      //  DeactivateTurnSystemPanel();
      //  turnSystemTxt.text = $"Player  {turnSystem.currentPlayerTurn}'s Turn";


    }

    public void TriggerTurnSystem()
    {
        turnSystemPanel.gameObject.SetActive(true);
        DeactivateTurnSystemPanel();
        turnSystemTxt.text = $"Player  {turnSystem.currentPlayerTurn}'s Turn";
    }

    public void DisplayGridNumbers(int[,] grid)
    {
        int index = 0;

        //converting the grid numbers to text:
        for(int y = 0; y < 4; y++)
        {
            for(int x = 0; x < 4; x++)
            {
               if(index < gridNumberTxts.Length)
                {
                    gridNumberTxts[index].text = grid[x,y].ToString(); // so here i'm making the grid numbers into strings that can be displayed as texts
                    index++;


                }
            }
            
        }

        if(gridPanel == null)
        {
            gridPanel.gameObject.SetActive(true); // making the pannels that holds all the numbers accessible
            Debug.Log("Panel is accessible");
        }
    }

    public void UpdatePlayerPiecePositions(int playerId, Vector2Int gridPosition)
    {
        Debug.Log($"=== CLIENT: UpdatePlayerPiecePositions - Player: {playerId}, GridPos: {gridPosition} ===");

        GameObject playerPiece = playerId == 1 ? player1Piece : player2Piece;

        if (playerPiece == null)
        {
            Debug.LogError("CLIENT: Player pieces have not been assigned in the inspector!");
            return;
        }

        Debug.Log($"CLIENT: Player piece found: {playerPiece.name}, Active: {playerPiece.activeInHierarchy}");

        // Column-major calculation
        int cellIndex = (gridPosition.x * 4) + gridPosition.y;
        Debug.Log($"CLIENT: Calculation - GridPos {gridPosition} → CellIndex {cellIndex}");

        if (cellIndex < 0 || cellIndex >= gridNumberTxts.Length)
        {
            Debug.LogError($"CLIENT: Invalid grid position! {gridPosition} -> cellIndex: {cellIndex}, Array length: {gridNumberTxts.Length}");
            return;
        }

        Text targetCell = gridNumberTxts[cellIndex];
        if (targetCell == null)
        {
            Debug.LogError($"CLIENT: Grid cell {cellIndex} not found");
            return;
        }

        Debug.Log($"CLIENT: Target cell found: {targetCell.name}, Text: '{targetCell.text}', Position: {targetCell.transform.position}");
        Debug.Log($"CLIENT: Player piece current position: {playerPiece.transform.position}");

        // Update the position
        playerPiece.transform.position = targetCell.transform.position;
        playerPiece.SetActive(true);

        Debug.Log($"CLIENT: Player piece NEW position: {playerPiece.transform.position}");
        Debug.Log($"=== CLIENT: Update COMPLETE ===");
    }

    public void HideOpponentPiece(int localPlayerID)
    {
        int opponentID = localPlayerID == 1 ? 2 : 1;
        GameObject opponentpiece = opponentID == 1 ? player1Piece : player2Piece; ;

        if(opponentpiece != null)
        {
            opponentpiece.SetActive(false);
        }
    }
  

    public void UpdateDigitsDisplay(int playerID, List<int> code)
    {
        Text[] displays = playerID == 1 ? p1DigitsDisplay : p2DigitsDisplay; // change between the digits display of both player 1 and 2 depending whose turn it is

        // clear any existing :
        for (int i = 0; i < displays.Length; i++)
        {
            if( i < code.Count)
            {
                displays[i].text = code[i].ToString(); // so here i'm showing the digit being pressed by the player whose turn is currently on
            }
            else
            {
                displays[i].text = "?"; // the placeholder Q for numbers not added yet
            }
        }
    }
    public void  StarsIncrementing()
    {
        for(int i = 0; i < stars.Length; i++)
        {
            stars[0].gameObject.SetActive(true);// try this first bc i think stars[i].gameObject.setActive(true) will set every star active.
            Debug.Log("A starr has been activated!");
            
        }
    }
    public void ClearDisplays(int playerID)
    {
        Text[] displays = playerID == 1 ? p1DigitsDisplay : p2DigitsDisplay;

        for (int i = 0; i < displays.Length; i++)
        {
            displays[i].text = "?";  // so rest to the "?" placeholder when the round resets
        }
    }

    public void ClearPlayerTexts()
    {
        P1CurrentSum.text = "";
        p1NeedTxt.text = "";
        P2CurrentSum.text = "";
        p2NeedTxt.text = "";
    }

    public void DeactivateTurnSystemPanel()
    {
        StartCoroutine(TurnSystemPanelDelay(3f));
        return;
    }

    private IEnumerator TurnSystemPanelDelay(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        turnSystemPanel.gameObject.SetActive(false);

    }


    public void  DeactivateRoundWin()
    {
        StartCoroutine(RoundWinDelay(5f));
        return;
    }


    private IEnumerator RoundWinDelay(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        roundWinPanel.gameObject.SetActive(false);
        roundWinText.gameObject.SetActive(false);
        
    }

    public void DeactivateMatchWin()
    {
        StartCoroutine(MatchWinDelay(5f));
        return;
    }

    private IEnumerator MatchWinDelay(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        matchWinPanel.gameObject.SetActive(false);
        matchWinText.gameObject.SetActive(false);

    }

    public void DeactivateRejectedCodeSound()
    {
        StartCoroutine(RejectedSoundDelay(5f));
        return;
    }

    private IEnumerator RejectedSoundDelay(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        incorrectCodeSound.Pause();
        Debug.Log("Incorrect sound has stppped playing!");
    }

    public void DeactivateAccepetedSound ()
    {
        StartCoroutine(AcceptedSoundDelay(5f));
        return;
    }


    private IEnumerator AcceptedSoundDelay( float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        correctCodeSound.Pause();
        Debug.Log("correct sound has stopped playing !");
    }

    public void DisableWarningText()
    {
        StartCoroutine(OutOfBoundsTxt(5f));
        return;
    }

    private IEnumerator OutOfBoundsTxt(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
     
        outofBoundsTxt.gameObject.SetActive(false);
    }
    public void Restart()
    {
        codeSystem.ResetCodes();
    }

}
