using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace TheEndTimes_Dwarfs
{
    public class WorkGiver_DecorateWall : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        [DebuggerHidden]
        public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
        {
            if (pawn.Faction == Faction.OfPlayer)
            {
                foreach (Designation designation in pawn.Map.designationManager.SpawnedDesignationsOfDef(RH_TET_DwarfDefOf.RH_TET_Dwarfs_DecorateWallDes))
                {
                    Designation des = designation;
                    yield return des.target.Cell;
                    des = (Designation)null;
                }
            }
        }
        
        public override bool HasJobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
        {

            Building building = c.GetFirstBuilding(pawn.Map);
            if (building != null && (c.IsForbidden(pawn) || pawn.Map.designationManager.DesignationOn(building, RH_TET_DwarfDefOf.RH_TET_Dwarfs_DecorateWallDes) == null))
                return false;
            if (building != null && !DwarfsUtil.IsDwarf(pawn) && !RH_TET_DwarfsMod.AnyOneCanDwarfModActive)
            { 
                if (forced)
                    Messages.Message((TaggedString)(Translator.Translate("RH_TET_Dwarfs_OnlyADawi")), (MessageTypeDef)MessageTypeDefOf.NegativeEvent, true);
                return false;
            }
            if (building == null || !building.def.defName.Contains("Smoothed"))
            {
                Verse.Log.ErrorOnce("Failed to find valid smoothed wall when trying to decorate a wall", 58988176);
                pawn.Map.designationManager.TryRemoveDesignationOn(building, RH_TET_DwarfDefOf.RH_TET_Dwarfs_DecorateWallDes);
                return false;
            }
            LocalTargetInfo target1 = (LocalTargetInfo)((Thing)building);
            if (pawn.CanReserve(target1, 1, -1, (ReservationLayerDef)null, forced))
            {
                LocalTargetInfo target2 = (LocalTargetInfo)c;
                if (pawn.CanReserve(target2, 1, -1, (ReservationLayerDef)null, forced))
                    return true;
            }
            return false;
        }

        public override Job JobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
        {
            return new Job(RH_TET_DwarfDefOf.RH_TET_Dwarfs_DecorateWallJob, (LocalTargetInfo)((Thing)c.GetEdifice(pawn.Map)));
        }
    }
}
