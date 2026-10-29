using System.Collections.Generic;
using UnityEngine;
using FridgeBattle.Data;
using FridgeBattle.Core;

namespace FridgeBattle.Fridge
{
    /// <summary>
    /// Manages all ingredients currently in the fridge.
    /// Handles placement, layer assignment, temperature, decay, etc.
    /// </summary>
    public class FridgeManager : MonoBehaviour
    {
        public static FridgeManager Instance { get; private set; }

        // All ingredients in the fridge
        public List<Ingredient.IngredientInstance> AllIngredients { get; private set; } = new();

        // Per-layer list (index = layer index)
        private List<List<Ingredient.IngredientInstance>> _layers = new();

        // Layer temps (index = layer index), default 4C
        public float[] LayerTemps { get; private set; }

        private int _nextInstanceID = 1;
        private LevelData _currentLevel;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void InitializeWithLevel(LevelData level)
        {
            _currentLevel = level;
            AllIngredients.Clear();
            _layers.Clear();
            _nextInstanceID = 1;

            for (int i = 0; i < level.layerCount; i++)
            {
                _layers.Add(new List<Ingredient.IngredientInstance>());
            }
            LayerTemps = new float[level.layerCount];
            for (int i = 0; i < level.layerCount; i++) LayerTemps[i] = 4f;

            // Give starting ingredients for level 1
            if (level.levelNumber == 1)
            {
                // Will be populated by GameManager.StartNewGame
            }
        }

        public Ingredient.IngredientInstance AddIngredient(IngredientData data)
        {
            var inst = new Ingredient.IngredientInstance(data, _nextInstanceID++);
            inst.LayerIndex = 0; // default top layer
            AllIngredients.Add(inst);
            if (_layers.Count > 0) _layers[0].Add(inst);
            return inst;
        }

        public void RemoveIngredient(int instanceID)
        {
            var inst = AllIngredients.Find(i => i.InstanceID == instanceID);
            if (inst != null)
            {
                _layers[inst.LayerIndex]?.Remove(inst);
                AllIngredients.Remove(inst);
            }
        }

        public void MoveToLayer(int instanceID, int newLayer)
        {
            var inst = AllIngredients.Find(i => i.InstanceID == instanceID);
            if (inst == null) return;
            if (newLayer < 0 || newLayer >= _layers.Count) return;

            _layers[inst.LayerIndex].Remove(inst);
            inst.LayerIndex = newLayer;
            _layers[newLayer].Add(inst);
        }

        public List<Ingredient.IngredientInstance> GetLayerIngredients(int layerIndex)
        {
            if (layerIndex < 0 || layerIndex >= _layers.Count) return new List<Ingredient.IngredientInstance>();
            return _layers[layerIndex];
        }

        public int LayerCount => _layers.Count;

        /// <summary>Called each day during evening settlement.</summary>
        public void EveningSettlement()
        {
            // Decay freshness
            foreach (var inst in AllIngredients)
            {
                // Temperature deviation factor
                float tempFactor = 1f;
                float layerTemp = LayerTemps[inst.LayerIndex];
                if (layerTemp < inst.Data.suitableTempMin || layerTemp > inst.Data.suitableTempMax)
                {
                    float deviation = Mathf.Min(
                        Mathf.Abs(layerTemp - inst.Data.suitableTempMin),
                        Mathf.Abs(layerTemp - inst.Data.suitableTempMax));
                    tempFactor = 1f + deviation * 0.1f; // +10% decay per degree off
                }

                // Same-layer friend/foe effects on decay
                float friendFactor = 1f;
                var sameLayer = _layers[inst.LayerIndex];
                foreach (var other in sameLayer)
                {
                    if (other.InstanceID == inst.InstanceID) continue;
                    int fwd = 0;
                    inst.Friendships.TryGetValue(other.InstanceID, out fwd);
                    // Symmetric: check both ways
                    int bwd = 0;
                    other.Friendships.TryGetValue(inst.InstanceID, out bwd);
                    int minF = Mathf.Min(fwd, bwd);

                    if (minF >= 80) friendFactor -= 0.2f;       // best friends: -20% decay
                    else if (minF >= 60) friendFactor -= 0.1f;  // friendly: -10%
                    else if (minF <= 20 && minF > 0) friendFactor += 0.2f; // hostile: +20% decay
                }

                float decay = inst.Data.baseDecayRate * tempFactor * friendFactor;
                inst.Freshness -= decay;
                if (inst.Freshness < 0f) inst.Freshness = 0f;
                if (inst.Freshness > 100f) inst.Freshness = 100f;
            }

            // Natural friendship decay (-1 per day)
            foreach (var inst in AllIngredients)
            {
                var keys = new List<int>(inst.Friendships.Keys);
                foreach (var k in keys)
                {
                    int v = inst.Friendships[k] - 1;
                    if (v < 0) v = 0;
                    inst.Friendships[k] = v;
                }
            }

            // Same-layer relationship boost
            foreach (var layer in _layers)
            {
                for (int i = 0; i < layer.Count; i++)
                {
                    for (int j = i + 1; j < layer.Count; j++)
                    {
                        // Initialize friendship if not present
                        if (!layer[i].Friendships.ContainsKey(layer[j].InstanceID))
                            layer[i].Friendships[layer[j].InstanceID] = 30;
                        if (!layer[j].Friendships.ContainsKey(layer[i].InstanceID))
                            layer[j].Friendships[layer[i].InstanceID] = 30;

                        // +3-8 per day random
                        int gain = Random.Range(3, 9);
                        layer[i].Friendships[layer[j].InstanceID] = Mathf.Min(100,
                            layer[i].Friendships[layer[j].InstanceID] + gain);
                        layer[j].Friendships[layer[i].InstanceID] = Mathf.Min(100,
                            layer[j].Friendships[layer[i].InstanceID] + gain);
                    }
                }
            }
        }

        public void FindAwakenings()
        {
            if (!_currentLevel.unlockFriendship) return;
            foreach (var layer in _layers)
            {
                if (layer.Count < 3) continue;

                // Check all pairs have bidirectional friendship >= 80
                bool allHigh = true;
                for (int i = 0; i < layer.Count && allHigh; i++)
                {
                    for (int j = i + 1; j < layer.Count && allHigh; j++)
                    {
                        int fwd = layer[i].Friendships.GetValueOrDefault(layer[j].InstanceID, 0);
                        int bwd = layer[j].Friendships.GetValueOrDefault(layer[i].InstanceID, 0);
                        if (fwd < 80 || bwd < 80) allHigh = false;
                    }
                }

                if (!allHigh) continue;

                // Average friendship
                float sum = 0; int count = 0;
                for (int i = 0; i < layer.Count; i++)
                {
                    for (int j = i + 1; j < layer.Count; j++)
                    {
                        sum += layer[i].Friendships.GetValueOrDefault(layer[j].InstanceID, 0);
                        sum += layer[j].Friendships.GetValueOrDefault(layer[i].InstanceID, 0);
                        count += 2;
                    }
                }
                float avg = count > 0 ? sum / count : 80f;
                float chance = (avg - 80f) * 2f; // x2% per point above 80
                chance = Mathf.Clamp(chance, 0f, 100f);

                if (Random.Range(0f, 100f) < chance)
                {
                    // Awaken!
                    foreach (var inst in layer)
                    {
                        if (!inst.IsAwakened)
                        {
                            inst.IsAwakened = true;
                            inst.AwakeningBuffHP = Mathf.RoundToInt(inst.Data.baseHP * 0.25f);
                            inst.AwakeningBuffATK = Mathf.RoundToInt(inst.Data.baseATK * 0.20f);
                            Debug.Log($"[Awakening] {inst.DisplayName} awakened!");
                        }
                    }
                }
            }
        }
    }
}
