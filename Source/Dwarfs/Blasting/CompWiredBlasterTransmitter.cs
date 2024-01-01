using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class CompWiredBlasterTransmitter : CompBlastingGridNode
    {
        public CompWiredBlasterTransmitter.AllowSignalPassage signalPassageTest;
        private int lastSignalId;

        private CompProperties_WiredBlasterTransmitter CustomProps
        {
            get
            {
                if (!(this.props is CompProperties_WiredBlasterTransmitter))
                    throw new Exception("CompWiredBlasterTransmitter requires CompProperties_WiredBlasterTransmitter");
                return (CompProperties_WiredBlasterTransmitter)this.props;
            }
        }

        public override void PrintForDetonationGrid(SectionLayer layer)
        {
            this.PrintConnection(layer);
        }

        public virtual void ReceiveSignal(
          int signalId,
          int signalSteps,
          CompWiredBlasterTransmitter source = null)
        {
            if (signalId == this.lastSignalId || signalSteps > 5000 || this.signalPassageTest != null && !this.signalPassageTest())
                return;
            this.lastSignalId = signalId;
            this.PassSignalToReceivers(signalSteps);
            this.ConductSignalToNeighbors(signalId, signalSteps);
        }

        private void ConductSignalToNeighbors(int signalId, int signalSteps)
        {
            if (this.parent.Map == null)
                throw new Exception("null map");
            foreach (IntVec3 intVec3 in GenAdj.CardinalDirectionsAround)
            {
                IntVec3 c = intVec3 + this.parent.Position;
                if (c.InBounds(this.parent.Map))
                {
                    List<Verse.Thing> thingList = this.parent.Map.thingGrid.ThingsListAtFast(c);
                    for (int index = 0; index < thingList.Count; ++index)
                        thingList[index].TryGetComp<CompWiredBlasterTransmitter>()?.ReceiveSignal(signalId, signalSteps + 1, this);
                }
            }
        }

        private void PassSignalToReceivers(int signalSteps)
        {
            if (this.parent.Map == null)
                throw new Exception("null map");
            int delayTicks = Mathf.RoundToInt((float)signalSteps * this.CustomProps.signalDelayPerTile);
            List<Verse.Thing> thingList = this.parent.Map.thingGrid.ThingsListAtFast(this.parent.Position);
            for (int index = 0; index < thingList.Count; ++index)
                thingList[index].TryGetComp<CompWiredBlasterReceiver>()?.ReceiveSignal(delayTicks);
        }

        public delegate bool AllowSignalPassage();
    }
}
