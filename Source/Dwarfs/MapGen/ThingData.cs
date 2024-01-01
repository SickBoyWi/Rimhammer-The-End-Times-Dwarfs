using RimWorld;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class ThingData
    {
        public ThingDef Thing;
        public ThingDef Stuff;
        public QualityCategory Quality = QualityCategory.Excellent;
        public TerrainDef Terrain;
        public int Count;
        public PawnKindDef Kind;
        public FactionDef Faction;
        public Rot4 Rotate;

        public override string ToString()
        {
            return $"Thing:{Thing}, Stuff:{Stuff}, Quality:{Quality}, Terrain:{Terrain}, Count:{Count}, Kind:{Kind}, Faction:{Faction}, Rotate:{Rotate}";
        }
    }
}
