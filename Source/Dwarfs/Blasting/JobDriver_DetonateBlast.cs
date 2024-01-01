using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace TheEndTimes_Dwarfs
{
    public class JobDriver_DetonateBlast : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, (ReservationLayerDef)null, true);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            IPawnDetonateable detonator = this.TargetThingA as IPawnDetonateable;
            if (detonator != null)
            {
                PathEndMode pathEndMode = detonator.UseInteractionCell ? PathEndMode.InteractionCell : PathEndMode.ClosestTouch;
                this.AddFailCondition(new Func<bool>(this.JobHasFailed));
                yield return Toils_Goto.GotoCell(TargetIndex.A, pathEndMode);
                yield return new Toil()
                {
                    initAction = (Action)(() => detonator.DoDetonation()),
                    defaultCompleteMode = ToilCompleteMode.Instant
                };
            }
        }

        private bool JobHasFailed()
        {
            IPawnDetonateable targetThingA = this.TargetThingA as IPawnDetonateable;
            return targetThingA == null || ((Verse.Thing)targetThingA).Destroyed || !targetThingA.WantsDetonation;
        }
    }
}
