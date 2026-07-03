using System;
using IsoLight.Power;

namespace IsoLight.Core
{
    [Serializable]
    public class MissionState
    {
        public bool HasMetMara;
        public bool HasStartedPowerPrioritiesQuest;

        public bool TalkedToHale;
        public bool TalkedToIvo;
        public bool TalkedToSela;
        public bool TalkedToEdda;
        public bool TalkedToGreer;
        public bool TalkedToLysa;

        public bool InspectedWaterFilters;
        public bool InspectedHydroponicFarm;
        public bool InspectedDefenseGate;
        public bool InspectedPublicStage;
        public bool InspectedWorkshop;
        public bool InspectedRelayStation;

        public int BreakerModulesCollected;
        public bool GeneratorRepaired;
        public bool GeneratorStarted;
        public bool GeneratorDefended;

        public PowerChoice FinalPowerChoice;
        public bool MissionCompleted;
    }
}
