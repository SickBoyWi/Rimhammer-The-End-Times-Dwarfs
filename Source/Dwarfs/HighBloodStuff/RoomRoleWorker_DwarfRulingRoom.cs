using RimWorld;
using System.Collections.Generic;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class RoomRoleWorker_DwarfRulingRoom : RoomRoleWorker
    {
        private static List<Building_DwarfThrone> tmpThrones = new List<Building_DwarfThrone>();
        
        public static string Validate(Room room)
        {
            if (room == null || room.OutdoorsForWork)
                return (string)"RH_TET_Dwarfs_HighBloodSeatInside".Translate();
            return (string)null;
        }

        public override float GetScore(Room room)
        {
            List<Thing> andAdjacentThings = room.ContainedAndAdjacentThings;
            bool flag = false;
            for (int index = 0; index < andAdjacentThings.Count; ++index)
            {
                if (andAdjacentThings[index] is Building_DwarfThrone)
                {
                    flag = true;
                    break;
                }
            }
            return !flag || RoomRoleWorker_DwarfRulingRoom.Validate(room) != null ? 0.0f : 10000f;
        }
    }
}
