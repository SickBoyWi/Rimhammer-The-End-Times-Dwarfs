using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace TheEndTimes_Dwarfs
{
    public class JobDriver_WatchGold : JobDriver_WatchBuilding
    {
        public override bool CanBeginNowWhileLyingDown()
        {
            if (!DwarfsUtil.IsDwarf(this.pawn))
            {
                // Only dwarf pawns can do this.
                return false;
            }

            return base.TargetC.HasThing && base.TargetC.Thing is Building_Bed && JobInBedUtility.InBedOrRestSpotNow(this.pawn, base.TargetC);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            if (!DwarfsUtil.IsDwarf(pawn))
            {
                // Only dwarf pawns can do this.
                return false;
            }

            LocalTargetInfo target = this.job.targetA;
            Job job = this.job;
            int num = this.job.def.joyMaxParticipants;
            int num2 = 0;
            if (!pawn.Reserve(target, job, num, num2, null, errorOnFailed))
            {
                return false;
            }
            
            pawn = this.pawn;
            target = this.job.targetB;
            job = this.job;
            if (!pawn.Reserve(target, job, 1, -1, null, errorOnFailed))
            {
                return false;
            }
            
            if (base.TargetC.HasThing)
            {
                if (base.TargetC.Thing is Building_Bed)
                {
                    pawn = this.pawn;
                    LocalTargetInfo targetC = this.job.targetC;
                    job = this.job;
                    num2 = ((Building_Bed)base.TargetC.Thing).SleepingSlotsCount;
                    num = 0;
                    if (!pawn.Reserve(targetC, job, num2, num, null, errorOnFailed))
                    {
                        return false;
                    }
                }
                else
                {
                    pawn = this.pawn;
                    LocalTargetInfo targetC = this.job.targetC;
                    job = this.job;
                    if (!pawn.Reserve(targetC, job, 1, -1, null, errorOnFailed))
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            JobDriver_WatchGold driverWatchThing = this;
            driverWatchThing.EndOnDespawnedOrNull<JobDriver_WatchGold>(TargetIndex.A, JobCondition.Incompletable);
            Toil watch;
            if ((!driverWatchThing.TargetC.HasThing ? 0 : (driverWatchThing.TargetC.Thing is Building_Bed ? 1 : 0)) != 0)
            {
                driverWatchThing.KeepLyingDown(TargetIndex.C);
                yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.C, TargetIndex.None);
                yield return Toils_Bed.GotoBed(TargetIndex.C);
                watch = Toils_LayDown.LayDown(TargetIndex.C, true, false, true, true, PawnPosture.LayingOnGroundNormal, false);
                watch.AddFailCondition((Func<bool>)(() => !watch.actor.Awake()));
            }
            else
            {
                yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
                watch = ToilMaker.MakeToil(nameof(MakeNewToils));
            }
            watch.AddPreTickIntervalAction(new Action<int>(driverWatchThing.WatchTickAction));
            watch.AddFinishAction((Action)(() => JoyUtility.TryGainRecRoomThought(this.pawn)));
            watch.defaultCompleteMode = ToilCompleteMode.Delay;
            watch.defaultDuration = driverWatchThing.job.def.joyDuration;
            watch.handlingFacing = true;
            yield return watch;
        }

        protected override void WatchTickAction(int delta)
        {
            this.pawn.rotationTracker.FaceCell(this.TargetA.Cell);
            this.pawn.GainComfortFromCellIfPossible(delta, false);
            JoyUtility.JoyTickCheckEnd(this.pawn, delta, JoyTickFullJoyAction.EndJob, 1f, null);
        }

        public override object[] TaleParameters()
        {
            return new object[]
            {
                this.pawn,
                base.TargetA.Thing.def
            };
        }
    }
}