using System.Collections.Generic;
using IsoLight.Power;
using UnityEngine;

namespace IsoLight.Relationships
{
    public class RelationshipManager : MonoBehaviour
    {
        [SerializeField] private RelationshipState relationshipState = new RelationshipState();

        public RelationshipState RelationshipState => relationshipState;

        public RelationshipLevel GetRelationship(string groupName)
        {
            return groupName switch
            {
                nameof(RelationshipState.RiversideTrust) => relationshipState.RiversideTrust,
                nameof(RelationshipState.DefenseSupport) => relationshipState.DefenseSupport,
                nameof(RelationshipState.FarmSupport) => relationshipState.FarmSupport,
                nameof(RelationshipState.MedicalSupport) => relationshipState.MedicalSupport,
                nameof(RelationshipState.StageSupport) => relationshipState.StageSupport,
                nameof(RelationshipState.WorkshopSupport) => relationshipState.WorkshopSupport,
                nameof(RelationshipState.RelaySupport) => relationshipState.RelaySupport,
                _ => RelationshipLevel.Medium
            };
        }

        public IReadOnlyList<RelationshipChange> ApplyPowerChoice(PowerChoice choice)
        {
            var changes = new List<RelationshipChange>();

            switch (choice)
            {
                case PowerChoice.WaterFilters:
                    SetRelationship(nameof(RelationshipState.RiversideTrust), RelationshipLevel.High, changes);
                    SetRelationship(nameof(RelationshipState.MedicalSupport), RelationshipLevel.High, changes);
                    SetRelationship(nameof(RelationshipState.FarmSupport), RelationshipLevel.Low, changes);
                    break;
                case PowerChoice.HydroponicFarm:
                    SetRelationship(nameof(RelationshipState.FarmSupport), RelationshipLevel.High, changes);
                    SetRelationship(nameof(RelationshipState.MedicalSupport), RelationshipLevel.Low, changes);
                    break;
                case PowerChoice.DefenseGrid:
                    SetRelationship(nameof(RelationshipState.DefenseSupport), RelationshipLevel.High, changes);
                    SetRelationship(nameof(RelationshipState.StageSupport), RelationshipLevel.Low, changes);
                    break;
                case PowerChoice.PublicStage:
                    SetRelationship(nameof(RelationshipState.StageSupport), RelationshipLevel.High, changes);
                    SetRelationship(nameof(RelationshipState.DefenseSupport), RelationshipLevel.Low, changes);
                    break;
                case PowerChoice.Workshop:
                    SetRelationship(nameof(RelationshipState.WorkshopSupport), RelationshipLevel.High, changes);
                    SetRelationship(nameof(RelationshipState.FarmSupport), RelationshipLevel.Low, changes);
                    SetRelationship(nameof(RelationshipState.MedicalSupport), RelationshipLevel.Low, changes);
                    break;
                case PowerChoice.RelayStation:
                    SetRelationship(nameof(RelationshipState.RelaySupport), RelationshipLevel.High, changes);
                    SetRelationship(nameof(RelationshipState.RiversideTrust), RelationshipLevel.Low, changes);
                    SetRelationship(nameof(RelationshipState.MedicalSupport), RelationshipLevel.Low, changes);
                    break;
                case PowerChoice.PartyReserve:
                    SetRelationship(nameof(RelationshipState.RiversideTrust), RelationshipLevel.Low, changes);
                    SetRelationship(nameof(RelationshipState.MedicalSupport), RelationshipLevel.Low, changes);
                    SetRelationship(nameof(RelationshipState.DefenseSupport), RelationshipLevel.Low, changes);
                    SetRelationship(nameof(RelationshipState.FarmSupport), RelationshipLevel.Low, changes);
                    SetRelationship(nameof(RelationshipState.StageSupport), RelationshipLevel.Low, changes);
                    SetRelationship(nameof(RelationshipState.WorkshopSupport), RelationshipLevel.Low, changes);
                    SetRelationship(nameof(RelationshipState.RelaySupport), RelationshipLevel.Low, changes);
                    break;
                case PowerChoice.SplitLoad:
                    SetRelationship(nameof(RelationshipState.RiversideTrust), RelationshipLevel.High, changes);
                    SetRelationship(nameof(RelationshipState.DefenseSupport), RelationshipLevel.Medium, changes);
                    SetRelationship(nameof(RelationshipState.FarmSupport), RelationshipLevel.Medium, changes);
                    SetRelationship(nameof(RelationshipState.MedicalSupport), RelationshipLevel.Medium, changes);
                    SetRelationship(nameof(RelationshipState.StageSupport), RelationshipLevel.Medium, changes);
                    SetRelationship(nameof(RelationshipState.WorkshopSupport), RelationshipLevel.Medium, changes);
                    SetRelationship(nameof(RelationshipState.RelaySupport), RelationshipLevel.Medium, changes);
                    break;
            }

            return changes;
        }

        public void SetRelationship(string groupName, RelationshipLevel level)
        {
            SetRelationship(groupName, level, null);
        }

        private void SetRelationship(string groupName, RelationshipLevel level, ICollection<RelationshipChange> changes)
        {
            var previous = GetRelationship(groupName);

            switch (groupName)
            {
                case nameof(RelationshipState.RiversideTrust):
                    relationshipState.RiversideTrust = level;
                    break;
                case nameof(RelationshipState.DefenseSupport):
                    relationshipState.DefenseSupport = level;
                    break;
                case nameof(RelationshipState.FarmSupport):
                    relationshipState.FarmSupport = level;
                    break;
                case nameof(RelationshipState.MedicalSupport):
                    relationshipState.MedicalSupport = level;
                    break;
                case nameof(RelationshipState.StageSupport):
                    relationshipState.StageSupport = level;
                    break;
                case nameof(RelationshipState.WorkshopSupport):
                    relationshipState.WorkshopSupport = level;
                    break;
                case nameof(RelationshipState.RelaySupport):
                    relationshipState.RelaySupport = level;
                    break;
            }

            if (changes != null && previous != level)
            {
                changes.Add(new RelationshipChange(groupName, previous, level));
            }
        }
    }
}
