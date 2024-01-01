using Verse;
using Verse.AI;

namespace TheEndTimes_Dwarfs
{
    public class ThinkNode_ConditionalTunnelCanReproduce : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            SkavenTunnel thing = pawn.mindState.duty.focus.Thing as SkavenTunnel;
            if (thing != null)
            { 
                bool retVal = thing.GetComp<CompSpawnerTunnels>().canSpawnTunnels;
                return retVal;
            }
            return false;
        }
    }
}
