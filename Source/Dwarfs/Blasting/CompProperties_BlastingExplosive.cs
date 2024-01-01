using RimWorld;

namespace TheEndTimes_Dwarfs
{
    public class CompProperties_BlastingExplosive : CompProperties_Explosive
    {
        public float miningRadius = 2f;
        public float breakingPower = 68400f;
        public float resourceBreakingCost = 2f;
        public float woodBreakingCost = 2f;

        public CompProperties_BlastingExplosive()
        {
            this.compClass = typeof(CompBlastingExplosive);
        }
    }
}
