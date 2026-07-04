using IsoLight.Characters;
using IsoLight.Enemies;
using UnityEngine;

namespace IsoLight.Combat
{
    public class CombatCommand
    {
        public CombatCommandType Type;
        public Vector3 Destination;
        public Enemy EnemyTarget;
        public AbilityData Ability;
        public Component AbilityTarget;
        public string Label;

        public static CombatCommand MoveTo(Vector3 destination)
        {
            return new CombatCommand
            {
                Type = CombatCommandType.MoveTo,
                Destination = destination,
                Label = "Move"
            };
        }

        public static CombatCommand SingleAttack(Enemy enemy)
        {
            return new CombatCommand
            {
                Type = CombatCommandType.SingleAttack,
                EnemyTarget = enemy,
                Label = "Attack"
            };
        }

        public static CombatCommand AutoAttack(Enemy enemy)
        {
            return new CombatCommand
            {
                Type = CombatCommandType.AutoAttack,
                EnemyTarget = enemy,
                Label = "Auto Attack"
            };
        }

        public static CombatCommand UseAbility(AbilityData ability, Component target)
        {
            return new CombatCommand
            {
                Type = CombatCommandType.UseAbility,
                Ability = ability,
                AbilityTarget = target,
                Label = "Ability"
            };
        }
    }
}
