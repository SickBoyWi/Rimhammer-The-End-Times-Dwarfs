using Verse;
using Verse.AI;

namespace TheEndTimes_Dwarfs
{
    public class JobGiver_Rule : ThinkNode_JobGiver
    {
        public override float GetPriority(Pawn pawn)
        {
            CompDwarfHighBlood comp = pawn.TryGetComp<CompDwarfHighBlood>();

            if (comp == null || comp.highBloodComp == null || comp.highBloodComp.GetCurrentHighBlood() == null)
            {
                return 0.0f;
            }
            else
            {
                bool randDo = Rand.MTBEventOccurs(DwarfPawn_HighBloodTracker.RulingMTB, 60000f, DwarfPawn_HighBloodTracker.RulingCheckInterval);
                bool timeDo = (comp.highBloodComp.lastRuledTicks + (DwarfPawn_HighBloodTracker.RulingMTB * 60000f)) < Find.TickManager.TicksGame;
                if (randDo && timeDo)
                {
                    return 6f;
                }
            }
            return 0.0f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            Building_DwarfThrone bestUsableThrone = DwarfsUtil.FindBestUsableThrone(pawn);

            if (bestUsableThrone == null || !bestUsableThrone.Spawned)
                return (Job)null;
            if (!pawn.CanReach((LocalTargetInfo)((Thing)bestUsableThrone), PathEndMode.InteractionCell, pawn.NormalMaxDanger(), false, false, TraverseMode.ByPawn))
                return (Job)null;
            // This is a rare occurrence when someone else is sitting on the throne doing something else (admiring a horn or art for instance).
            if (!pawn.CanReserveAndReach(bestUsableThrone, PathEndMode.InteractionCell, Danger.Some, 1, -1, null, true))
                return (Job)null;

            return JobMaker.MakeJob(RH_TET_DwarfDefOf.RH_TET_Dwarfs_Rule, (LocalTargetInfo)((Thing)bestUsableThrone));
        }
    }
}