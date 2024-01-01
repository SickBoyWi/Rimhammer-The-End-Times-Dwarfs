using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class CompProperties_MoteThrower : CompProperties
    {
        public int emissionInterval = -1;
        public int burstCount = 1;
        public Color colorA = Color.white;
        public Color colorB = Color.white;
        public ThingDef mote;
        public Vector3 offsetMin;
        public Vector3 offsetMax;
        public FloatRange scale;
        public FloatRange rotationRate;
        public FloatRange velocityX;
        public FloatRange velocityY;

        public CompProperties_MoteThrower()
        {
            this.compClass = typeof(CompMoteThrower);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            if (this.mote == null)
                yield return "CompMoteThrower has no mote assigned.";
        }
    }
}
