using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class PlaceWorker_MountainBreakerExplosive : PlaceWorker
    {
        private static readonly Color IneffectivePlacementColor = Color.yellow;
        private static readonly Color EffectivePlacementColor = Color.green;
        private static readonly List<IntVec3> effectiveRadiusCells = new List<IntVec3>();
        private static readonly List<IntVec3> overheadMountainCells = new List<IntVec3>();
        private const float AdditionalRoofDisplayRadius = 3f;

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            float explosiveRadius = BlastingUtility.TryGetExplosiveRadius(def);
            if ((double)explosiveRadius <= 0.0)
                return;
            Map currentMap = Find.CurrentMap;
            if (currentMap == null)
                return;
            RoofGrid roofGrid = currentMap.roofGrid;
            PlaceWorker_MountainBreakerExplosive.effectiveRadiusCells.Clear();
            PlaceWorker_MountainBreakerExplosive.overheadMountainCells.Clear();
            int num1 = GenRadial.NumCellsInRadius(explosiveRadius);
            int num2 = GenRadial.NumCellsInRadius(explosiveRadius + 3f);
            for (int index = 0; index < num2; ++index)
            {
                IntVec3 c = center + GenRadial.RadialPattern[index];
                if (c.InBounds(currentMap))
                {
                    RoofDef roofDef = roofGrid.RoofAt(c);
                    if (roofDef != null && roofDef.isThickRoof)
                        PlaceWorker_MountainBreakerExplosive.overheadMountainCells.Add(c);
                    if (index < num1)
                        PlaceWorker_MountainBreakerExplosive.effectiveRadiusCells.Add(c);
                }
            }
            if (PlaceWorker_MountainBreakerExplosive.overheadMountainCells.Count > 0 && Find.Selector.NumSelected <= 1)
                GenDraw.DrawFieldEdges(PlaceWorker_MountainBreakerExplosive.overheadMountainCells, Color.white);
            Color color = BlastingUtility.IsEffectiveRoofBreakerPlacement(explosiveRadius, center, currentMap) ? PlaceWorker_MountainBreakerExplosive.EffectivePlacementColor : PlaceWorker_MountainBreakerExplosive.IneffectivePlacementColor;
            GenDraw.DrawFieldEdges(PlaceWorker_MountainBreakerExplosive.effectiveRadiusCells, color);
        }
    }
}
