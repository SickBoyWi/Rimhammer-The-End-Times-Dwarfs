using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class Alert_HighBloodNoAcceptableFood : Alert
    {
        private List<Pawn> targetsResult = new List<Pawn>();

        public Alert_HighBloodNoAcceptableFood()
        {
            this.defaultLabel = (string)"RH_TET_Dwarfs_HighBloodNoAcceptableFood".Translate();
            this.defaultExplanation = (string)"RH_TET_Dwarfs_HighBloodNoAcceptableFoodDesc".Translate();
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
                            if (pawnsToCheck.Contains(freeColonist) && freeColonist.Spawned && (freeColonist.story == null || !freeColonist.story.traits.HasTrait(TraitDefOf.Ascetic)))
                            {
                                CompDwarfHighBlood comp = freeColonist.TryGetComp<CompDwarfHighBlood>();

                                if (comp == null)
                                    Log.Error("Dwarf has null high blood comp.");

                                HighBlood highBlood = comp.highBloodComp.HighBloodInEffectForReading.First();
                                Thing foodSource;
                                ThingDef foodDef;
                                if (highBlood != null && highBlood.highMaintenance && (highBlood.def.foodRequirement.Defined && !FoodUtility.TryFindBestFoodSourceFor(freeColonist, freeColonist, false, out foodSource, out foodDef, true, true, false, false, false, false, false, true, true, false, FoodPreferability.DesperateOnly)))
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

                HighBlood highBlood = comp.highBloodComp.HighBloodInEffectForReading.First();
                return t.LabelShort + " (" + highBlood.def.GetLabelFor(t.gender) + "):\n" + highBlood.def.SatisfyingMeals(false).Select<ThingDef, string>((Func<ThingDef, string>)(m => (string)m.LabelCap)).ToLineList("- ", false);
            })).ToLineList("\n", false));
        }
    }
}
