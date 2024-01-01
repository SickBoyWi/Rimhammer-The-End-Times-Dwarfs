using RimWorld;
using Verse;
using Verse.AI;

namespace TheEndTimes_Dwarfs
{
    public class JoyGiver_PlayDice : JoyGiver_InteractBuilding
    {
        protected override bool CanDoDuringGathering
        {
            get
            {
                return true;
            }
        }

        protected override Job TryGivePlayJob(Pawn pawn, Thing t)
        {
            return JobMaker.MakeJob(this.def.jobDef, (LocalTargetInfo)t);
        }
    }
}
