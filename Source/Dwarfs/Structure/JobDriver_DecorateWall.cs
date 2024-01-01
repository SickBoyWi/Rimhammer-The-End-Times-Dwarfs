using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace TheEndTimes_Dwarfs
{
    public class JobDriver_DecorateWall : JobDriver
    {
        private float workLeft = -1000f;

        protected int BaseWorkAmount
        {
            get
            {
                return 1200;
            }
        }

        protected DesignationDef DesDef
        {
            get
            {
                return RH_TET_DwarfDefOf.RH_TET_Dwarfs_DecorateWallDes;
            }
        }

        protected StatDef SpeedStat
        {
            get
            {
                return StatDefOf.WorkSpeedGlobal;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, (ReservationLayerDef)null, errorOnFailed) && this.pawn.Reserve((LocalTargetInfo)this.job.targetA.Cell, this.job, 1, -1, (ReservationLayerDef)null, errorOnFailed);
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn<JobDriver_DecorateWall>((Func<bool>)(() =>
            {
                if (!this.job.ignoreDesignations)
                    return this.Map.designationManager.DesignationOn(this.TargetA.Thing, this.DesDef) == null;
                return false;
            }));
            this.FailOnDespawnedNullOrForbidden<JobDriver_DecorateWall>(TargetIndex.A);
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
            Toil doWork = new Toil()
            {
                initAction = (Action)(() => this.workLeft = (float)this.BaseWorkAmount)
            };
            doWork.tickAction = (Action)(() =>
            {
                this.workLeft -= this.SpeedStat == null ? 1f : doWork.actor.GetStatValue(this.SpeedStat, true);
                if (doWork.actor.skills != null)
                    doWork.actor.skills.Learn(SkillDefOf.Artistic, 0.1f, false);
                if ((double)this.workLeft > 0.0)
                    return;
                this.DoEffect();
                Designation designation = this.Map.designationManager.DesignationOn(this.TargetA.Thing, this.DesDef);
                if (designation != null)
                    designation.Delete();
                this.ReadyForNextToil();
            });
            doWork.FailOnCannotTouch<Toil>(TargetIndex.A, PathEndMode.Touch);
            doWork.WithProgressBar(TargetIndex.A, (Func<float>)(() => (float)(1.0 - (double)this.workLeft / (double)this.BaseWorkAmount)), false, -0.5f);
            doWork.defaultCompleteMode = ToilCompleteMode.Never;
            doWork.activeSkill = (Func<SkillDef>)(() => SkillDefOf.Artistic);
            yield return doWork;
        }

        protected void DoEffect()
        {
            string defName = this.TargetA.Thing.def.defName;
            Random random = new Random();

            float xp = 250;

            defName += "_Decorated";
            defName = "RH_TET_Dwarfs_" + defName;
            this.pawn.skills.Learn(SkillDefOf.Artistic, xp, false);

            SmoothableWallUtility.Notify_SmoothedByPawn(JobDriver_DecorateWall.DecorateWall(this.TargetA.Thing, defName, this.pawn), this.pawn);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.workLeft, "workLeft", 0.0f, false);
        }

        public static Thing DecorateWall(Thing target, string newWall, Pawn smoother)
        {
            Map map = target.Map;
            map.designationManager.TryRemoveDesignationOn(target, RH_TET_DwarfDefOf.RH_TET_Dwarfs_DecorateWallDes);
            target.Destroy(DestroyMode.WillReplace);
            Thing newThing = ThingMaker.MakeThing(ThingDef.Named(newWall), target.Stuff);
            newThing.SetFaction(smoother.Faction, (Pawn)null);
            GenSpawn.Spawn(newThing, target.Position, map, target.Rotation, WipeMode.Vanish, false);
            return newThing;
        }
    }
}
