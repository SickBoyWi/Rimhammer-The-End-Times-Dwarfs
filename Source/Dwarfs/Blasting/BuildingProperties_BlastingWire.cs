using RimWorld;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class BuildingProperties_BlastingWire : BuildingProperties
    {
        public float failChanceWhenFullyWet = 0.05f;
        public float daysToDryByself = 0.8f;
        public float baseDryingTemp = 20f;
        public bool fireOnFail = true;
        public int dryOffJobDurationTicks = 60;
        public EffecterDef failEffecter;
    }
}
