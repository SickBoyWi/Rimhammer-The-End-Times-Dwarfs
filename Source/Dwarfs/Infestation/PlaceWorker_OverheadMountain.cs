using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class PlaceWorker_OverheadMountain : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkDef, IntVec3 loc, Rot4 rot, Map map, Thing thingtoIgnore = null,
                                                      Thing thing = null)
        {
            if (!loc.Roofed(map) || !loc.GetRoof(map).isThickRoof)
            {
                return new AcceptanceReport("RH_TET_Dwarfs_PlaceWorker_OverheadMountain".Translate());
            }

            if (checkDef.defName.Equals("RH_TET_Dwarfs_SteamInfestationLure"))
            {
                IEnumerable<Building_InfestationLure> colonistLures = map.listerBuildings.AllBuildingsColonistOfClass<Building_InfestationLure>();
                if (colonistLures != null && colonistLures.Count() > 0)
                {
                    return new AcceptanceReport("RH_TET_Dwarfs_InfestLure_Fail_OneAllowed".Translate());
                }
            }

            return AcceptanceReport.WasAccepted;
        }

    }
}
