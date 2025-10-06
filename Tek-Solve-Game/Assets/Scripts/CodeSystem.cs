using UnityEngine;
using Mirror;
using System;
using System.Linq;
using System.Collections;

public class CodeSystem : NetworkBehaviour
{
    [SyncVar]public int targetNumber; // the targe number that will be shown to both players 
    public readonly SyncList<int> player1Code = new SyncList<int>(); // what the first player inputs
    public readonly SyncList<int> player2Code = new SyncList<int>();  // what the second player inputs

    bool ValidateCode(SyncList<int> code)
    {
        return code.Sum() == targetNumber && code.Count == 4; //here i'm basically checking if the 4-digit code users have inputted equals to the target numbers they were given.
    }

    public void AddToCode(int playerID, int number)
    {
        var code = playerID == 1 ? player1Code : player2Code;
        code.Add(number); // here i'm allowing each player on their turn to add a number or a single digit of the 4-digit code required.

        if(code.Count == 4)
        {
            CheckCodeSubmission(playerID, code);
        }
    }

    [Server]
    void CheckCodeSubmission(int playerID, SyncList<int> code)
    {
        if(code.Sum() == targetNumber && code.Count == 4)
        {
            Console.WriteLine("Yayy, someone cracked the code!");
            
            // player who got it wins the round and the board state changes for the next round.
        }

        else
        {
            code.RemoveAt(3);// it's 3 bc a list starts from index 0 not 1 so then the 4th digit becomes index 3;
            //RpcCodeRejected(playerID);
        }
    }
}