using RimWorld;
using Verse;
using Verse.AI;

namespace TheEndTimes_Dwarfs
{
    public class JobGiver_TunnelDefense : JobGiver_AIFightEnemies
    {
        protected override IntVec3 GetFlagPosition(Pawn pawn)
        {
            SkavenTunnel thing = pawn.mindState.duty.focus.Thing as SkavenTunnel;
            if (thing != null && thing.Spawned)
                return thing.Position;
            return pawn.Position;
        }

        protected override float GetFlagRadius(Pawn pawn)
        {
            return pawn.mindState.duty.radius;
        }

        protected override Job MeleeAttackJob(Thing enemyTarget)
        {
            Job job = base.MeleeAttackJob(enemyTarget);
            job.attackDoorIfTargetLost = true;
            return job;
        }
    }
}
