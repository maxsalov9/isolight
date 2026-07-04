using System.Collections.Generic;
using UnityEngine;

namespace IsoLight.Characters
{
    [CreateAssetMenu(fileName = "SO_Character", menuName = "IsoLight/Characters/Character Data")]
    public class CharacterData : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public string Role;
        public Sprite Portrait;
        public int MaxHealth = 100;
        public int MaxEnergy = 50;
        public int AttackDamage = 16;
        public float AttackRange = 6f;
        public float AttackCooldown = 1f;
        public List<AbilityData> Abilities = new List<AbilityData>();
    }
}
