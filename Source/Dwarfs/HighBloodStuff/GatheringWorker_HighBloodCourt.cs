using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace TheEndTimes_Dwarfs
{
    public class GatheringWorker_HighBloodCourt : GatheringWorker
    {
        protected override LordJob CreateLordJob(IntVec3 spot, Pawn organizer)
        {
            return (LordJob)new LordJob_Joinable_Court(spot, organizer, this.def);
        }

        public override bool CanExecute(Map map, Pawn organizer = null)
        {
            IntVec3 spot;
            return organizer != null && this.TryFindGatherSpot(organizer, out spot);
        }

        protected override bool TryFindGatherSpot(Pawn organizer, out IntVec3 spot)
        {
            Building_DwarfThrone bestUsableThrone = DwarfsUtil.FindBestUsableThrone(organizer);
            if (bestUsableThrone != null)
            {


                spot = bestUsableThrone.InteractionCell;
                return true;
            }
            spot = IntVec3.Invalid;
            return false;
        }

        protected override void SendLetter(IntVec3 spot, Pawn organizer)
        {
            Find.LetterStack.ReceiveLetter((TaggedString)this.def.letterTitle, this.def.letterText.Formatted(organizer.Named("ORGANIZER")) + "\n\n" + GatheringWorker_HighBloodCourt.OutcomeBreakdownForPawn(organizer), LetterDefOf.PositiveEvent, (LookTargets)new TargetInfo(spot, organizer.Map, false), (Faction)null, (Quest)null, (List<ThingDef>)null, (string)null);
        }

        public static string OutcomeBreakdownForPawn(Pawn organizer)
        {
            return (string)("RH_TET_Dwarfs_AbilityCourtStatInfo".Translate(organizer.Named("ORGANIZER"), (NamedArgument)StatDefOf.SocialImpact.label) + ": " + organizer.GetStatValue(StatDefOf.SocialImpact, true).ToStringPercent() + "\n\n" + "RH_TET_Dwarfs_AbilityCourtPossibleOutcomes".Translate() + ":\n" + LordJob_Joinable_Court.OutcomeChancesForPawn(organizer).Reverse<Tuple<ThoughtDef, float>>().Select<Tuple<ThoughtDef, float>, string>((Func<Tuple<ThoughtDef, float>, string>)(o => o.Item1.stages[0].LabelCap + " " + o.Item2.ToStringPercent())).ToLineList("  - ", false));
        }
    }
}
