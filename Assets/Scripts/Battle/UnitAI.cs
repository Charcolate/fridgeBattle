using System.Collections.Generic;
using UnityEngine;

namespace FridgeBattle.Battle
{
    [RequireComponent(typeof(Unit))]
    public class UnitAI : MonoBehaviour
    {
        public float moveSpeed = 3f;
        private Unit _unit;
        private BattleManager _battle;

        void Awake() { _unit = GetComponent<Unit>(); }
        public void BindBattle(BattleManager battle) { _battle = battle; }

        void Update()
        {
            if (_unit == null || _unit.IsDead) return;
            if (_battle == null || !_battle.IsRunning) return;

            var enemies = IsIngredient() ? _battle.GetEnemies() : _battle.GetIngredients();
            if (enemies == null || enemies.Count == 0) return;

            // Find nearest living enemy
            Unit nearest = null;
            float nearestDist = float.MaxValue;
            foreach (var e in enemies)
            {
                if (e == null || e.IsDead) continue;
                float d = Vector3.Distance(transform.position, e.transform.position);
                if (d < nearestDist) { nearestDist = d; nearest = e; }
            }
            if (nearest == null) return;

            float attackRange = _unit.Range * 0.9f;
            if (nearestDist <= attackRange)
            {
                _unit._attackCooldown -= Time.deltaTime;
                if (_unit._attackCooldown <= 0f)
                {
                    nearest.TakeDamage(_unit.ATK);
                    _unit._attackCooldown = 1f / _unit.AS;
                }
            }
            else
            {
                Vector3 dir = (nearest.transform.position - transform.position).normalized;
                dir.y = 0;
                transform.position += dir * moveSpeed * Time.deltaTime;
                transform.LookAt(nearest.transform.position);
            }
        }

        private bool IsIngredient() => _unit is IngredientUnit;
    }
}
