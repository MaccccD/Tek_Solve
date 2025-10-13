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
     public Text roundWinText;
     public GameObject matchWinPanel;
     public Text matchWinText;

    [Header("Player Stats")]
     public GameObject statsPanel;
     public Text player1WinsText;
     public Text player2WinsText;
     public Button RestartGame;
     public Button ExitGame;

    [Header("Game Audios")]
     public AudioSource incorrectCodeSound;
     public AudioSource correctCodeSound;
     public AudioSource backgroundMusic;


    [Header("Script References")]
    [SerializeField] private RoundManagementSystem roundSystem;
    [SerializeField] private GridSystem gridSystem;
    [SerializeField] private MovementSystem movementSystem;
    [SerializeField] private TurnSystem turnSystem;




    private void Start()
    {
        roundSystem = FindFirstObjectByType<RoundManagementSystem>();
        gridSystem = FindFirstObjectByType<GridSystem>();
        movementSystem = FindFirstObjectByType<MovementSystem>();
        turnSystem = FindFirstObjectByType<TurnSystem>();
        Debug.Log("yayy, scripts found");
        InitiateRound();
    }
    public void InitiateRound()
    {
        roundSystem.StartNextRound();//refences to the grid and all the movement positions.
        roundsNumberTxt.text = roundSystem.currentRound.ToString();// show the number of rounds.
        targetNumberTxt.text = gridSystem.targetNumber.ToString();// show the target number.
        turnSystemTxt.text = turnSystem.CurrentPlayerTurn.ToString(); // to show players whose turn it is.
        lastMoveTxt.text = movementSystem.GetRequiredMoveType(1).ToString(); // the last move ui 
        
    }

    private void Update()
    {
        turnSystem.SwitchTurn();
        turnSystemTxt.text = turnSystem.CurrentPlayerTurn.ToString();
        return;
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
                displays[i].text = "?"; // the placehlder Q for numbers not added yet
            }
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
