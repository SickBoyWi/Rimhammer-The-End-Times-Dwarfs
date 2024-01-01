using RimWorld;
using System.Collections.Generic;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class ThoughtWorker_ThaneOnMap : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (p.MapHeld != null && RH_TET_DwarfsMod.thanes != null && RH_TET_DwarfsMod.thanes.Count > 0)
            {
                List<Pawn> mapColonists = p.MapHeld.mapPawns.FreeColonistsSpawned;
                foreach (Pawn thane in RH_TET_DwarfsMod.thanes)
                if (!p.Equals(thane) && mapColonists.Contains(thane))
                    return (ThoughtState)true;
            }

            return (ThoughtState)false;
        }
    }
}