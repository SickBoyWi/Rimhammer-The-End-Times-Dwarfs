using Verse;
using Verse.AI;

namespace TheEndTimes_Dwarfs
{
    public class JobGiver_WanderTunnel : JobGiver_Wander
    {
        public JobGiver_WanderTunnel()
        {
            this.wanderRadius = 7.5f;
            this.ticksBetweenWandersRange = new IntRange(125, 200);
        }

        protected override IntVec3 GetWanderRoot(Pawn pawn)
        {
            SkavenTunnel thing = pawn.mindState.duty.focus.Thing as SkavenTunnel;
            if (thing == null || !thing.Spawned)
                return pawn.Position;
            return thing.Position;
        }
    }
}

