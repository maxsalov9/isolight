using System.Collections.Generic;
using IsoLight.Relationships;

namespace IsoLight.Power
{
    public class PowerAllocationResult
    {
        public PowerSystemData ChosenSystem;
        public PowerChoice Choice;
        public string ConsequenceText;
        public string SupportersText;
        public string OppositionText;
        public int PartyPowerCellsCharge;
        public bool PartySelfishnessSeen;
        public bool MissionCompleted;
        public readonly List<RelationshipChange> RelationshipChanges = new List<RelationshipChange>();
    }
}
