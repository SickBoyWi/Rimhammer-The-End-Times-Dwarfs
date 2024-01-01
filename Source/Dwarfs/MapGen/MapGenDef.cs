using System.Collections.Generic;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class MapGenDef : Def
    {
        public List<string> targetTags;
        public float Commonality;
        public IntVec3 defogPosition;
        public IntVec2 size;
        public List<MapObject> MapData;
        public List<MapObject> UnderTerrainData;
        public List<RoofObject> RoofData;
    }
}
