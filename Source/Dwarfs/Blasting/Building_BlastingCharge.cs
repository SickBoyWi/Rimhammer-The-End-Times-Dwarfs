using HugsLib.Settings;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;
using Verse.Sound;

namespace TheEndTimes_Dwarfs
{
    public class Building_BlastingCharge : Building
    {
        private CompCustomBlast explosiveComp;
        private int ticksSinceFlare;
        private bool isArmed = true;
        private bool desiredArmState;

        public bool FuseLit
        {
            get
            {
                return this.explosiveComp.WickStarted;
            }
        }

        public virtual void LightFuse()
        {
            if (this.FuseLit)
                return;
            this.explosiveComp.StartWick(true);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.explosiveComp = this.GetComp<CompCustomBlast>();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.isArmed, "isArmed", false, false);
            Scribe_Values.Look<int>(ref this.ticksSinceFlare, "ticksSinceFlare", 0, false);
            Scribe_Values.Look<bool>(ref this.desiredArmState, "desiredArmState", false, false);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (DebugSettings.godMode)
            {
                Command_Action commandAction2 = new Command_Action();
                commandAction2.action = (Action)(() =>
                {
                    this.Arm();
                    this.LightFuse();
                });
                commandAction2.icon = RH_TET_DwarfDefOf.Textures.CommandButton_DetonateUI;
                commandAction2.defaultLabel = "DEV: Detonate!";
                yield return (Gizmo)commandAction2;
            }
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                Gizmo g = gizmo;
                yield return g;
                g = (Gizmo)null;
            }
        }

        public void Arm()
        {
            if (this.IsArmed)
                return;
            this.desiredArmState = true;
            this.isArmed = true;
        }

        public bool IsArmed
        {
            get
            {
                return this.isArmed;
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (!this.FuseLit)
                return;
        }

        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostApplyDamage(dinfo, totalDamageDealt);
            if (dinfo.Def != DamageDefOf.EMP)
                return;
            this.Disarm();
        }

        public void Disarm()
        {
            if (!this.IsArmed)
                return;
            this.isArmed = false;
            this.desiredArmState = false;
            this.explosiveComp.StopWick();
        }

        public override void Draw()
        {
            base.Draw();
            if (!this.isArmed)
                return;
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            return stringBuilder.ToString();
        }

        public void ReceiveWirelessSignal(Verse.Thing sender)
        {
            this.LightFuse();
        }

        private void ArmGizmoAction()
        {
            this.desiredArmState = !this.desiredArmState;
        }
    }
}
