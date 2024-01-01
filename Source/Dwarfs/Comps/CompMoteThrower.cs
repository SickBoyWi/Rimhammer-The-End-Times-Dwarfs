using RimWorld;
using UnityEngine;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class CompMoteThrower : ThingComp
    {
        public bool emittedBefore;
        public int ticksSinceLastEmitted;

        private CompProperties_MoteThrower Props
        {
            get
            {
                return (CompProperties_MoteThrower)this.props;
            }
        }

        private Vector3 EmissionOffset
        {
            get
            {
                return new Vector3(Rand.Range(this.Props.offsetMin.x, this.Props.offsetMax.x), Rand.Range(this.Props.offsetMin.y, this.Props.offsetMax.y), Rand.Range(this.Props.offsetMin.z, this.Props.offsetMax.z));
            }
        }

        private Color EmissionColor
        {
            get
            {
                return Color.Lerp(this.Props.colorA, this.Props.colorB, Rand.Value);
            }
        }

        private bool IsOn
        {
            get
            {
                if (!this.parent.Spawned)
                    return false;
                Building_Horn parent = this.parent as Building_Horn;
                return (parent != null && parent.IsBeingPlayed);
            }
        }

        public override void CompTick()
        {
            if (!this.IsOn)
                return;
            if (this.Props.emissionInterval != -1)
            {
                if (this.ticksSinceLastEmitted >= this.Props.emissionInterval)
                {
                    this.Emit();
                    this.ticksSinceLastEmitted = 0;
                }
                else
                    ++this.ticksSinceLastEmitted;
            }
            else
            {
                if (this.emittedBefore)
                    return;
                this.Emit();
                this.emittedBefore = true;
            }
        }

        private void Emit()
        {
            for (int index = 0; index < this.Props.burstCount; ++index)
            {
                MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(this.Props.mote, (ThingDef)null);
                moteThrown.Scale = this.Props.scale.RandomInRange;
                moteThrown.rotationRate = this.Props.rotationRate.RandomInRange;
                moteThrown.exactPosition = this.parent.DrawPos + this.EmissionOffset;
                moteThrown.instanceColor = this.EmissionColor;
                moteThrown.SetVelocity(this.Props.velocityX.RandomInRange, this.Props.velocityY.RandomInRange);
                GenSpawn.Spawn((Thing)moteThrown, moteThrown.exactPosition.ToIntVec3(), this.parent.Map, WipeMode.Vanish);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref this.ticksSinceLastEmitted, "ticksSinceLastEmitted", 0, false);
            Scribe_Values.Look<bool>(ref this.emittedBefore, "emittedBefore", false, false);
        }
    }
}
