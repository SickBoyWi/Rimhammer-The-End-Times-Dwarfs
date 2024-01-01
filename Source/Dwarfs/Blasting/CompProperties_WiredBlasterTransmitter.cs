using Verse;

namespace TheEndTimes_Dwarfs
{
    public class CompProperties_WiredBlasterTransmitter : CompProperties
    {
        public float signalDelayPerTile;

        public CompProperties_WiredBlasterTransmitter()
        {
            this.compClass = typeof(CompWiredBlasterTransmitter);
        }
    }
}
