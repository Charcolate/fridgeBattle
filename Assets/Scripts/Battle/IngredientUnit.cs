using UnityEngine;
using FridgeBattle.Ingredient;

namespace FridgeBattle.Battle
{
    /// <summary>
    /// Runtime battle representation of an ingredient.
    /// Reads stats from IngredientInstance and computes effective battle stats.
    /// </summary>
    public class IngredientUnit : Unit
    {
        public IngredientInstance Source { get; private set; }

        public void Initialize(IngredientInstance source)
        {
            Source = source;
            MaxHP = source.EffectiveHP();
            CurrentHP = MaxHP;
            ATK = source.EffectiveATK();
            AS = source.Data.baseAS;
            Range = source.Data.baseRange;
        }

        protected override void Die()
        {
            // Mark the source ingredient as dead in the fridge (removed at battle end)
            base.Die();
        }
    }
}
