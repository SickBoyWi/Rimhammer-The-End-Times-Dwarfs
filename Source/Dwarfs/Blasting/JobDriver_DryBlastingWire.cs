using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace TheEndTimes_Dwarfs
{
    public class JobDriver_DryBlastingWire : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, (ReservationLayerDef)null, true);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.AddFailCondition(new Func<bool>(this.JobHasFailed));
            Building_BlastingWire wire = this.TargetThingA as Building_BlastingWire;
            if (wire != null)
            {
                yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
                int jobDuration = wire.DryOffJobDuration;
                yield return Toils_General.Wait(jobDuration, TargetIndex.None).WithEffect(EffecterDefOf.Clean, TargetIndex.A).WithProgressBarToilDelay(TargetIndex.A, jobDuration, false, -0.5f);
                yield return new Toil()
                {
                    initAction = (Action)(() =>
                    {
                        if (!wire.WantDrying)
                            return;
                        wire.DryOff();
                    }),
                    defaultCompleteMode = ToilCompleteMode.Instant
                };
            }
        }

        private bool JobHasFailed()
        {
            Building_BlastingWire targetThingA = this.TargetThingA as Building_BlastingWire;
            return this.TargetThingA == null || this.TargetThingA.Destroyed || targetThingA == null || !targetThingA.WantDrying;
        }
    }
}
