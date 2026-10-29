using UnityEngine;

namespace FridgeBattle.Data
{
    public enum IngredientCategory
    {
        Veggie,
        Meat,
        Seafood,
        Staple,
        Egg,
        Dairy,
        Drink,
        Spice,
        JunkFood
    }

    [CreateAssetMenu(fileName = "IngredientData", menuName = "FridgeBattle/Data/Ingredient")]
    public class IngredientData : ScriptableObject
    {
        [Header("Identity")]
        public string ingredientID;
        public string displayName;
        public IngredientCategory category;

        [Header("Battle Stats")]
        public int baseHP = 50;
        public int baseATK = 10;
        public float baseAS = 1.0f;
        public int baseRange = 1;

        [Header("Fridge")]
        public float suitableTempMin = 2f;
        public float suitableTempMax = 8f;
        public float baseDecayRate = 5f;
        public int purchaseCost = 10;

        [Header("Trust")]
        [Range(0, 100)] public int baseTrust = 60;
    }
}
