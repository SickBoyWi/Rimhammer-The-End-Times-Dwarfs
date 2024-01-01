using RimWorld;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class Command_AbilityHoldCourt : Command_Ability
    {
        public override string Tooltip
        {
            get
            {
                TaggedString taggedString = this.ability.def.LabelCap + "\n\n" + "RH_TET_Dwarfs_AbilityHoldCourtTooltip".Translate(this.ability.pawn.Named("ORGANIZER")) + "\n";
                if (this.ability.CooldownTicksRemaining > 0)
                    taggedString += "\n" + "RH_TET_Dwarfs_AbilityHoldCourtCoolDown".Translate() + ": " + this.ability.CooldownTicksRemaining.ToStringTicksToPeriod(true, false, true, true);
                return (string)(taggedString + ("\n" + GatheringWorker_HighBloodCourt.OutcomeBreakdownForPawn(this.ability.pawn)));
            }
        }

        public Command_AbilityHoldCourt(Ability ability)
          : base(ability)
        {
        }
    }
}
