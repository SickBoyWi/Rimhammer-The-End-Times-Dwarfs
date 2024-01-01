using RimWorld;
using UnityEngine;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class HighBlood : IExposable
    {
        public int receivedTick = -1;
        public HighBloodDef def;
        public bool highMaintenance;
        private const int RoomRequirementsGracePeriodTicks = 180000;

        public float RoomRequirementGracePeriodDaysLeft
        {
            get
            {
                return Mathf.Max((180000 - (GenTicks.TicksGame - this.receivedTick)).TicksToDays(), 0.0f);
            }
        }

        public bool RoomRequirementGracePeriodActive(Pawn pawn)
        {
            if (GenTicks.TicksGame - this.receivedTick < 180000)
                return true;
            return false;
        }

        public HighBlood()
        {
        }

        public HighBlood(HighBlood other)
        {
            this.def = other.def;
            this.receivedTick = other.receivedTick;
        }

        public void ExposeData()
        {
            Scribe_Defs.Look<HighBloodDef>(ref this.def, "def");
            Scribe_Values.Look<int>(ref this.receivedTick, "receivedTick", 0, false);
            Scribe_Values.Look<bool>(ref this.highMaintenance, "requiresMore", false, false);
        }
    }
}
