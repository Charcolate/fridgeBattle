using UnityEngine;
using FridgeBattle.Data;

namespace FridgeBattle.Core
{
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }

        public int Money { get; private set; } = 50;
        public int Energy { get; private set; } = 10;
        public int RemainingActions { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Initialize()
        {
            Money = 50;
            Energy = 10;
        }

        public void OnDayStart(LevelData level)
        {
            RemainingActions = level.actionPointsPerDay;
            // Energy regenerates
            Energy += level.layerCount * 3;
        }

        /// <summary>Attempt to spend 1 action point. Returns true if successful.</summary>
        public bool TryConsumeAction()
        {
            if (RemainingActions <= 0) return false;
            RemainingActions--;
            return true;
        }

        public bool TrySpendMoney(int amount)
        {
            if (Money < amount) return false;
            Money -= amount;
            return true;
        }

        public bool TrySpendEnergy(int amount)
        {
            if (Energy < amount) return false;
            Energy -= amount;
            return true;
        }

        public void AddMoney(int amount) => Money += amount;
        public void AddEnergy(int amount) => Energy += amount;
        public void AddActions(int count) => RemainingActions += count;
    }
}
