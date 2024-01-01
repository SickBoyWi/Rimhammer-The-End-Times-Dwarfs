using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace TheEndTimes_Dwarfs
{
    public abstract class LordToil_TunnelRelated : LordToil
    {
        private LordToil_TunnelRelatedData Data
        {
            get
            {
                return (LordToil_TunnelRelatedData)this.data;
            }
        }

        public LordToil_TunnelRelated()
        {
            this.data = (LordToilData)new LordToil_TunnelRelatedData();
        }

        protected void FilterOutUnspawnedTunnels()
        {
            this.Data.assignedTunnels.RemoveAll<Pawn, SkavenTunnel>((Predicate<KeyValuePair<Pawn, SkavenTunnel>>)(x =>
            {
                if (x.Value != null)
                    return !x.Value.Spawned;
                return true;
            }));
        }

        protected SkavenTunnel GetTunnelFor(Pawn pawn)
        {
            SkavenTunnel tunnel;
            if (this.Data.assignedTunnels.TryGetValue(pawn, out tunnel))
                return tunnel;
            SkavenTunnel closestTunnel = this.FindClosestTunnel(pawn);
            if (closestTunnel != null)
                this.Data.assignedTunnels.Add(pawn, closestTunnel);
            return closestTunnel;
        }

        private SkavenTunnel FindClosestTunnel(Pawn pawn)
        {
            return (SkavenTunnel)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(RH_TET_DwarfDefOf.RH_TET_SkavenTunnel), PathEndMode.Touch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 30f, (Predicate<Thing>)(x => x.Faction == pawn.Faction), (IEnumerable<Thing>)null, 0, 30, false, RegionType.Set_Passable, false);
        }
    }
}