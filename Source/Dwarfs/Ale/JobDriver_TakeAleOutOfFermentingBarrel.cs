using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace TheEndTimes_Dwarfs
{
    public class JobDriver_TakeAleOutOfFermentingBarrel : JobDriver
    {
        private const TargetIndex BarrelInd = TargetIndex.A;
        private const TargetIndex AleToHaulInd = TargetIndex.B;
        private const TargetIndex StorageCellInd = TargetIndex.C;
        private const int Duration = 200;

        protected Building_FermentingAleBarrel AleBarrel =>
            (Building_FermentingAleBarrel)job.GetTarget(BarrelInd).Thing;

        public override bool TryMakePreToilReservations(bool yeaa) => pawn.Reserve(AleBarrel, job, 1, -1, null);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(BarrelInd);
            this.FailOnBurningImmobile(BarrelInd);
            yield return Toils_Goto.GotoThing(BarrelInd, PathEndMode.Touch);
            yield return Toils_General.Wait(Duration).FailOnDestroyedNullOrForbidden(BarrelInd)
                .FailOnCannotTouch(BarrelInd, PathEndMode.Touch).FailOn(() => !AleBarrel.Fermented)
                .WithProgressBarToilDelay(BarrelInd, false, -0.5f);
            yield return new Toil
            {
                initAction = delegate
                {
                    Thing thing = AleBarrel.TakeOutAle();
                    GenPlace.TryPlaceThing(thing, pawn.Position, Map, ThingPlaceMode.Near, null);
                    StoragePriority currentPriority = StoreUtility.CurrentStoragePriorityOf(thing);
                    if (StoreUtility.TryFindBestBetterStoreCellFor(thing,
                        pawn, Map, currentPriority, pawn.Faction, out var c, true))
                    {
                        job.SetTarget(TargetIndex.C, c);
                        job.SetTarget(TargetIndex.B, thing);
                        job.count = thing.stackCount;
                    }
                    else
                    {
                        EndJobWith(JobCondition.Incompletable);
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return Toils_Reserve.Reserve(AleToHaulInd, 1, -1, null);
            yield return Toils_Reserve.Reserve(StorageCellInd, 1, -1, null);
            yield return Toils_Goto.GotoThing(AleToHaulInd, PathEndMode.ClosestTouch);
            yield return Toils_Haul.StartCarryThing(AleToHaulInd, false, false, false);
            Toil carryToCell = Toils_Haul.CarryHauledThingToCell(StorageCellInd);
            yield return carryToCell;
            yield return Toils_Haul.PlaceHauledThingInCell(StorageCellInd, carryToCell, true);
            yield break;
        }
    }
}