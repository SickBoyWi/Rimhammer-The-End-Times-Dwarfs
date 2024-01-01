using RimWorld;
using System.Collections.Generic;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class Alert_HighBloodRequiresBedroom : Alert
    {
        private List<Pawn> targetsResult = new List<Pawn>();

        public Alert_HighBloodRequiresBedroom()
        {
            this.defaultLabel = (string)"RH_TET_Dwarfs_HighBloodNeedBedroomAssigned".Translate();
            this.defaultExplanation = (string)"RH_TET_Dwarfs_HighBloodNeedBedroomAssignedDesc".Translate();
        }

        public List<Pawn> Targets
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
                    for (int index = 0; index < maps.Count; ++index)
                    {
                        foreach (Pawn freeColonist in maps[index].mapPawns.FreeColonists)
                        {
                            if (pawnsToCheck.Contains(freeColonist))
                            {
                                CompDwarfHighBlood comp = freeColonist.TryGetComp<CompDwarfHighBlood>();
                                
                                if (comp == null)
                                    Log.Error("Dwarf has null high blood comp.");

                                if (comp.highBloodComp != null && comp.highBloodComp.CanRequireBedroom() && (comp.highBloodComp.HighestHighBloodWithBedroomRequirements() != null && !comp.highBloodComp.HasPersonalBedroom()))
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
