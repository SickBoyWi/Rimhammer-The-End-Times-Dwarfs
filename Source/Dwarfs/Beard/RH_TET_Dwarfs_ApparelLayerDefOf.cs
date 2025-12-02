using RimWorld;
using Verse;

namespace TheEndTimes_Dwarfs
{
    [DefOf]
    public static class RH_TET_Dwarfs_ApparelLayerDefOf
    {
        public static ApparelLayerDef RH_TET_Dwarfs_BeardCover;

        static RH_TET_Dwarfs_ApparelLayerDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ApparelLayerDefOf));
        }
    }
}