using Verse;

namespace TheEndTimes_Dwarfs
{
    public class HediffCompProperties_DissolveSlayerTattooOnDeath : HediffCompProperties
    {
        public int moteCount = 3;
        public FloatRange moteOffsetRange = new FloatRange(0.2f, 0.4f);
        public int filthCount = 4;
        public FleckDef fleck;
        public ThingDef mote;
        public ThingDef filth;
        public HediffDef injuryCreatedOnDeath;
        public IntRange injuryCount;
        public SoundDef sound;

        public HediffCompProperties_DissolveSlayerTattooOnDeath()
        {
            this.compClass = typeof(HediffComp_DissolveSlayerTattooOnDeath);
        }
    }
}
