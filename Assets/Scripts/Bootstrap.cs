using System.Collections.Generic;
using UnityEngine;
using FridgeBattle.Data;
using FridgeBattle.Core;
using FridgeBattle.Fridge;
using FridgeBattle.Battle;
using FridgeBattle.Ingredient;

namespace FridgeBattle
{
    /// <summary>
    /// Attach this to a GameObject in the boot scene.
    /// It creates all manager singletons and bootstraps the game state.
    /// </summary>
    public class Bootstrap : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject ingredientUnitPrefab;
        public GameObject enemyUnitPrefab;

        [Header("Battle Scene Refs")]
        public Transform battleRoot;
        public BattleGrid grid;

        [Header("Market Pool (starting data)")]
        public List<IngredientData> marketPool = new();

        void Start()
        {
            // Ensure managers exist
            EnsureManager<GameManager>();
            EnsureManager<ResourceManager>();
            EnsureManager<FridgeManager>();
            var battleManager = EnsureManager<BattleManager>();
            battleManager.ingredientUnitPrefab = ingredientUnitPrefab;
            battleManager.enemyUnitPrefab = enemyUnitPrefab;
            battleManager.battleRoot = battleRoot;
            battleManager.grid = grid;

            // Build level data
            BuildLevels();

            // Seed market pool if empty
            if (marketPool.Count == 0)
            {
                marketPool = CreateDefaultIngredientData();
            }

            // Start!
            GameManager.Instance.StartNewGame();

            // Add starter ingredients (2 tomato, 1 beef for level 1)
            // We can't easily load ScriptableObjects at runtime without Resources,
            // so let's just use the market pool first entries
            if (marketPool.Count >= 2)
            {
                FridgeManager.Instance.AddIngredient(marketPool[0]);
                FridgeManager.Instance.AddIngredient(marketPool[0]);
            }
            if (marketPool.Count >= 2)
            {
                FridgeManager.Instance.AddIngredient(marketPool[1]);
            }

            // Auto-start first battle for testing
            StartFirstBattle();
        }

        private T EnsureManager<T>() where T : Component
        {
            var existing = FindFirstObjectByType<T>();
            if (existing != null) return existing;
            var go = new GameObject(typeof(T).Name);
            DontDestroyOnLoad(go);
            return go.AddComponent<T>();
        }

        private void BuildLevels()
        {
            var gm = GameManager.Instance;
            if (gm.allLevels.Count > 0) return; // already built

            // --- Level 1: Tiny Fridge (2 layers, 3x3 grid) ---
            var lv1 = ScriptableObject.CreateInstance<LevelData>();
            lv1.levelNumber = 1;
            lv1.levelName = "小小冰箱";
            lv1.layerCount = 2;
            lv1.battleGridCols = 3;
            lv1.battleGridRows = 3;
            lv1.maxDeploy = 6;
            lv1.actionPointsPerDay = 3;
            // Days
            for (int d = 0; d < 5; d++)
            {
                var dc = new DayConfig();
                var wave = new WaveConfig();
                wave.enemy = CreateDefaultEnemyData("cocci1", EnemyType.Cocci, 40, 8, 1.0f, 1);
                wave.count = 3 + d; // difficulty ramps
                dc.waves.Add(wave);
                lv1.days.Add(dc);
            }

            gm.allLevels.Add(lv1);

            // Levels 2-9: stub levels (we'll add them iteratively)
            for (int i = 2; i <= 9; i++)
            {
                var lv = ScriptableObject.CreateInstance<LevelData>();
                lv.levelNumber = i;
                lv.levelName = $"冰箱 Lv{i}";
                lv.layerCount = Mathf.Clamp(2 + (i - 1) / 2, 2, 7);
                lv.battleGridCols = i <= 3 ? 3 : i <= 6 ? 4 : 5;
                lv.battleGridRows = i <= 6 ? 3 : 4;
                lv.maxDeploy = lv.battleGridCols * lv.battleGridRows / 2;
                lv.actionPointsPerDay = i <= 3 ? 3 : i <= 6 ? 4 : 5;

                for (int d = 0; d < 5; d++)
                {
                    var dc = new DayConfig();
                    var wave = new WaveConfig();
                    wave.enemy = CreateDefaultEnemyData("cocciX", EnemyType.Cocci, 50 + i * 10, 10 + i * 2, 1.0f, 1);
                    wave.count = 3 + d + i;
                    dc.waves.Add(wave);
                    lv.days.Add(dc);
                }

                gm.allLevels.Add(lv);
            }
        }

        private EnemyData CreateDefaultEnemyData(string id, EnemyType type, int hp, int atk, float atkSpeed, int range)
        {
            var d = ScriptableObject.CreateInstance<EnemyData>();
            d.enemyID = id;
            d.displayName = id;
            d.type = type;
            d.hp = hp;
            d.atk = atk;
            d.atkSpeed = atkSpeed;
            d.range = range;
            d.sellValue = 2;
            return d;
        }

        private List<IngredientData> CreateDefaultIngredientData()
        {
            var list = new List<IngredientData>();

            // Tomato (Veggie)
            var tomato = ScriptableObject.CreateInstance<IngredientData>();
            tomato.ingredientID = "tomato";
            tomato.displayName = "番茄";
            tomato.category = IngredientCategory.Veggie;
            tomato.baseHP = 45;
            tomato.baseATK = 12;
            tomato.baseAS = 1.1f;
            tomato.baseRange = 1;
            tomato.suitableTempMin = 2f;
            tomato.suitableTempMax = 8f;
            tomato.baseDecayRate = 5f;
            tomato.purchaseCost = 8;
            tomato.baseTrust = 65;
            list.Add(tomato);

            // Beef (Meat)
            var beef = ScriptableObject.CreateInstance<IngredientData>();
            beef.ingredientID = "beef";
            beef.displayName = "牛肉";
            beef.category = IngredientCategory.Meat;
            beef.baseHP = 80;
            beef.baseATK = 10;
            beef.baseAS = 0.8f;
            beef.baseRange = 1;
            beef.suitableTempMin = 0f;
            beef.suitableTempMax = 4f;
            beef.baseDecayRate = 7f;
            beef.purchaseCost = 15;
            beef.baseTrust = 60;
            list.Add(beef);

            // Egg
            var egg = ScriptableObject.CreateInstance<IngredientData>();
            egg.ingredientID = "egg";
            egg.displayName = "鸡蛋";
            egg.category = IngredientCategory.Egg;
            egg.baseHP = 55;
            egg.baseATK = 11;
            egg.baseAS = 1.0f;
            egg.baseRange = 2;
            egg.suitableTempMin = 2f;
            egg.suitableTempMax = 6f;
            egg.baseDecayRate = 4f;
            egg.purchaseCost = 6;
            egg.baseTrust = 70;
            list.Add(egg);

            return list;
        }

        private void StartFirstBattle()
        {
            // Start battle for day 1
            BattleManager.Instance.StartBattle(
                GameManager.Instance.CurrentLevel,
                GameManager.Instance.CurrentDayInLevel - 1);

            GameManager.Instance.SetPhase(GamePhase.Battle);
        }
    }
}
