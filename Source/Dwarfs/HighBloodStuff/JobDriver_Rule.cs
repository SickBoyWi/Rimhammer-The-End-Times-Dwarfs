using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace TheEndTimes_Dwarfs
{
    public class JobDriver_Rule : JobDriver
    {
        private const TargetIndex ThroneInd = TargetIndex.A;
        private const TargetIndex FacingInd = TargetIndex.B;
        private const int JobEndInterval = 5000;
        private const int Duration = 5000;
        private const int ApplyThoughtInitialTicks = 10000;

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
            JobDriver_Rule f1 = this;
            f1.FailOnDespawnedNullOrForbidden<JobDriver_Rule>(TargetIndex.A);
            yield return Toils_General.Do(new Action(delegate() { f1.job.SetTarget(FacingInd, (LocalTargetInfo)(this.DwarfThrone.InteractionCell + this.DwarfThrone.Rotation.FacingCell));}));
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            Toil f2 = new Toil();
            f2.FailOnCannotTouch<Toil>(TargetIndex.A, PathEndMode.InteractionCell);
            f2.FailOn<Toil>(new Func<bool>(() => this.DwarfThrone.AssignedPawn != this.pawn));
            f2.FailOn<Toil>(new Func<bool>(() => RoomRoleWorker_DwarfRulingRoom.Validate(this.DwarfThrone.GetRoom(RegionType.Set_Passable)) != null));
            f2.defaultCompleteMode = ToilCompleteMode.Delay;
            f2.defaultDuration = Duration;
            f2.tickAction = new Action(delegate ()
            {
                CompDwarfHighBlood comp = pawn.TryGetComp<CompDwarfHighBlood>();
                if (comp != null && comp.highBloodComp != null && comp.highBloodComp.GetCurrentHighBlood() != null)
                { 
                    if (comp.highBloodComp.applyRuledThoughtsTick == 0)
                        comp.highBloodComp.applyRuledThoughtsTick = Find.TickManager.TicksGame + 4500;
                    else if (comp.highBloodComp.applyRuledThoughtsTick <= Find.TickManager.TicksGame)
                    {
                        comp.highBloodComp.applyRuledThoughtsTick = Find.TickManager.TicksGame + 60000;
                        ThoughtDef def = (ThoughtDef)null;
                        if (this.DwarfThrone.GetRoom(RegionType.Set_Passable).Role == RH_TET_DwarfDefOf.RH_TET_Dwarfs_RulingRoom)
                            def = RH_TET_DwarfDefOf.RH_TET_Dwarfs_Ruled;
                        if (def != null)
                        {
                            int scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(this.DwarfThrone.GetRoom(RegionType.Set_Passable).GetStat(RoomStatDefOf.Impressiveness));
                            if (def.stages[scoreStageIndex] != null)
                                this.pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(def, scoreStageIndex), (Pawn)null);
                        }
                    }
                    this.rotateToFace = FacingInd;
                    this.pawn.GainComfortFromCellIfPossible(1, false);

                    comp.highBloodComp.lastRuledTicks = Find.TickManager.TicksGame;
                }
            });
            yield return f2;
        }
    }
}
