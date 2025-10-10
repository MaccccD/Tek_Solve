using UnityEngine;
using Mirror;
public class LevelSystem : NetworkBehaviour
{
   public struct LevelConfig 
    {
        //this struct basically houses the different target number ranges and grid board size ranges as the levels of the game progresses
        public int gridSize;
        public Vector2Int numberRange;
        public Vector2Int targetRange;

    }

    LevelConfig[] levels = {
        new LevelConfig { gridSize = 4, numberRange = new(1,9), targetRange = new(15,30) },   // Level 1 ( when the game starts)
        new LevelConfig { gridSize = 4, numberRange = new(1,12), targetRange = new(20,40) },  // Level 2  (after the first 5 rounds have been completed)
        new LevelConfig { gridSize = 5, numberRange = new(1,9), targetRange = new(18,35) },   // Level 3
        new LevelConfig { gridSize = 5, numberRange = new(1,15), targetRange = new(25,50) }   // Level 4
        
    };

}
