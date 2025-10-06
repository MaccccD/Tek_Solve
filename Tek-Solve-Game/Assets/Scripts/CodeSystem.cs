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
}
