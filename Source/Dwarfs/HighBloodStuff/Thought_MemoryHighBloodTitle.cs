using RimWorld;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class Thought_MemoryHighBlood : Thought_Memory
    {
        public HighBloodDef highBlood;

        public override string LabelCap
        {
            get
            {
                return (string)this.CurStage.label.Formatted(this.highBlood.GetLabelCapFor(this.pawn).Named("HIGHBLOOD"));
            }
        }

        public override string Description
        {
            get
            {
                return (string)this.CurStage.description.Formatted(this.highBlood.GetLabelCapFor(this.pawn).Named("HIGHBLOOD"), this.pawn.Named("PAWN"));
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look<HighBloodDef>(ref this.highBlood, "highBlood");
        }
    }
}
