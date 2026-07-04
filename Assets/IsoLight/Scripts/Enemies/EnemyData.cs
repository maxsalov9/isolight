using UnityEngine;

namespace IsoLight.Enemies
{
    [CreateAssetMenu(fileName = "SO_Enemy", menuName = "IsoLight/Enemies/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public int MaxHealth = 40;
        public int Damage = 8;
        public float AttackRange = 5f;
        public float AttackCooldown = 1.5f;
        public float MoveSpeed = 2.8f;
        public EnemyRole Role;
    }
}
