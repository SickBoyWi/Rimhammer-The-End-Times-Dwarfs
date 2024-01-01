using RimWorld;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class ThoughtWorker_HighBloodNoPersonalBedroom : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            CompDwarfHighBlood comp = p.TryGetComp<CompDwarfHighBlood>();

            if (comp == null || comp.highBloodComp == null || comp.highBloodComp.GetCurrentHighBlood() == null || p.MapHeld == null || (!p.MapHeld.IsPlayerHome || comp.highBloodComp.HighestHighBloodWithBedroomRequirements() == null))
                return (ThoughtState)false;
            return (ThoughtState)(!comp.highBloodComp.HasPersonalBedroom());
        }

        public override string PostProcessDescription(Pawn p, string description)
        {
            CompDwarfHighBlood comp = p.TryGetComp<CompDwarfHighBlood>();

            if (comp.highBloodComp.HighestHighBloodWithBedroomRequirements() != null)
                return (string)description.Formatted(comp.highBloodComp.HighestHighBloodWithBedroomRequirements().def.GetLabelCapFor(p).Named("HIGHBLOOD"));
            else
                return "";
        }
    }
}