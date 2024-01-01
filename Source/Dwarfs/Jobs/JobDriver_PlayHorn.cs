using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace TheEndTimes_Dwarfs
{
    public class JobDriver_PlayHorn : JobDriver_SitFacingBuilding
    {
        public Building_Horn Horn
        {
            get
            {
                return (Building_Horn)(Thing)this.job.GetTarget(TargetIndex.A);
            }
        }

        protected override void ModifyPlayToil(Toil toil)
        {
            if (!this.pawn.CanReserve(this.Horn))
                return;
            base.ModifyPlayToil(toil);
            toil.AddPreInitAction((Action)(() => this.Horn.StartPlaying(this.pawn)));
            toil.AddFinishAction((Action)(() => this.Horn.StopPlaying()));
        }
    }
}