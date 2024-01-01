using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

// TODO JEH 1.1 set up in patches - may not need.
namespace TheEndTimes_Dwarfs
{
    public class StorytellerComp_CapturedHold : StorytellerComp
    {
        private const int StartOnDay = 120;

        private int IntervalsPassed
        {
            get
            {
                return Find.TickManager.TicksGame / 1000;
            }
        }

        [DebuggerHidden]
        public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
        {
            //if (this.IntervalsPassed == 840)
            //{
            //    IncidentDef inc = TheEndTimesDefOf.RH_TET_ChaosPortalGreatJourneyOffer;
            //    if (inc.TargetAllowed(target))
            //    {
            //        FiringIncident fi = new FiringIncident(inc, this, this.GenerateParms(inc.category, target));
            //        yield return fi;
            //    }
            //}
            return null;
        }
    }
}
