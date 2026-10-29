using UnityEngine;

namespace FridgeBattle.Battle
{
    /// <summary>
    /// Base class for any combat unit in battle (ingredient or enemy).
    /// Handles movement, attack, HP, death, AI tick.
    /// </summary>
    public abstract class Unit : MonoBehaviour
    {
        public int MaxHP { get; protected set; }
        public int CurrentHP { get; protected set; }
        public int ATK { get; protected set; }
        public float AS { get; protected set; }     // attacks per second
        public int Range { get; protected set; }    // 1 = melee
        public bool IsRanged => Range > 1;

        internal float _attackCooldown;
        internal bool _isDead;

        protected Unit _currentTarget;

        public System.Action<Unit> OnDeath;

        public void TakeDamage(int dmg)
        {
            if (_isDead) return;
            CurrentHP -= dmg;
            if (CurrentHP <= 0)
            {
                CurrentHP = 0;
                Die();
            }
        }

        protected virtual void Die()
        {
            _isDead = true;
            OnDeath?.Invoke(this);
            Destroy(gameObject, 0.1f);
        }

        public bool IsDead => _isDead;
    }
}
