using UnityEngine;

namespace IsoLight.Characters
{
    [CreateAssetMenu(fileName = "SO_Ability", menuName = "IsoLight/Characters/Ability Data")]
    public class AbilityData : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        [TextArea]
        public string Description;
        public Sprite Icon;
        public float Cooldown = 6f;
        public int EnergyCost = 10;
        public AbilityTargetType TargetType = AbilityTargetType.Enemy;
        public AbilityEffectType EffectType = AbilityEffectType.DamageEnemy;
        public int PowerAmount = 20;
        public float Range = 8f;
        public float StunDuration = 0f;
    }
}
