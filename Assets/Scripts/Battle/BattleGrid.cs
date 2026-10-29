using UnityEngine;

namespace FridgeBattle.Battle
{
    public class BattleGrid : MonoBehaviour
    {
        public int cols = 3;
        public int rows = 3;
        public float cellSize = 1.5f;
        public Vector3 origin = Vector3.zero;

        // Grid[x,y] returns world position
        public Vector3 GetCellCenter(int x, int y)
        {
            return origin + new Vector3(x * cellSize, 0, y * cellSize);
        }

        public bool IsValid(int x, int y)
        {
            return x >= 0 && x < cols && y >= 0 && y < rows;
        }

        /// <summary>Ingredients deploy on left columns (0..deployCols-1), enemies spawn from right.</summary>
        public int deployCols => Mathf.CeilToInt(cols / 2f);
    }
}
