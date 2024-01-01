using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace TheEndTimes_Dwarfs
{
    public class LordToil_TunnelRelatedData : LordToilData
    {
        public Dictionary<Pawn, SkavenTunnel> assignedTunnels = new Dictionary<Pawn, SkavenTunnel>();

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
                this.assignedTunnels.RemoveAll<Pawn, SkavenTunnel>((Predicate<KeyValuePair<Pawn, SkavenTunnel>>)(x => x.Key.Destroyed));
            Scribe_Collections.Look<Pawn, SkavenTunnel>(ref this.assignedTunnels, "assignedTunnels", LookMode.Reference, LookMode.Reference);
            if (Scribe.mode != LoadSaveMode.PostLoadInit)
                return;
            this.assignedTunnels.RemoveAll<Pawn, SkavenTunnel>((Predicate<KeyValuePair<Pawn, SkavenTunnel>>)(x => x.Value == null));
        }
    }
}