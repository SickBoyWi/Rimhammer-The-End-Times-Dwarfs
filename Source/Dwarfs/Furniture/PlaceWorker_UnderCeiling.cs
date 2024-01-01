using System.Linq;
using RimWorld;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class PlaceWorker_UnderCeiling : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(
          BuildableDef checkingDef,
          IntVec3 loc,
          Rot4 rot,
          Map map,
          Thing thingToIgnore = null,
          Thing thing = null)
        {
            if (!map.roofGrid.Roofed(loc))
                return new AcceptanceReport("RH_TET_Dwarfs_PlaceWorker_UnderCeiling".Translate());
            return (AcceptanceReport)true;
        }
    }
}