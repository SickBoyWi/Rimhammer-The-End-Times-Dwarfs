using RimWorld;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class ThoughtWorker_KingOnMap : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (p.MapHeld != null && RH_TET_DwarfsMod.king != null)
            {
                if (!p.Equals(RH_TET_DwarfsMod.king) && p.MapHeld.mapPawns.FreeColonistsSpawned.Contains(RH_TET_DwarfsMod.king))
                    return (ThoughtState)true;
            }

            return (ThoughtState)false;
        }
    }
}