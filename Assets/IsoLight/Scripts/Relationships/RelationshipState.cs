using System;

namespace IsoLight.Relationships
{
    [Serializable]
    public class RelationshipState
    {
        public RelationshipLevel RiversideTrust = RelationshipLevel.Medium;
        public RelationshipLevel DefenseSupport = RelationshipLevel.Medium;
        public RelationshipLevel FarmSupport = RelationshipLevel.Medium;
        public RelationshipLevel MedicalSupport = RelationshipLevel.Medium;
        public RelationshipLevel StageSupport = RelationshipLevel.Medium;
        public RelationshipLevel WorkshopSupport = RelationshipLevel.Medium;
        public RelationshipLevel RelaySupport = RelationshipLevel.Medium;
    }
}
