using UnityEngine;
using FridgeBattle.Data;

namespace FridgeBattle.Battle
{
    public class EnemyUnit : Unit
    {
        public EnemyData Data { get; private set; }
        public bool CanSplit => Data.splitThreshold > 0;

        public void Initialize(EnemyData data)
        {
            Data = data;
            MaxHP = data.hp;
            CurrentHP = MaxHP;
            ATK = data.atk;
            AS = data.atkSpeed;
            Range = data.range;
        }

        protected override void Die()
        {
            // Handle split
            if (CanSplit && CurrentHP <= (int)(Data.hp * Data.splitThreshold / 100f))
            {
                // Splitting handled in BattleManager
            }
            base.Die();
        }
    }
}
