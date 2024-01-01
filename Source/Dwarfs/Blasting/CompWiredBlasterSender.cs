using System;
using System.Collections.Generic;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class CompWiredBlasterSender : CompBlastingGridNode
    {
        public void SendNewSignal()
        {
            if (this.parent.Map == null)
                throw new Exception("Map was null.");
            List<Verse.Thing> thingList = this.parent.Map.thingGrid.ThingsListAtFast(this.parent.Position);
            for (int index = 0; index < thingList.Count; ++index)
            {
                thingList[index].TryGetComp<CompWiredBlasterTransmitter>()?.ReceiveSignal(Rand.Int, 0, (CompWiredBlasterTransmitter)null);
            }
        }

        public override void PrintForDetonationGrid(SectionLayer layer)
        {
            this.PrintEndpoint(layer);
        }
    }
}
