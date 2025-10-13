using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UISystem : MonoBehaviour
{
    [Header("Game Screen UI")]
    [SerializeField] public Text targetNumberTxt;
    [SerializeField] public Text lastMoveTxt;
    [SerializeField] public Text roundsNumberTxt;
    [SerializeField] public Text turnSystemTxt;
    [SerializeField] public Text incorrectCodeTxt;

    [Header("Player 1 & Player 2 UI")]
    [SerializeField] public List<int> p1digitcode = new List<int>(4);
    [SerializeField] public List<int> p2digitcode = new List<int>(4);
    [SerializeField] public Text P1CurrentSum;
    [SerializeField] public Text P2CurrentSum;
    [SerializeField] public Text p1NeedTxt;
    [SerializeField] public Text p2NeedTxt;

    [Header("Player Victory UI")]
    [SerializeField] public GameObject roundWinPanel;
    [SerializeField] public Text roundWinText;
    [SerializeField] public GameObject matchWinPanel;
    [SerializeField] public Text matchWinText;

    [Header("Player Stats")]
    [SerializeField] public Text player1WinsText;
    [SerializeField] public Text player2WinsText;
    [SerializeField] public Button RestartGame;
    [SerializeField] public Button ExitGame;

    [Header("Game Audios")]
    [SerializeField] public AudioSource incorrectCodeSound;
    [SerializeField] public AudioSource correctCodeSound;
    [SerializeField] public AudioSource backgroundMusic;



}
