using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace TheEndTimes_Dwarfs
{
    public class JobDriver_PlayDice : JobDriver
    {
        private const int ShotDuration = 600;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.targetA, this.job, this.job.def.joyMaxParticipants, 0, (ReservationLayerDef)null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            JobDriver_PlayDice f = this;
            f.EndOnDespawnedOrNull<JobDriver_PlayDice>(TargetIndex.A, JobCondition.Incompletable);
            Toil chooseCell = Toils_Misc.FindRandomAdjacentReachableCell(TargetIndex.A, TargetIndex.B);
            yield return chooseCell;
            yield return Toils_Reserve.Reserve(TargetIndex.B, 1, -1, (ReservationLayerDef)null);
            yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
            Toil toil = new Toil();
            toil.initAction = new Action(delegate ()
            {
                f.job.locomotionUrgency = LocomotionUrgency.Walk;
                });
            toil.tickAction = new Action(delegate ()
            {
                f.pawn.rotationTracker.FaceCell(this.TargetA.Thing.OccupiedRect().ClosestCellTo(f.pawn.Position));
                if (f.ticksLeftThisToil == 300)
                    RH_TET_DwarfDefOf.RH_TET_Dwarfs_PlayDice.PlayOneShot((SoundInfo)new TargetInfo(f.pawn.Position, f.pawn.Map, false));
                if (Find.TickManager.TicksGame > f.startTick + f.job.def.joyDuration)
                    this.EndJobWith(JobCondition.Succeeded);
                else
                    JoyUtility.JoyTickCheckEnd(this.pawn, JoyTickFullJoyAction.EndJob, 1f, (Building)this.TargetThingA);
                });
            toil.handlingFacing = true;
            toil.socialMode = RandomSocialMode.SuperActive;
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = 600;
            toil.AddFinishAction(new Action(delegate ()
            {
                JoyUtility.TryGainRecRoomThought(f.pawn);
                }));
            yield return toil;
            yield return Toils_Reserve.Release(TargetIndex.B);
            yield return Toils_Jump.Jump(chooseCell);
        }

        public override object[] TaleParameters()
        {
            return new object[2]
            {
                (object) this.pawn,
                (object) this.TargetA.Thing.def
            };
        }
    }
}
