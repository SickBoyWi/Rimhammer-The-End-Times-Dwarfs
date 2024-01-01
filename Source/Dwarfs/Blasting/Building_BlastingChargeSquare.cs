using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class Building_BlastingChargeSquare : Building_AdvancedBlastingExplosive
    {
        public static List<IntVec3> GetAffectedCellsSquareAtPosition(
          IntVec3 position,
          float radius)
        {
            int num = (int)Mathf.Clamp(Mathf.Round(radius), 0.0f, 25f);
            List<IntVec3> intVec3List1 = new List<IntVec3>();
            List<IntVec3> intVec3List2 = new List<IntVec3>()
              {
                new IntVec3(position.x - num, 0, position.z - num),
                new IntVec3(position.x + num, 0, position.z - num),
                new IntVec3(position.x - num, 0, position.z + num),
                new IntVec3(position.x + num, 0, position.z + num)
              };
            CellRect cellRect = new CellRect(position.x - num, position.z - num, num * 2 + 1, num * 2 + 1);
            if (num > 0)
            {
                foreach (IntVec3 intVec3 in cellRect)
                {
                    if (!intVec3List2.Contains(intVec3))
                        intVec3List1.Add(intVec3);
                }
            }
            else
                intVec3List1.Add(position);
            return intVec3List1;
        }

        internal override List<IntVec3> GetAffectedCellsAtPosition(
          IntVec3 position,
          float radius)
        {
            return Building_BlastingChargeSquare.GetAffectedCellsSquareAtPosition(position, radius);
        }
    }
}
