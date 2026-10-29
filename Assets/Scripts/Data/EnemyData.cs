using UnityEngine;

namespace FridgeBattle.Data
{
    public enum EnemyType
    {
        Cocci,      // 普通球菌
        Clostridium,// 梭菌
        Mold,       // 霉菌团
        Bacillus,   // 变异杆菌
        Mutiny      // 叛变食材（运行时生成，不从ScriptableObject读）
    }

    [CreateAssetMenu(fileName = "EnemyData", menuName = "FridgeBattle/Data/Enemy")]
    public class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        public string enemyID;
        public string displayName;
        public EnemyType type;

        [Header("Battle Stats")]
        public int hp = 40;
        public int atk = 8;
        public float atkSpeed = 1.0f;
        public int range = 1;

        [Header("Special")]
        public float infectionChance = 0f; // 0-1 probability to apply infection on hit
        public int splitThreshold = 0;     // HP percent at which to split (0 = no split)
        public int splitCount = 0;         // how many children to spawn on split

        [Header("Wave Reward")]
        public int sellValue = 2;          // coins gained when sold via "work"
    }
}
