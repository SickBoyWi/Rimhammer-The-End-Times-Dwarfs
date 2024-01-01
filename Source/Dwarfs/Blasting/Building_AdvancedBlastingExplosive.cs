

using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class Building_AdvancedBlastingExplosive : Building_BlastingCharge
    {
        private List<IntVec3> affectedCells;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            CompBlastingExplosive comp = this.GetComp<CompBlastingExplosive>();
            if (comp == null)
                return;
            this.affectedCells = this.GetAffectedCellsAtPosition(this.Position, comp.MiningProps.miningRadius);
            comp.AssignCustomMiningArea(this.affectedCells);
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            if (this.affectedCells == null)
                return;
            GenDraw.DrawFieldEdges(this.affectedCells);
        }

        internal virtual List<IntVec3> GetAffectedCellsAtPosition(
          IntVec3 position,
          float radius)
        {
            return GenRadial.RadialCellsAround(position, radius, true).ToList<IntVec3>();
        }
    }
}
