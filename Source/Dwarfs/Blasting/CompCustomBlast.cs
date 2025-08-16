using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TheEndTimes_Dwarfs
{
    public class CompCustomBlast : ThingComp
    {
        private bool wickStarted;
        private int wickTicksLeft;
        private Sustainer wickSoundSustainer;
        private bool detonated;
        private int wickTotalTicks;
        private bool wickIsSilent;
        protected Map parentMap;
        protected IntVec3 parentPosition;

        private CompProperties_Explosive ExplosiveProps
        {
            get
            {
                return (CompProperties_Explosive)this.props;
            }
        }

        public int WickTotalTicks
        {
            get
            {
                return this.wickTotalTicks;
            }
        }

        public int WickTicksLeft
        {
            get
            {
                return this.wickTicksLeft;
            }
        }

        protected int StartWickThreshold
        {
            get
            {
                return Mathf.RoundToInt(this.ExplosiveProps.startWickHitPointsPercent * (float)this.parent.MaxHitPoints);
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            this.parentMap = this.parent.Map;
            this.parentPosition = this.parent.Position;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.wickStarted, "wickStarted", false, false);
            Scribe_Values.Look<int>(ref this.wickTicksLeft, "wickTicksLeft", 0, false);
            Scribe_Values.Look<int>(ref this.wickTotalTicks, "wickTotalTicks", 0, false);
            Scribe_Values.Look<bool>(ref this.wickIsSilent, "wickIsSilent", false, false);
        }

        public override void CompTick()
        {
            if (!this.wickStarted)
                return;
            if (this.wickSoundSustainer == null)
            {
                if (!this.wickIsSilent)
                    this.StartWickSustainer();
            }
            else
                this.wickSoundSustainer.Maintain();
            --this.wickTicksLeft;
            if (this.wickTicksLeft > 0)
                return;
            this.Detonate();
        }

        private void StartWickSustainer()
        {
            SoundDefOf.MetalHitImportant.PlayOneShot((SoundInfo)((Verse.Thing)this.parent));
            SoundInfo info = SoundInfo.InMap((TargetInfo)((Verse.Thing)this.parent), MaintenanceType.PerTick);
            this.wickSoundSustainer = SoundDefOf.HissSmall.TrySpawnSustainer(info);
        }

        public override void PostDraw()
        {
            if (!this.wickStarted || this.wickIsSilent)
                return;
            this.parent.Map.overlayDrawer.DrawOverlay((Verse.Thing)this.parent, OverlayTypes.BurningWick);
        }

        public override void PostPreApplyDamage(ref DamageInfo dInfo, out bool absorbed)
        {
            absorbed = false;
            if (dInfo.Def.ExternalViolenceFor((Verse.Thing)this.parent) && (double)dInfo.Amount >= (double)this.parent.HitPoints)
            {
                this.Detonate();
                if (!this.parent.Destroyed)
                    return;
                absorbed = true;
            }
            else if (this.wickStarted && (dInfo.Def == DamageDefOf.Stun || this.wickIsSilent && dInfo.Def == DamageDefOf.EMP))
            {
                this.StopWick();
            }
            else
            {
                if (this.wickStarted || this.StartWickThreshold == 0 || this.parent.HitPoints > this.StartWickThreshold || !dInfo.Def.ExternalViolenceFor((Verse.Thing)this.parent))
                    return;
                this.StartWick(false);
            }
        }

        public void StartWick(bool silent)
        {
            if (this.wickStarted && !silent)
                this.wickIsSilent = false;
            if (this.wickStarted)
                return;
            this.wickIsSilent = silent;
            this.wickStarted = true;
            this.wickTotalTicks = this.wickTicksLeft = this.ExplosiveProps.wickTicks.RandomInRange;
            if (this.ExplosiveProps.explosiveDamageType == null)
                return;
            GenExplosion.NotifyNearbyPawnsOfDangerousExplosive((Verse.Thing)this.parent, this.ExplosiveProps.explosiveDamageType, (Faction)null);
        }

        public void StopWick()
        {
            this.wickStarted = false;
        }

        public bool WickStarted
        {
            get
            {
                return this.wickStarted;
            }
        }

        protected virtual void Detonate()
        {
            if (this.detonated)
                return;
            this.detonated = true;
            if (!this.parent.Destroyed)
                this.parent.Destroy(DestroyMode.KillFinalize);
            CompProperties_Explosive explosiveProps = this.ExplosiveProps;
            if (explosiveProps.explosiveDamageType == null || (double)explosiveProps.explosiveRadius <= 0.0)
                return;
            float explosiveRadius = explosiveProps.explosiveRadius;
            if (this.parent.stackCount > 1 && (double)explosiveProps.explosiveExpandPerStackcount > 0.0)
                explosiveRadius += Mathf.Sqrt((float)(this.parent.stackCount - 1) * explosiveProps.explosiveExpandPerStackcount);
            GenExplosion.DoExplosion(this.parentPosition, this.parentMap, explosiveRadius, explosiveProps.explosiveDamageType, (Verse.Thing)this.parent, -1, -1f, (SoundDef)null, (ThingDef)null, (ThingDef)null, (Verse.Thing)null, (ThingDef)null, 0.0f, 1, new GasType?(GasType.BlindSmoke), new float?(), (int)byte.MaxValue, false, (ThingDef)null, 0.0f, 1, 0.0f, false);
        }
    }
}
