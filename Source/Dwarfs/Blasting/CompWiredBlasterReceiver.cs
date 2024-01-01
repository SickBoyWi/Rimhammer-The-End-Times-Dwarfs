using HugsLib;
using RimWorld;
using System;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class CompWiredBlasterReceiver : CompBlastingGridNode
    {
        public void ReceiveSignal(int delayTicks)
        {
            Building_BlastingCharge parent = this.parent as Building_BlastingCharge;
            if (parent != null && !parent.IsArmed)
                return;
            CompCustomBlast customExplosive = this.parent.GetComp<CompCustomBlast>();
            CompExplosive vanillaExplosive = this.parent.GetComp<CompExplosive>();
            if (customExplosive != null)
            {
                HugsLibController.Instance.TickDelayScheduler.ScheduleCallback((Action)(() => customExplosive.StartWick(true)), delayTicks, (Verse.Thing)this.parent, false);
            }
            if (vanillaExplosive == null)
                return;
            HugsLibController.Instance.TickDelayScheduler.ScheduleCallback((Action)(() => vanillaExplosive.StartWick((Verse.Thing)null)), delayTicks, (Verse.Thing)this.parent, false);
        }

        public override void PrintForDetonationGrid(SectionLayer layer)
        {
            this.PrintEndpoint(layer);
        }
    }
}
