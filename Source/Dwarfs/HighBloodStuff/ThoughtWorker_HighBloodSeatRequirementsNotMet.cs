using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class ThoughtWorker_HighBloodSeatRequirementsNotMet : ThoughtWorker
    {
        protected bool Active(Pawn p)
        {
            CompDwarfHighBlood comp = p.TryGetComp<CompDwarfHighBlood>();
            if (comp == null || comp.highBloodComp == null || comp.highBloodComp.GetCurrentHighBlood() == null || p.MapHeld == null || !p.MapHeld.IsPlayerHome)
                return false;
            return this.UnmetRequirements(p).Any<string>();
        }

        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!this.Active(p))
                return (ThoughtState)false;
            return (ThoughtState)true;
        }

        protected IEnumerable<string> UnmetRequirements(Pawn p)
        {
            CompDwarfHighBlood comp = p.TryGetComp<CompDwarfHighBlood>();
            return comp.highBloodComp.GetUnmetThroneroomRequirements(p, false, false);
        }

        public override string PostProcessDescription(Pawn p, string description)
        {
            CompDwarfHighBlood comp = p.TryGetComp<CompDwarfHighBlood>();
            return (string)description.Formatted((NamedArgument)this.UnmetRequirements(p).ToLineList("- ", false), comp.highBloodComp.HighestHighBloodWithThroneRoomRequirements().def.GetLabelCapFor(p).Named("HIGHBLOOD"));
        }
    }
}
