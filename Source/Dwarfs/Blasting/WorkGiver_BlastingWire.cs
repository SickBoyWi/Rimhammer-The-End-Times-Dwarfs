// Decompiled with JetBrains decompiler
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace TheEndTimes_Dwarfs
{
    public class WorkGiver_BlastingWire : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.ClosestTouch;
            }
        }

        public override IEnumerable<Verse.Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            IEnumerable<Verse.Designation> designations = pawn.Map.designationManager.SpawnedDesignationsOfDef(RH_TET_DwarfDefOf.RH_TET_Dwarfs_BlastingWireDryOut);
            foreach (Verse.Designation designation1 in designations)
            {
                Verse.Designation designation = designation1;
                if (designation.target.Thing != null)
                {
                    yield return designation.target.Thing;
                    designation = (Verse.Designation)null;
                }
            }
        }

        public override bool HasJobOnThing(Pawn pawn, Verse.Thing t, bool forced = false)
        {
            Building_BlastingWire buildingDetonatorWire = t as Building_BlastingWire;
            if (buildingDetonatorWire == null)
                return false;
            return buildingDetonatorWire.WantDrying && pawn.CanReserveAndReach((LocalTargetInfo)t, PathEndMode.Touch, Danger.Deadly, 1, -1, (ReservationLayerDef)null, false);
        }

        public override Verse.AI.Job JobOnThing(Pawn pawn, Verse.Thing t, bool forced = false)
        {
            Building_BlastingWire buildingDetonatorWire = t as Building_BlastingWire;
            if (buildingDetonatorWire == null || !buildingDetonatorWire.WantDrying)
                return (Verse.AI.Job)null;
            return new Verse.AI.Job(RH_TET_DwarfDefOf.RH_TET_Dwarfs_DryBlastingWire, (LocalTargetInfo)t);
        }
    }
}
