using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class UISystem : MonoBehaviour
{
    [Header("Game Screen UI")]
    [SerializeField] public Text targetNumberTxt;
    [SerializeField] public Text lastMoveTxt;
    [SerializeField] public Text roundsNumberTxt;
    [SerializeField] public Text turnSystemTxt;
    [SerializeField] public Text incorrectCodeTxt;

    [Header("Player 1 UI")]
    [SerializeField] public List<int> p1digitcode = new List<int>(4);
    [SerializeField] public Text P1CurrentSum;
    [SerializeField] public Text p1NeedTxt;

    [Header("Player 2 UI")]
    [SerializeField] public List<int> p2digitcode = new List<int>(4);
    [SerializeField] public Text P2CurrentSum;
    [SerializeField] public Text p2NeedTxt;

    [Header("Player Victory UI")]
    [SerializeField] public GameObject roundWinPanel;
    [SerializeField] public Text roundWinText;
    [SerializeField] public GameObject matchWinPanel;
    [SerializeField] public Text matchWinText;

    [Header("Player Stats")]
    [SerializeField] public GameObject statsPanel;
    [SerializeField] public Text player1WinsText;
    [SerializeField] public Text player2WinsText;
    [SerializeField] public Button RestartGame;
    [SerializeField] public Button ExitGame;

    [Header("Game Audios")]
    [SerializeField] public AudioSource incorrectCodeSound;
    [SerializeField] public AudioSource correctCodeSound;
    [SerializeField] public AudioSource backgroundMusic;


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


}
