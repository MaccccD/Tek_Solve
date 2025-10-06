using UnityEngine;
using System.Collections;
using System;
using Mirror;


public class GridSystem : NetworkBehaviour
{
   [SerializeField] [SyncVar] private int[][] griNumbers = new int[4][]; // dumi: using a jagged edge array here instead of the multimdimensional array 

   // void GenerateGrid() { }
}
