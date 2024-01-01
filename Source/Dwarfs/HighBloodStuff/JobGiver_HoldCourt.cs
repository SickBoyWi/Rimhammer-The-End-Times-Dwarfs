using Verse;
using Verse.AI;

namespace TheEndTimes_Dwarfs
{
    public class JobGiver_HoldCourt : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            PawnDuty duty = pawn.mindState.duty;
            if (duty == null)
                return (Job)null;
            Building_DwarfThrone thing = duty.focusSecond.Thing as Building_DwarfThrone;
            if (thing == null || thing.AssignedPawn != pawn)
                return (Job)null;
            if (!pawn.CanReach((LocalTargetInfo)((Thing)thing), PathEndMode.InteractionCell, Danger.None, false, false, TraverseMode.ByPawn))
                return (Job)null;
            return JobMaker.MakeJob(RH_TET_DwarfDefOf.RH_TET_Dwarfs_HoldCourt, duty.focusSecond);
        }
    }
}
