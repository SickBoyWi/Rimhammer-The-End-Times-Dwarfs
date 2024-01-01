using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TheEndTimes_Dwarfs
{
    public static class WatchThingUtility
    {
        private static int MIN_WATCH_DISTANCE = 1;
        private static int MAX_WATCH_DISTANCE = 1;
        private static int WATCH_RECTANGLE_WIDTH = (MAX_WATCH_DISTANCE * 2) + 1;

        private static List<int> allowedDirections = new List<int>();

        [DebuggerHidden]
        public static IEnumerable<IntVec3> CalculateWatchCells(ThingDef def, IntVec3 center, Map map)
        {
            List<int> allowedDirections = WatchThingUtility.CalculateAllowedDirections(def);
            for (int i = 0; i < allowedDirections.Count; i++)
            {
                foreach (IntVec3 c in WatchThingUtility.GetWatchCellRect(center, allowedDirections[i]))
                {
                    if (WatchThingUtility.EverPossibleToWatchFrom(c, center, map, true))
                    {
                        yield return c;
                    }
                }
            }
        }

        public static bool TryFindBestWatchCell(
            Thing toWatch,
            Pawn pawn,
            bool desireSit,
            out IntVec3 result,
            out Building chair)
        {
            List<int> allowedDirections = WatchThingUtility.CalculateAllowedDirections(toWatch.def);
            IntVec3 c = IntVec3.Invalid;
            for (int index1 = 0; index1 < allowedDirections.Count; ++index1)
            {
                CellRect watchCellRect = WatchThingUtility.GetWatchCellRect(toWatch.Position, allowedDirections[index1]);
                IntVec3 centerCell = watchCellRect.CenterCell;
                int num = watchCellRect.Area * 4;
                for (int index2 = 0; index2 < num; ++index2)
                {
                    IntVec3 intVec3 = centerCell + GenRadial.RadialPattern[index2];
                    if (watchCellRect.Contains(intVec3))
                    {
                        bool flag = false;
                        Building building = (Building)null;
                        if (WatchThingUtility.EverPossibleToWatchFrom(intVec3, toWatch.Position, toWatch.Map, false) && !intVec3.IsForbidden(pawn) && (pawn.CanReserve((LocalTargetInfo)intVec3, 1, -1, (ReservationLayerDef)null, false) && pawn.Map.pawnDestinationReservationManager.CanReserve(intVec3, pawn, false)))
                        {
                            if (desireSit)
                            {
                                building = intVec3.GetEdifice(pawn.Map);
                                
                                if (building != null && building.def.building.isSittable && pawn.CanReserve((LocalTargetInfo)((Thing)building), 1, -1, (ReservationLayerDef)null, false))
                                    flag = true;
                            }
                            else
                                flag = true;
                        }
                        if (flag)
                        {
                            if (desireSit && building.Rotation != new Rot4(allowedDirections[index1]).Opposite)
                            {
                                c = intVec3;
                            }
                            else
                            {
                                result = intVec3;
                                chair = building;
                                return true;
                            }
                        }
                    }
                }
            }
            
            if (c.IsValid)
            {
                result = c;
                chair = c.GetEdifice(pawn.Map);
                return true;
            }
            
            result = IntVec3.Invalid;
            chair = (Building)null;
            return false;
        }

        public static bool CanWatchFromBed(Pawn pawn, Building_Bed bed, Thing toWatch)
        {
            if (!WatchThingUtility.EverPossibleToWatchFrom(pawn.Position, toWatch.Position, pawn.Map, true))
            {
                return false;
            }
            if (toWatch.def.rotatable)
            {
                Rot4 rotation = bed.Rotation;
                CellRect cellRect = toWatch.OccupiedRect();
                if (rotation == Rot4.North && cellRect.maxZ < pawn.Position.z)
                {
                    return false;
                }
                if (rotation == Rot4.South && cellRect.minZ > pawn.Position.z)
                {
                    return false;
                }
                if (rotation == Rot4.East && cellRect.maxX < pawn.Position.x)
                {
                    return false;
                }
                if (rotation == Rot4.West && cellRect.minX > pawn.Position.x)
                {
                    return false;
                }
            }
            List<int> list = WatchThingUtility.CalculateAllowedDirections(toWatch.def);
            for (int i = 0; i < list.Count; i++)
            {
                if (WatchThingUtility.GetWatchCellRect(toWatch.Position, list[i]).Contains(pawn.Position))
                {
                    return true;
                }
            }
            return false;
        }

        private static CellRect GetWatchCellRect(IntVec3 center, int watchRot)
        {
            Rot4 a = new Rot4(watchRot);
            CellRect result;
            if (a.IsHorizontal)
            {
                int num = center.x + GenAdj.CardinalDirections[watchRot].x * MIN_WATCH_DISTANCE;
                int num2 = center.x + GenAdj.CardinalDirections[watchRot].x * MAX_WATCH_DISTANCE;
                int num3 = center.z + WATCH_RECTANGLE_WIDTH / 2;
                int num4 = center.z - WATCH_RECTANGLE_WIDTH / 2;
                if (WATCH_RECTANGLE_WIDTH % 2 == 0)
                {
                    if (a == Rot4.West)
                    {
                        num4++;
                    }
                    else
                    {
                        num3--;
                    }
                }
                result = new CellRect(Mathf.Min(num, num2), num4, Mathf.Abs(num - num2) + 1, num3 - num4 + 1);
            }
            else
            {
                int num5 = center.z + GenAdj.CardinalDirections[watchRot].z * MIN_WATCH_DISTANCE;
                int num6 = center.z + GenAdj.CardinalDirections[watchRot].z * MAX_WATCH_DISTANCE;
                int num7 = center.x + WATCH_RECTANGLE_WIDTH / 2;
                int num8 = center.x - WATCH_RECTANGLE_WIDTH / 2;
                if (WATCH_RECTANGLE_WIDTH % 2 == 0)
                {
                    if (a == Rot4.North)
                    {
                        num8++;
                    }
                    else
                    {
                        num7--;
                    }
                }
                result = new CellRect(num8, Mathf.Min(num5, num6), num7 - num8 + 1, Mathf.Abs(num5 - num6) + 1);
            }
            return result;
        }

        private static bool EverPossibleToWatchFrom(IntVec3 watchCell, IntVec3 thingCenter, Map map, bool bedAllowed)
        {
            return (watchCell.Standable(map) || (bedAllowed && watchCell.GetEdifice(map) is Building_Bed)) && GenSight.LineOfSight(thingCenter, watchCell, map, true, null, 0, 0);
        }

        private static List<int> CalculateAllowedDirections(ThingDef toWatchDef)
        {
            WatchThingUtility.allowedDirections.Clear();

            WatchThingUtility.allowedDirections.Add(0);
            WatchThingUtility.allowedDirections.Add(1);
            WatchThingUtility.allowedDirections.Add(2);
            WatchThingUtility.allowedDirections.Add(3);

            return WatchThingUtility.allowedDirections;
        }
    }
}
