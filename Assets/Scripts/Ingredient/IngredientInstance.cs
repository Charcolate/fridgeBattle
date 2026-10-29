using System.Collections.Generic;
using UnityEngine;
using FridgeBattle.Data;

namespace FridgeBattle.Ingredient
{
    public class IngredientInstance
    {
        public int InstanceID { get; private set; }
        public IngredientData Data { get; private set; }
        public string DisplayName => Data.displayName;

        public float Freshness { get; set; } = 100f;
        public int Trust { get; set; }
        public int LayerIndex { get; set; } = 0;

        public Dictionary<int, int> Friendships { get; private set; } = new();

        public int AwakeningBuffHP { get; set; } = 0;
        public int AwakeningBuffATK { get; set; } = 0;
        public bool IsAwakened { get; set; } = false;

        public IngredientInstance(IngredientData data, int instanceID)
        {
            Data = data;
            InstanceID = instanceID;
            Trust = data.baseTrust;
            Freshness = 100f;
        }

        public int EffectiveHP()
        {
            float trustMod = 0.5f + (Trust / 100f) * 0.5f;
            float freshMod = Freshness >= 30f ? 1f : (Freshness >= 10f ? 0.85f : 0.7f);
            return Mathf.RoundToInt((Data.baseHP + AwakeningBuffHP) * trustMod * freshMod);
        }

        public int EffectiveATK()
        {
            float trustMod = 0.5f + (Trust / 100f) * 0.5f;
            float freshMod = Freshness >= 30f ? 1f : (Freshness >= 10f ? 0.85f : 0.7f);
            return Mathf.RoundToInt((Data.baseATK + AwakeningBuffATK) * trustMod * freshMod);
        }

        public bool IsExpired => Freshness <= 0f;
    }
}
