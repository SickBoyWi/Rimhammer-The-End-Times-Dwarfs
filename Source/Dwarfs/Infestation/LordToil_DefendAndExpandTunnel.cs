using RimWorld;
using Verse;
using Verse.AI;

namespace TheEndTimes_Dwarfs
{
    public class LordToil_DefendAndExpandTunnel : LordToil_TunnelRelated
    {
        public float distToTunnelToAttack = 10f;

        public override void UpdateAllDuties()
        {
            this.FilterOutUnspawnedTunnels();
            for (int index = 0; index < this.lord.ownedPawns.Count; ++index)
            {
                SkavenTunnel hiveFor = this.GetTunnelFor(this.lord.ownedPawns[index]);
                PawnDuty pawnDuty = new PawnDuty(RH_TET_DwarfDefOf.RH_TET_SkavenDefendAndExpandTunnel, (LocalTargetInfo)((Thing)hiveFor), this.distToTunnelToAttack);
                this.lord.ownedPawns[index].mindState.duty = pawnDuty;
            }
        }
    }
}
