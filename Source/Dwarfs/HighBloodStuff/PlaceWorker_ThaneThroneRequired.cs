using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class PlaceWorker_ThaneThroneRequired : PlaceWorker
    {
        public override void DrawGhost(
          ThingDef def,
          IntVec3 center,
          Rot4 rot,
          Color ghostCol,
          Thing thing = null)
        {
        }

        public override AcceptanceReport AllowsPlacing(
          BuildableDef def,
          IntVec3 center,
          Rot4 rot,
          Map map,
          Thing thingToIgnore = null,
          Thing thing = null)
        {
            List<Thing> thingList = map.listerThings.ThingsOfDef(RH_TET_DwarfDefOf.RH_TET_Dwarfs_Throne);

            if (thingList.NullOrEmpty())
            {
                return (AcceptanceReport)"RH_TET_Dwarfs_ThaneThroneRequired".Translate();
            }

            return (AcceptanceReport)true;
        }
    }
}
