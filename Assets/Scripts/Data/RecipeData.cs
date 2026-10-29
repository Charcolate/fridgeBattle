using UnityEngine;

namespace FridgeBattle.Data
{
    [CreateAssetMenu(fileName = "RecipeData", menuName = "FridgeBattle/Data/Recipe")]
    public class RecipeData : ScriptableObject
    {
        [Header("Identity")]
        public string recipeID;
        public string resultName;

        [Header("Inputs")]
        public IngredientData inputA;
        public IngredientData inputB;
        public int requiredFriendshipAtoB = 40; // minimum friendship A->B
        public int requiredFriendshipBtoA = 40; // minimum friendship B->A

        [Header("Output Stats (override base)")]
        public int resultHP;
        public int resultATK;
        public float resultAS;
        public int resultRange;

        [Header("Cost")]
        public int energyCost = 10;
    }
}
