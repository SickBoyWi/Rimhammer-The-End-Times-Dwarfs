using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class PlaceWorker_OnWalls : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef,
                                                            IntVec3 loc,
                                                               Rot4 rot,
                                                                Map map,
                                                              Thing thingToIgnore = null,
                                                              Thing thing = null)
        {

            Building building = loc.GetEdifice(map);

            if (building == null || building.def == null || building.def.graphicData == null)
                return (AcceptanceReport)"RH_TET_Dwarfs_WallNeeded".Translate();
            else if ((building.def.graphicData.linkFlags & (LinkFlags.Wall | LinkFlags.Rock)) == 0)
                return (AcceptanceReport)"RH_TET_Dwarfs_WallNeeded".Translate();

            if (rot.FacingCell != null)
            {

                IntVec3 facingLoc = loc;

                switch (rot.AsInt)
                {
                    case 0: // south
                        --facingLoc.z;
                        break;
                    case 1: // west
                        --facingLoc.x;
                        break;
                    case 2: // north
                        ++facingLoc.z;
                        break;
                    case 3: // east
                        ++facingLoc.x;
                        break;
                    default:
                        throw new System.Exception($"RH_TET: PlaceWorker_OnWalls " +
                            "found an invalid rotation for placed thing at {loc}.");
                }

                Building facingLocBuilding = facingLoc.GetEdifice(map);
                if (facingLocBuilding != null && facingLocBuilding.def != null && facingLocBuilding.def.graphicData != null && DwarfsUtil.IsWallRockDoorOrSolid(facingLocBuilding))
                    return (AcceptanceReport)"RH_TET_Dwarfs_OpenSpaceNeeded".Translate();
            }

            return AcceptanceReport.WasAccepted;
        }

        public override void DrawGhost(
          ThingDef def,
          IntVec3 center,
          Rot4 rot,
          Color ghostCol,
          Thing _thing = null)
        {
            base.DrawGhost(def, center, rot, ghostCol, (Thing)null);
            bool flag = false;
            Map currentMap = Find.CurrentMap;
            if (!GenGrid.InBounds(center + IntVec3Utility.RotatedBy(IntVec3.South, rot), currentMap))
                flag = true;
            Room roomGroup = GridsUtility.GetRoom(center + IntVec3Utility.RotatedBy(new IntVec3(0, 0, 1), rot), currentMap);
            if (roomGroup != null)
            {
                if (roomGroup != StaticWallLightStuff.lastRoom)
                {
                    StaticWallLightStuff.lastRoom = roomGroup;
                    StaticWallLightStuff.preLitCells.Clear();
                    using (List<IntVec3>.Enumerator enumerator1 = new List<IntVec3>(roomGroup.Cells).GetEnumerator())
                    {
                        while (enumerator1.MoveNext())
                        {
                            IntVec3 current = enumerator1.Current;
                            List<Thing> thingList = new List<Thing>();
                            using (List<Thing>.Enumerator enumerator2 = GridsUtility.GetThingList(current, currentMap).GetEnumerator())
                            {
                                while (enumerator2.MoveNext())
                                {
                                    if (((string)((Def)enumerator2.Current.def).defName).Contains("WallLightGlower"))
                                    {
                                        StaticWallLightStuff.preLitCells.Add(current);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
                flag = true;
            if (!flag)
            {
                StaticWallLightStuff.ClearCells();
                using (List<IntVec3>.Enumerator enumerator = StaticWallLightStuff.preLitCells.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                        StaticWallLightStuff.GetLightCells(enumerator.Current, currentMap);
                }
                StaticWallLightStuff.GetLightCells(center + IntVec3Utility.RotatedBy(IntVec3.South, rot), currentMap);
                if (StaticWallLightStuff.totalCells.Count > 0)
                    GenDraw.DrawFieldEdges(StaticWallLightStuff.totalCells);
            }
        }

        public PlaceWorker_OnWalls()
            : base()
        {
        }
    }
}
