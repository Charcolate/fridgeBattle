using System.Collections.Generic;
using UnityEngine;

namespace FridgeBattle.Data
{
    [System.Serializable]
    public class WaveConfig
    {
        public EnemyData enemy;
        public int count = 3;
    }

    [System.Serializable]
    public class DayConfig
    {
        public List<WaveConfig> waves = new List<WaveConfig>();
    }

    [CreateAssetMenu(fileName = "LevelData", menuName = "FridgeBattle/Data/Level")]
    public class LevelData : ScriptableObject
    {
        [Header("Identity")]
        public int levelNumber = 1;
        public string levelName = "Tiny Fridge";

        [Header("Fridge")]
        public int layerCount = 2;
        public int battleGridCols = 3;
        public int battleGridRows = 3;
        public int maxDeploy = 6;

        [Header("Progression Unlocks")]
        public int actionPointsPerDay = 3;
        public bool unlockFriendship = false;   // Lv5
        public bool unlockStoryTime = false;    // Lv5
        public bool unlockSynthesis = false;    // Lv3
        public bool unlockBreeding = false;     // Lv5
        public bool unlockDoorBattle = false;   // Lv7
        public bool unlockMutate = false;       // Lv9

        [Header("Days (5 per level)")]
        public List<DayConfig> days = new List<DayConfig>();
    }
}
