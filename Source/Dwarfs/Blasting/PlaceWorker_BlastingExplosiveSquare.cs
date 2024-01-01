using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class PlaceWorker_BlastingExplosiveSquare : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            CompProperties_BlastingExplosive propertiesMiningExplosive = (CompProperties_BlastingExplosive)def.comps.FirstOrDefault<CompProperties>((Func<CompProperties, bool>)(c => c is CompProperties_BlastingExplosive));
            if (propertiesMiningExplosive == null)
                return;
            GenDraw.DrawFieldEdges(Building_BlastingChargeSquare.GetAffectedCellsSquareAtPosition(center, propertiesMiningExplosive.miningRadius));
            base.DrawGhost(def, center, rot, ghostCol);
        }
    }
}
