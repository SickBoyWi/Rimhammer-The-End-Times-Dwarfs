using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TheEndTimes_Dwarfs
{
    [StaticConstructorOnStartup]
    public class JobDriver_HoldCourt : JobDriver
    {
        private static readonly IntRange MoteInterval = new IntRange(300, 500);
        public static readonly Texture2D moteImage1 = ContentFinder<Texture2D>.Get("Mote/RH_TET_Dwarfs_HoldCourtSpeech", true);
        public static readonly Texture2D moteImage2 = ContentFinder<Texture2D>.Get("Mote/RH_TET_Dwarfs_HoldCourtSpeech2", true);
        public static readonly Texture2D moteImage3 = ContentFinder<Texture2D>.Get("Mote/RH_TET_Dwarfs_HoldCourtSpeech3", true);
        public static readonly Texture2D moteImage4 = ContentFinder<Texture2D>.Get("Mote/RH_TET_Dwarfs_HoldCourtSpeech4", true);
        private const TargetIndex ThroneIndex = TargetIndex.A;
        private const TargetIndex FacingIndex = TargetIndex.B;

        private Building_DwarfThrone DwarfThrone
        {
            get
            {
                return (Building_DwarfThrone)this.TargetThingA;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve((LocalTargetInfo)((Thing)this.DwarfThrone), this.job, 1, -1, (ReservationLayerDef)null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            JobDriver_HoldCourt f1 = this;
            f1.FailOnDespawnedNullOrForbidden<JobDriver_HoldCourt>(TargetIndex.A);
            yield return Toils_General.Do(new Action(delegate () { f1.job.SetTarget(TargetIndex.B, (LocalTargetInfo)(this.DwarfThrone.InteractionCell + this.DwarfThrone.Rotation.FacingCell)); }));
            Toil f2 = new Toil();
            f2.FailOnCannotTouch<Toil>(TargetIndex.A, PathEndMode.InteractionCell);
            f2.FailOn<Toil>(new Func<bool>(() => this.DwarfThrone.AssignedPawn != this.pawn));
            f2.FailOn<Toil>(new Func<bool>(() => RoomRoleWorker_DwarfRulingRoom.Validate(this.DwarfThrone.GetRoom(RegionType.Set_Passable)) != null));
            f2.tickAction = new Action(delegate ()
            {
                this.pawn.GainComfortFromCellIfPossible(1, false);
                this.pawn.skills.Learn(SkillDefOf.Social, 0.3f, false);
                if (this.pawn.IsHashIntervalTick(JobDriver_HoldCourt.MoteInterval.RandomInRange))
                {
                    int rando = RH_TET_DwarfsMod.random.Next(0, 4);
                    if (rando == 1)
                        MoteMaker.MakeSpeechBubble(this.pawn, JobDriver_HoldCourt.moteImage1);
                    else if (rando == 2)
                        MoteMaker.MakeSpeechBubble(this.pawn, JobDriver_HoldCourt.moteImage2);
                    else if (rando == 3)
                        MoteMaker.MakeSpeechBubble(this.pawn, JobDriver_HoldCourt.moteImage3);
                    else
                        MoteMaker.MakeSpeechBubble(this.pawn, JobDriver_HoldCourt.moteImage4);
                }
                this.rotateToFace = TargetIndex.B;
            });
            f2.defaultCompleteMode = ToilCompleteMode.Never;
            yield return f2;
        }
    }
}
