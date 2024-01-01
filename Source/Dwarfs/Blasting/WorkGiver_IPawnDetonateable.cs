using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace TheEndTimes_Dwarfs
{
    public class WorkGiver_IPawnDetonateable : WorkGiver_Scanner
    {
        public override IEnumerable<Verse.Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            List<Building> buildings = pawn.Map.listerBuildings.allBuildingsColonist;
            for (int i = 0; i < buildings.Count; ++i)
            {
                if (buildings[i] is IPawnDetonateable)
                    yield return (Verse.Thing)buildings[i];
            }
        }

        public override bool HasJobOnThing(Pawn pawn, Verse.Thing t, bool forced = false)
        {
            IPawnDetonateable pawnDetonateable = t as IPawnDetonateable;
            if (pawnDetonateable == null)
                return false;
            PathEndMode peMode = pawnDetonateable.UseInteractionCell ? PathEndMode.InteractionCell : PathEndMode.ClosestTouch;
            return !pawn.Dead && !pawn.Downed && (!pawn.IsBurning() && pawnDetonateable.WantsDetonation) && pawn.CanReserveAndReach((LocalTargetInfo)t, peMode, Danger.Some, 1, -1, (ReservationLayerDef)null, false);
        }

        public override Verse.AI.Job JobOnThing(Pawn pawn, Verse.Thing t, bool forced = false)
        {
            IPawnDetonateable pawnDetonateable = t as IPawnDetonateable;
            if (pawnDetonateable == null || !pawnDetonateable.WantsDetonation)
                return (Verse.AI.Job)null;
            return new Verse.AI.Job(RH_TET_DwarfDefOf.RH_TET_Dwarfs_DetonateBlast, (LocalTargetInfo)t);
        }
    }
}
