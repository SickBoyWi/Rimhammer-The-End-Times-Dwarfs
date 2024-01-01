using RimWorld;
using System.Collections.Generic;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class Alert_HighBloodNoThroneAssigned : Alert
    {
        private List<Pawn> targetsResult = new List<Pawn>();

        public Alert_HighBloodNoThroneAssigned()
        {
            this.defaultLabel = (string)"RH_TET_Dwarfs_NeedsHighBloodSeatLabel".Translate();
            this.defaultExplanation = (string)"RH_TET_Dwarfs_NeedsHighBloodSeat".Translate();
        }

        private List<Pawn> Targets
        {
            get
            {
                this.targetsResult.Clear();

                if (RH_TET_DwarfsMod.king != null || (RH_TET_DwarfsMod.thanes != null && RH_TET_DwarfsMod.thanes.Count > 0))
                {
                    List<Pawn> pawnsToCheck = new List<Pawn>();
                    if (RH_TET_DwarfsMod.king != null)
                        pawnsToCheck.Add(RH_TET_DwarfsMod.king);
                    if (RH_TET_DwarfsMod.thanes != null && RH_TET_DwarfsMod.thanes.Count > 0)
                        pawnsToCheck.AddRange(RH_TET_DwarfsMod.thanes);
                    
                    List<Map> maps = Find.Maps;
                    for (int index1 = 0; index1 < maps.Count; ++index1)
                    {
                        foreach (Pawn freeColonist in maps[index1].mapPawns.FreeColonists)
                        {
                            if (pawnsToCheck.Contains(freeColonist))
                            {
                                bool flag = false;
                                
                                CompDwarfHighBlood comp = freeColonist.TryGetComp<CompDwarfHighBlood>();

                                if (comp == null)
                                    Log.Error("Dwarf has null high blood comp.");
                                
                                List<HighBlood> highBloods = comp.highBloodComp.HighBloodInEffectForReading;
                                for (int index2 = 0; index2 < highBloods.Count; ++index2)
                                {
                                    if (!highBloods[index2].def.highBloodRoomRequirements.NullOrEmpty<RoomRequirement>())
                                    {
                                        flag = true;
                                        break;
                                    }
                                }
                                if (flag && (RH_TET_DwarfsMod.assignedThrones.TryGetValue(freeColonist) == null || RH_TET_DwarfsMod.assignedThrones.TryGetValue(freeColonist).Count <= 0))
                                    this.targetsResult.Add(freeColonist);
                            }
                        }
                    }
                }
                return this.targetsResult;
            }
        }

        public override AlertReport GetReport()
        {
            return AlertReport.CulpritsAre(this.Targets);
        }
    }
}
