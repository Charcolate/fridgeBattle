using System.Collections.Generic;
using UnityEngine;
using FridgeBattle.Data;

namespace FridgeBattle.Core
{
    public enum GamePhase
    {
        Fridge,      // 冰箱管理（调整层/温度）
        Actions,     // 行动选择阶段
        Story,       // 故事对话
        Deploy,      // 布阵
        Battle,      // 自动战斗
        Evening,     // 晚间结算
        LevelUp      // 冰箱升级到下一关
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("References")]
        public List<LevelData> allLevels = new List<LevelData>();

        // Runtime state
        public int CurrentLevelIndex { get; private set; }
        public int CurrentDayInLevel { get; private set; }
        public int CurrentTotalDay { get; private set; }
        public GamePhase Phase { get; private set; } = GamePhase.Fridge;

        public LevelData CurrentLevel => allLevels[Mathf.Clamp(CurrentLevelIndex, 0, allLevels.Count - 1)];

        public event System.Action<GamePhase> OnPhaseChanged;
        public event System.Action OnDayAdvanced;
        public event System.Action<int> OnLevelUp;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void StartNewGame()
        {
            CurrentLevelIndex = 0;
            CurrentDayInLevel = 1;
            CurrentTotalDay = 1;
            SetPhase(GamePhase.Fridge);

            // Init other managers
            ResourceManager.Instance.Initialize();
            Fridge.FridgeManager.Instance.InitializeWithLevel(CurrentLevel);
        }

        public void SetPhase(GamePhase phase)
        {
            Phase = phase;
            OnPhaseChanged?.Invoke(phase);
            Debug.Log($"[GameManager] Phase -> {phase}");
        }

        public void AdvanceDay()
        {
            CurrentDayInLevel++;
            CurrentTotalDay++;

            // Evening settlement happens before advancing
            Fridge.FridgeManager.Instance.EveningSettlement();
            ResourceManager.Instance.OnDayStart(CurrentLevel);

            // Check level complete (5 days per level)
            if (CurrentDayInLevel > 5)
            {
                AdvanceLevel();
            }

            OnDayAdvanced?.Invoke();
            SetPhase(GamePhase.Fridge);
        }

        public void AdvanceLevel()
        {
            CurrentLevelIndex++;
            CurrentDayInLevel = 1;
            OnLevelUp?.Invoke(CurrentLevelIndex);

            if (CurrentLevelIndex >= allLevels.Count)
            {
                // Game Complete
                Debug.Log("=== ALL 9 LEVELS CLEARED! ===");
                return;
            }

            Fridge.FridgeManager.Instance.InitializeWithLevel(CurrentLevel);
            SetPhase(GamePhase.Fridge);
        }
    }
}
