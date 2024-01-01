using RimWorld;
using Verse;
using Verse.AI.Group;

namespace TheEndTimes_Dwarfs
{
    public class LordToilData_HoldCourt : LordToilData_Gathering
    {
        public SpectateRectSide spectateRectAllowedSides = SpectateRectSide.All;
        public CellRect spectateRect;
        public SpectateRectSide spectateRectPreferredSide;

        public override void ExposeData()
        {
            Scribe_Values.Look<CellRect>(ref this.spectateRect, "spectateRect", new CellRect(), false);
            Scribe_Values.Look<SpectateRectSide>(ref this.spectateRectAllowedSides, "spectateRectAllowedSides", SpectateRectSide.None, false);
            Scribe_Values.Look<SpectateRectSide>(ref this.spectateRectPreferredSide, "spectateRectPreferredSide", SpectateRectSide.None, false);
        }
    }
}
