using RimWorld;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class ThoughtWorker_MissingBeardSad : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!p.RaceProps.Humanlike || p.RaceProps.IsMechanoid || p.gender != Gender.Male || !DwarfsUtil.IsDwarf(p))
                return ThoughtState.Inactive;

            if (p.health.hediffSet.PartIsMissing(p.RaceProps.body.AllParts.FirstOrFallback(part => part.def == RH_TET_DwarfDefOf.RH_TET_Dwarfs_BP_JawBeard)))
            {
                return ThoughtState.ActiveAtStage(1);
            }
            if (p.health.hediffSet.GetInjuredParts().FirstOrFallback(part => part.def == RH_TET_DwarfDefOf.RH_TET_Dwarfs_BP_JawBeard) != null)
            {
                return ThoughtState.ActiveAtStage(0);
            }

            return ThoughtState.Inactive;
        }
    }
}