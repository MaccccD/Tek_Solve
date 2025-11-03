using UnityEngine;
using Mirror;

public class PlayerInput : NetworkBehaviour
{
    public static PlayerInput Instance { get; private set; } // the singleton pattern
    private MovementSystem movementSystem; //refrence to the movement logic
    private CodeSystem codeSystem;
    private TurnSystem turnSystem;
    [SerializeField][SyncVar] private int myPlayerID;
    


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

        if(isLocalPlayer && isServer)
        {
            myPlayerID = 1; // so the player that joins as host / server , automically becomes player 1 
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
            //Debug.Log("yayy , move number 7 working !");
           
        }
        if (Input.GetKeyDown(KeyCode.Keypad8))
        {
            movementSystem.AttemptMove(myPlayerID, 8); //so the up-move.
           
          //  Debug.Log("yayy , move number 8 working !");
        }
        if (Input.GetKeyDown(KeyCode.Keypad9))
        {
            movementSystem.AttemptMove(myPlayerID, 9); // so diagonal up-right.
            
           // Debug.Log("yayy, move number 9  working");
        }
        if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            movementSystem.AttemptMove(myPlayerID, 4); // so left move (adjacent).
        }
        if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            movementSystem.AttemptMove(myPlayerID, 5); // (so center move, adjacent).
        }
        if (Input.GetKeyDown(KeyCode.Keypad6))
        {
            movementSystem.AttemptMove(myPlayerID, 6); // so the right-move (adjacent).
        }
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            movementSystem.AttemptMove(myPlayerID, 1); // so diagonal down left.
        }
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            movementSystem.AttemptMove(myPlayerID, 2); // so the down move (adjacent).
        }
        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            movementSystem.AttemptMove(myPlayerID, 3); //so the diagonal down-right.
        }


        // Alternative: Regular number keys for laptops without numpad
        else if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Q)) // so diagonal up left will be the Q 
            movementSystem.AttemptMove(myPlayerID, 7);
        else if (Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.W)) // up
            movementSystem.AttemptMove(myPlayerID, 8);
        else if (Input.GetKeyDown(KeyCode.Alpha9) || Input.GetKeyDown(KeyCode.E))// diagonal up right.
            movementSystem.AttemptMove(myPlayerID, 9);
        else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.A)) //left
            movementSystem.AttemptMove(myPlayerID, 4);
        else if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.D)) //right
            movementSystem.AttemptMove(myPlayerID, 6);
        else if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Z))//diagonal down left
            movementSystem.AttemptMove(myPlayerID, 1);
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.S)) // down
            movementSystem.AttemptMove(myPlayerID, 2);
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.C)) //diagonal down right
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
