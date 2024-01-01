using RimWorld;
using System.Collections.Generic;
using TheEndTimes_Magic;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class WorkGiver_DoBillDwarfs : RimWorld.WorkGiver_DoBill
    {
        public override Verse.AI.Job JobOnThing(Pawn pawn, Verse.Thing thing, bool forced = false)
        {
            Verse.AI.Job job = base.JobOnThing(pawn, thing, forced);
            if (job != null && job.def == JobDefOf.DoBill)
            {
                var worker = job.RecipeDef.Worker as RecipeWorkerWithJob;
                if (worker != null)
                    return new Verse.AI.Job(worker.Job, job.targetA)
                    {
                        targetQueueB = job.targetQueueB,
                        countQueue = job.countQueue,
                        haulMode = job.haulMode,
                        bill = job.bill
                    };
            }
            return job;
        }
    }
}
