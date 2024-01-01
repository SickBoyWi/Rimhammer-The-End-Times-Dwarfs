using Verse;
using Verse.AI;

namespace TheEndTimes_Dwarfs
{
    public class LordToil_DefendTunnelAggressively : LordToil_TunnelRelated
    {
        public float distToTunnelToAttack = 40f;

        public override void UpdateAllDuties()
        {
            this.FilterOutUnspawnedTunnels();
            for (int index = 0; index < this.lord.ownedPawns.Count; ++index)
            {
                SkavenTunnel hiveFor = this.GetTunnelFor(this.lord.ownedPawns[index]);
                PawnDuty pawnDuty = new PawnDuty(RH_TET_DwarfDefOf.RH_TET_SkavenDefendTunnelAggressively, (LocalTargetInfo)((Thing)hiveFor), this.distToTunnelToAttack);
                this.lord.ownedPawns[index].mindState.duty = pawnDuty;
            }
        }
    }
}