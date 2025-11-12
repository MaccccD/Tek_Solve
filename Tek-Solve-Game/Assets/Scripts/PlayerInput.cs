using UnityEngine;
using Mirror;

public class PlayerInput : NetworkBehaviour
{
    public static PlayerInput Instance { get; private set; } // the singleton pattern
    private MovementSystem movementSystem; //refrence to the movement logic
    private CodeSystem codeSystem;
    private TurnSystem turnSystem;
    private UISystem visualSystem;
    [SyncVar] public int myPlayerID = 0;


    


    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this; // this singleton patterns calls this or evokes this script immediately when the agme runs before the methods can be called in the start.
        }
    }


    public void Start()
    {
        movementSystem = FindObjectOfType<MovementSystem>();
        turnSystem = FindObjectOfType<TurnSystem>();
        codeSystem = FindObjectOfType<CodeSystem>();
        visualSystem = FindObjectOfType<UISystem>();

        if(isLocalPlayer && isServer)
        {
            myPlayerID = 1; // so the player that joins as host / server , automically becomes player 1 
            visualSystem.backgroundMusic = GetComponent<AudioSource>();
            visualSystem.backgroundMusic.Play();
            //console output:
            Debug.Log("Player 1 assigned !");
        }
        else if (isLocalPlayer)
        {
            CmdRequestPlayerID();
           // myPlayerID = 2; // the other then becomes player 2 if the first player that joined hasn't become automatically asigned as player 1 ID .
        }

    }

    [Command]
    void CmdRequestPlayerID()
    {
        myPlayerID = 2; // the second player's ID who joins as the client
        //console output:
        Debug.Log("Player 2 assigned");
    }

   

    public void Update()
    {
        if (!isLocalPlayer)  //checking if the rest of the code here runs on the local plyer first or else exit.
        {
            return;
            
        } 
        if(turnSystem.currentPlayerTurn != myPlayerID) // here i'm checking if it is the current player's turn 
        {
            return;
        }

        //detecting numpad key presses:
        if (Input.GetKeyDown(KeyCode.Keypad7))
        {
            movementSystem.AttemptMove(myPlayerID, 7); //so diagonal up-left;
            visualSystem.diagonalMoveSound.Play();
            visualSystem.DeactivateDiagonalSound();
                                                       //Debug.Log("yayy , move number 7 working !");
        }
        else if (Input.GetKeyDown(KeyCode.Keypad8))
        {
            movementSystem.AttemptMove(myPlayerID, 8);
            visualSystem.adjacentMoveSound.Play();
            visualSystem.DeactivateAdjacentSound();
        }
        else if (Input.GetKeyDown(KeyCode.Keypad9))
        {
            movementSystem.AttemptMove(myPlayerID, 9);
            visualSystem.diagonalMoveSound.Play();
            visualSystem.DeactivateDiagonalSound();
        }
        else if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            movementSystem.AttemptMove(myPlayerID, 4);
            visualSystem.adjacentMoveSound.Play();
            visualSystem.DeactivateAdjacentSound();
        }
        else if (Input.GetKeyDown(KeyCode.Keypad6))
        {
            movementSystem.AttemptMove(myPlayerID, 6);
            visualSystem.adjacentMoveSound.Play();
            visualSystem.DeactivateAdjacentSound();
        }
        else if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            movementSystem.AttemptMove(myPlayerID, 1);
            visualSystem.diagonalMoveSound.Play();
            visualSystem.DeactivateDiagonalSound();
        }
        else if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            movementSystem.AttemptMove(myPlayerID, 2);
            visualSystem.adjacentMoveSound.Play();
            visualSystem.DeactivateAdjacentSound();
        }
        else if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            movementSystem.AttemptMove(myPlayerID, 3);
            visualSystem.diagonalMoveSound.Play();
            visualSystem.DeactivateDiagonalSound();
        }


        // Alternative: Regular number keys for laptops without numpad
        else if (Input.GetKeyDown(KeyCode.Q))
            movementSystem.AttemptMove(myPlayerID, 7);
        else if (Input.GetKeyDown(KeyCode.W))
            movementSystem.AttemptMove(myPlayerID, 8);
        else if (Input.GetKeyDown(KeyCode.E))
            movementSystem.AttemptMove(myPlayerID, 9);
        else if (Input.GetKeyDown(KeyCode.A))
            movementSystem.AttemptMove(myPlayerID, 4);
        else if (Input.GetKeyDown(KeyCode.D))
            movementSystem.AttemptMove(myPlayerID, 6);
        else if (Input.GetKeyDown(KeyCode.Z))
            movementSystem.AttemptMove(myPlayerID, 1);
        else if (Input.GetKeyDown(KeyCode.S))
            movementSystem.AttemptMove(myPlayerID, 2);
        else if (Input.GetKeyDown(KeyCode.C))
            movementSystem.AttemptMove(myPlayerID, 3);
    }

     void OnGUI() // a method to help debug.
    {
        GUI.Label(new Rect(10, 10, 200, 30), $"You are player : {myPlayerID}");

        if(turnSystem != null)
        {
            bool isMyTurn = turnSystem.currentPlayerTurn == myPlayerID; // checking which player'sturn it is :
            string turnText = isMyTurn ? "Your Turn" : $"Player {myPlayerID}, {turnSystem.currentPlayerTurn}'s Turn";
            GUI.Label(new Rect(10, 40, 200, 30), turnText);

            if(movementSystem != null)
            {
                var requiredMove = movementSystem.GetRequiredMoveType(myPlayerID);
                if(requiredMove!= MovementSystem.MoveType.None)
                {
                    GUI.Label(new Rect(10, 70, 300, 30), $"The required Move is: {requiredMove}");
                }
            }
        }
    }
}
