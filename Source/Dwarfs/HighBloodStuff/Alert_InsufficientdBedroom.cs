﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class Alert_InsufficientHighBloodBedroom : Alert
    {
        private List<Pawn> targetsResult = new List<Pawn>();

        public Alert_InsufficientHighBloodBedroom()
        {
            this.defaultLabel = (string)"RH_TET_Dwarfs_InappropriateBedroom".Translate();
            this.defaultExplanation = (string)"RH_TET_Dwarfs_InappropriateBedroomDesc".Translate();
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

                                if (comp.highBloodComp != null && comp.highBloodComp.GetUnmetBedroomRequirements(true, false).Any<string>())
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

        public override TaggedString GetExplanation()
        {
            return (TaggedString)(this.defaultExplanation + "\n" + this.Targets.Select<Pawn, string>((Func<Pawn, string>)(t =>
            {
                CompDwarfHighBlood comp = t.TryGetComp<CompDwarfHighBlood>();

                HighBlood highBlood = comp.highBloodComp.HighestHighBloodWithBedroomRequirements();

                StringBuilder stringBuilder = new StringBuilder();

                if (highBlood != null)
                { 
                    HighBloodDef highBloodDef = highBlood.def;
                    string[] array1 = comp.highBloodComp.GetUnmetBedroomRequirements(false, false).ToArray<string>();
                    string[] array2 = comp.highBloodComp.GetUnmetBedroomRequirements(true, true).ToArray<string>();
                    bool flag = highBloodDef != null && (uint)array1.Length > 0;
                    if (flag)
                        stringBuilder.Append(t.LabelShort + " (" + highBloodDef.GetLabelFor(t.gender) + "):\n" + ((IList<string>)array1).ToLineList("- "));
                    if (array2.Length != 0)
                    {
                        if (flag)
                            stringBuilder.Append("\n\n");
                        stringBuilder.Append((string)(t.LabelShort + " (" + highBlood.def.GetLabelFor(t.gender) + ", " + "RH_TET_Dwarfs_RoomRequirementGracePeriodDesc".Translate((NamedArgument)highBlood.RoomRequirementGracePeriodDaysLeft.ToString("0.0")) + ")" + ":\n" + ((IList<string>)array2).ToLineList("- ")));
                    }
                }
                return stringBuilder.ToString();
            })).ToLineList("\n", false));
        }
    }
}