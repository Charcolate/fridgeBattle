using System.Collections.Generic;
using UnityEngine;
using FridgeBattle.Data;
using FridgeBattle.Ingredient;
using FridgeBattle.Fridge;
using FridgeBattle.Core;

namespace FridgeBattle.Battle
{
    public enum BattleResult { Ongoing, Win, Lose }

    /// <summary>
    /// Orchestrates one auto-battle: spawns enemies, deploys ingredients,
    /// runs AI ticks, checks win/loss.
    /// </summary>
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        [Header("Prefabs")]
        public GameObject ingredientUnitPrefab; // simple cube placeholder
        public GameObject enemyUnitPrefab;      // simple sphere placeholder

        [Header("Scene Refs")]
        public Transform battleRoot;
        public BattleGrid grid;

        // Runtime
        public bool IsRunning { get; private set; }
        public BattleResult Result { get; private set; } = BattleResult.Ongoing;

        private readonly List<IngredientUnit> _ingredientUnits = new();
        private readonly List<EnemyUnit> _enemyUnits = new();
        private int _currentWave = 0;
        private LevelData _levelData;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void StartBattle(LevelData levelData, int dayIndex)
        {
            _levelData = levelData;
            Result = BattleResult.Ongoing;
            IsRunning = true;
            _currentWave = 0;

            // Deploy ingredients
            DeployIngredients();

            // Spawn first wave
            SpawnWave(dayIndex, _currentWave);

            Debug.Log($"[BattleManager] Battle started! Wave 0 / {levelData.days[dayIndex].waves.Count}");
        }

        private void DeployIngredients()
        {
            var allIng = FridgeManager.Instance.AllIngredients;
            int maxDeploy = Mathf.Min(allIng.Count, _levelData.maxDeploy);

            for (int i = 0; i < maxDeploy; i++)
            {
                var src = allIng[i];
                if (src.IsExpired) continue; // skip expired

                int col = i % grid.deployCols;
                int row = i / grid.deployCols;
                if (row >= grid.rows) break;

                Vector3 pos = grid.GetCellCenter(col, row);
                var go = Instantiate(ingredientUnitPrefab, pos, Quaternion.identity, battleRoot);
                var unit = go.AddComponent<IngredientUnit>();
                unit.Initialize(src);
                go.AddComponent<UnitAI>().BindBattle(this);
                unit.OnDeath += OnIngredientDied;

                // Color the cube by category for clarity
                var renderer = go.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.material.color = GetCategoryColor(src.Data.category);

                _ingredientUnits.Add(unit);
            }
        }

        private Color GetCategoryColor(IngredientCategory cat)
        {
            return cat switch
            {
                IngredientCategory.Veggie => Color.green,
                IngredientCategory.Meat => new Color(0.9f, 0.5f, 0.5f),
                IngredientCategory.Seafood => Color.blue,
                IngredientCategory.Staple => Color.yellow,
                IngredientCategory.Egg => new Color(1f, 0.9f, 0.7f),
                _ => Color.gray
            };
        }

        private void SpawnWave(int dayIndex, int waveIndex)
        {
            if (dayIndex >= _levelData.days.Count) return;
            var waves = _levelData.days[dayIndex].waves;
            if (waveIndex >= waves.Count) return;

            var wave = waves[waveIndex];
            int spawnX = grid.cols - 1; // rightmost column
            for (int i = 0; i < wave.count; i++)
            {
                int row = i % grid.rows;
                Vector3 pos = grid.GetCellCenter(spawnX, row) + Random.insideUnitSphere * 0.3f;
                pos.y = 0.5f; // lift slightly so it's visible above floor

                var go = Instantiate(enemyUnitPrefab, pos, Quaternion.identity, battleRoot);
                var unit = go.AddComponent<EnemyUnit>();
                unit.Initialize(wave.enemy);
                go.AddComponent<UnitAI>().BindBattle(this);
                unit.OnDeath += OnEnemyDied;

                _enemyUnits.Add(unit);
            }
            Debug.Log($"[BattleManager] Spawned wave {waveIndex} ({wave.count} enemies)");
        }

        private void OnIngredientDied(Unit unit)
        {
            var iu = unit as IngredientUnit;
            _ingredientUnits.Remove(iu);
        }

        private void OnEnemyDied(Unit unit)
        {
            var eu = unit as EnemyUnit;
            _enemyUnits.Remove(eu);
        }

        public List<Unit> GetEnemies() => new List<Unit>(_enemyUnits);
        public List<Unit> GetIngredients() => new List<Unit>(_ingredientUnits);

        void Update()
        {
            if (!IsRunning) return;

            // Check all ingredients dead => Lose
            if (_ingredientUnits.Count == 0)
            {
                EndBattle(BattleResult.Lose);
                return;
            }

            // Check all enemies dead => next wave or Win
            if (_enemyUnits.Count == 0)
            {
                _currentWave++;
                int dayIdx = GameManager.Instance.CurrentDayInLevel - 1;
                int totalWaves = _levelData.days[dayIdx].waves.Count;

                if (_currentWave >= totalWaves)
                {
                    EndBattle(BattleResult.Win);
                }
                else
                {
                    SpawnWave(dayIdx, _currentWave);
                }
            }
        }

        private void EndBattle(BattleResult result)
        {
            IsRunning = false;
            Result = result;
            Debug.Log($"[BattleManager] Battle ended: {result}");

            // Destroy all remaining units
            foreach (var u in _ingredientUnits) if (u != null) Destroy(u.gameObject);
            foreach (var u in _enemyUnits) if (u != null) Destroy(u.gameObject);
            _ingredientUnits.Clear();
            _enemyUnits.Clear();
        }
    }
}
