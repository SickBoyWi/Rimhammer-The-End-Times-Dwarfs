using Verse;

namespace TheEndTimes_Dwarfs
{
    public class CompProperties_SpawnerTunnels : CompProperties
    {
        public float TunnelSpawnPreferredMinDist = 3.5f;
        public float TunnelSpawnRadius = 10f;
        public FloatRange TunnelSpawnIntervalDays = new FloatRange(2f, 3f);
        public SimpleCurve ReproduceRateFactorFromNearbyTunnelCountCurve = new SimpleCurve()
        {
              {
                new CurvePoint(0.0f, 1f),
                true
              },
              {
                new CurvePoint(7f, 0.35f),
                true
              }
        };

        public CompProperties_SpawnerTunnels()
        {
            this.compClass = typeof(CompSpawnerTunnels);
        }
    }
}
