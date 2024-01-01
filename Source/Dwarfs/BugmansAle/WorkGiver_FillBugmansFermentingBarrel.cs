using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace TheEndTimes_Dwarfs
{
    public class WorkGiver_FillBugmansFermentingBarrel : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ThingDef.Named("RH_TET_Dwarfs_BugmansFermentingBarrel"));
        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public static void Reset()
        {
            TemperatureTrans = "BadTemperature".Translate().ToLower();
            NoWortTrans = "RH_TET_Dwarfs_NoBugmansWortAle".Translate();
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Building_BugmansFermentingAleBarrel Building_FermentingAleBarrel) ||
                Building_FermentingAleBarrel.Fermented || Building_FermentingAleBarrel.SpaceLeftForWort <= 0)
                return false;

            var ambientTemperature = Building_FermentingAleBarrel.AmbientTemperature;
            var compProperties =
                Building_FermentingAleBarrel.def.GetCompProperties<CompProperties_TemperatureRuinable>();
            if (ambientTemperature < compProperties.minSafeTemperature + 2f ||
                ambientTemperature > compProperties.maxSafeTemperature - 2f)
            {
                JobFailReason.Is(TemperatureTrans);
                return false;
            }
            if (t.IsForbidden(pawn)) return false;
            LocalTargetInfo target = t;
            if (!pawn.CanReserve(target, 1, -1, null, forced)) return false;
            if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
                return false;

            if (FindWort(pawn, Building_FermentingAleBarrel) != null) return !t.IsBurning();
            JobFailReason.Is(NoWortTrans);
            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var barrel = (Building_BugmansFermentingAleBarrel)t;
            var t2 = FindWort(pawn, barrel);
            return new Job(DefDatabase<JobDef>.GetNamed("RH_TET_Dwarfs_FillBugmansFermentingBarrel"), t, t2);
        }

        private Thing FindWort(Pawn pawn, Building_BugmansFermentingAleBarrel barrel)
        {
            bool Predicate(Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, -1, null, false);
            var position = pawn.Position;
            var map = pawn.Map;
            var thingReq = ThingRequest.ForDef(ThingDef.Named("RH_TET_Dwarf_BugmansXXXWort"));
            const PathEndMode peMode = PathEndMode.ClosestTouch;
            var traverseParams = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false);
            Predicate<Thing> validator = Predicate;
            return GenClosest.ClosestThingReachable(position, map, thingReq, peMode, traverseParams, 9999f, validator,
                null, 0, -1, false, RegionType.Set_Passable, false);
        }

        private static string TemperatureTrans;

        private static string NoWortTrans;
    }
}