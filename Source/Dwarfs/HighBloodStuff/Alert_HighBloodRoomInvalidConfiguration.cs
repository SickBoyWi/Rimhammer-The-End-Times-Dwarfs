using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class Alert_HighBloodRoomInvalidConfiguration : Alert
    {
        private static string validationInfo;

        public Alert_HighBloodRoomInvalidConfiguration()
        {
            this.defaultLabel = (string)"RH_TET_Dwarfs_HighBloodSeatInvalidConfiguration".Translate();
            this.defaultExplanation = (string)"RH_TET_Dwarfs_HighBloodSeatInvalidConfigurationDesc".Translate();
        }

        public override TaggedString GetExplanation()
        {
            return base.GetExplanation() + "\n\n" + Alert_HighBloodRoomInvalidConfiguration.validationInfo;
        }

        public override AlertReport GetReport()
        {
            List<Map> maps = Find.Maps;
            for (int index = 0; index < maps.Count; ++index)
            {
                List<Thing> thrones = new List<Thing>();
                thrones.AddRange(maps[index].listerThings.ThingsOfDef(RH_TET_DwarfDefOf.RH_TET_Dwarfs_Throne));
                thrones.AddRange(maps[index].listerThings.ThingsOfDef(RH_TET_DwarfDefOf.RH_TET_Dwarfs_KingsThrone));
                foreach (Building_DwarfThrone thing in thrones)
                {
                    Alert_HighBloodRoomInvalidConfiguration.validationInfo = RoomRoleWorker_DwarfRulingRoom.Validate(thing.GetRoom(RegionType.Set_Passable));
                    if (Alert_HighBloodRoomInvalidConfiguration.validationInfo != null)
                        return AlertReport.CulpritIs((GlobalTargetInfo)((Thing)thing));
                }
            }
            return (AlertReport)false;
        }
    }
}
