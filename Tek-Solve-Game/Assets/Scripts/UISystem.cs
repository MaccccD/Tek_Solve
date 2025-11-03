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
     public Text turnSystemTxt;
     public Text incorrectCodeTxt;

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


    [Header("Game Audios")]
     private AudioSource incorrectCodeSound;
     private AudioSource correctCodeSound;
     private AudioSource backgroundMusic;


    [Header("Script References")]
     private RoundManagementSystem roundSystem;
     private GridSystem gridSystem;
     private MovementSystem movementSystem;
     private TurnSystem turnSystem;




    private void Start()
    {
        StartCoroutine(WaitForSystems());
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

    private IEnumerator WaitForSystems()
    {
        // Wait until systems are ready. Using "Instance" bc of the single-ton pattern that makes the script awake immediately when the game runs and makes the accessible globally.
        while (GridSystem.Instance == null ||
               MovementSystem.Instance == null ||
               TurnSystem.Instance == null ||
               RoundManagementSystem.Instance == null)
        {
            yield return new  WaitForSeconds(3f);
        }

        gridSystem = GridSystem.Instance; // applying the singleton pattern here so that all systems accessible
        movementSystem = MovementSystem.Instance;
        turnSystem = TurnSystem.Instance;
        roundSystem = RoundManagementSystem.Instance;

        Debug.Log("Yayyy, All Systems have been found!");

        yield return new WaitForSeconds(3f); // this is to allow the sync to occur and complete between the client and host

        DisplayGridNumbers(gridSystem.GetGrid());// showing the grid numbers on grid
        targetNumberTxt.text = gridSystem.targetNumber.ToString();
        Debug.Log($"Target number is showing on both the client and the host{targetNumberTxt}");
        
        InitiateRound();
        InitializePieces();
    }
    public void InitiateRound()
    {
       // roundSystem.StartNextRound();//references to the grid and all the movement positions.(server only )
        roundsNumberTxt.text = "Round Number: " +  roundSystem.currentRound.ToString();// show the number of rounds.
        targetNumberTxt.text = "Target Number: " + gridSystem.targetNumber.ToString();// show the target number.
        turnSystemTxt.text = turnSystem.currentPlayerTurn.ToString(); // to show players whose turn it is.
        lastMoveTxt.text = movementSystem.GetRequiredMoveType(1).ToString(); // the last move ui to return the opposite next move
        
    }

    public void InitializePieces()
    {
        player1Piece.SetActive(true);
        player2Piece.SetActive(true);

        //so both players start at the centre:
        Vector2Int startingPosition = new Vector2Int(1,1);
       

        UpdatePlayerPiecePositions(1, startingPosition);
        UpdatePlayerPiecePositions(2, startingPosition);

        Debug.Log("Player pieces intialized!");
    }
    public void DisplayGridNumbers(int[,] grid)
    {
        int index = 0;

        //converting the gid numbers to text:
        for(int x =0; x < 4; x++)
        {
            for(int y = 0; y < 4; y++)
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
        GameObject playerPiece = playerId == 1 ? player1Piece : player2Piece;

        if(playerPiece == null)
        {
            Debug.LogError("Player pieves have not been assigned in the inspector!");
            return;
        }

        //here i'm calculating the grid cell based on the 4 x 4 grid i have layout:
        int cellIndex = (gridPosition.y * 4) + gridPosition.x;

        if(cellIndex < 0 || cellIndex >= gridNumberTxts.Length)
        {
            Debug.LogError($"Invalid grid postion!{gridPosition}");
        }

        //the text comp at that grid position:
        Text targetCell = gridNumberTxts[cellIndex];

        if(targetCell == null)
        {
            Debug.LogError($"Grid cell {cellIndex} not found");
            return;
        }

        playerPiece.transform.position = targetCell.transform.position;// placing the piece at the centre of the grid cell:
        playerPiece.SetActive(true);

        Debug.Log($"Player {playerId} piece moved to grid position {gridPosition} (cell index {cellIndex}");

        //   RectTransform peiceRect = playerPiece.GetComponent<RectTransform>();
        //  RectTransform cellRect = targetCell.GetComponent<RectTransform>();

        //okay using the corner placement so that the grid number is not hidden:
        //   Vector3 cellPosition = targetCell.transform.position;

        //  float offsetX = cellRect.rect.width * 0.3f; //30% to the right
        //   float offsetY = cellRect.rect.height * 0.3f; //30% up

        //player 1 will be top right and then player 2 will be bottom right:
        //   if(playerId == 1)
        //   {
        //       playerPiece.transform.position = cellPosition + new Vector3(offsetX, offsetY, 0);
        //  }
        //   else // for player 2
        //  {
        //       playerPiece.transform.position = cellPosition + new Vector3(offsetX, -offsetY, 0);
        //   }

        //  playerPiece.SetActive(true);





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
    private void Update()
    {
        if (turnSystem != null) // update the text ui , not switching turns:
        {
            turnSystemTxt.text = $"Player {turnSystem.currentPlayerTurn}'s Turn";
            lastMoveTxt.text = movementSystem.GetRequiredMoveType(turnSystem.currentPlayerTurn).ToString();
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

        for( int i = 0; i < displays.Length; i++)
        {
            displays[i].text = "?";  // so rest to the "?" placeholder when the round resets 
        }
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

}
