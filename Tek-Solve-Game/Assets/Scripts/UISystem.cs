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
            yield return new  WaitForSeconds(2f);
        }

        gridSystem = GridSystem.Instance; // applying the singleton pattern here so that all systems accessible
        movementSystem = MovementSystem.Instance;
        turnSystem = TurnSystem.Instance;
        roundSystem = RoundManagementSystem.Instance;

        Debug.Log("Yayyy, All Systems have been found!");

        yield return new WaitForSeconds(3f); // this is to allow the sync to occur and complete between the client and host

        DisplayGridNumbers(gridSystem.GetGrid());// showing the grid numbers on grid
        targetNumberTxt.text = gridSystem.targetNumber.ToString();
        Debug.Log($"Target number is showing on both the cleint and the host{targetNumberTxt}");

        InitiateRound();
    }
    public void InitiateRound()
    {
       // roundSystem.StartNextRound();//refences to the grid and all the movement positions.(server only )
        roundsNumberTxt.text = roundSystem.currentRound.ToString();// show the number of rounds.
        targetNumberTxt.text = gridSystem.targetNumber.ToString();// show the target number.
        turnSystemTxt.text = turnSystem.currentPlayerTurn.ToString(); // to show players whose turn it is.
        lastMoveTxt.text = movementSystem.GetRequiredMoveType(1).ToString(); // the last move ui 
        
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
