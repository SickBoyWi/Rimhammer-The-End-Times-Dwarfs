using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Xml.Linq;
using TheEndTimes_Magic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace TheEndTimes_Dwarfs
{
    partial class HarmonyPatches_OtherStuff
    {
        public static PawnKindDef Slave;

        // TODO : (FIXED 20240101) THIS NEED REPLACEMENT ASAP. Save, then clean up if there's no issue shortly.
        [HarmonyPatch(typeof(ReverseDesignatorDatabase), "InitDesignators", null)]
        public static class ReverseDesignatorDatabase_InitDesignators
        {
            // TODO : (FIXED 20240101) THIS WORKS BUT IS A REALLY BAD IDEA.
            //public static bool Prefix(ref List<Designator> ___desList)
            //{
            //    // (FIXED 20240101) TODO THIS NEEDS A BETTER FIX. It will muck up new designators added to core code,
            //    // and perhaps designators added by other modders.
            //    ___desList = new List<Designator>();
            //    ___desList.Add((Designator)new Designator_Cancel());
            //    ___desList.Add((Designator)new Designator_Claim());
            //    ___desList.Add((Designator)new Designator_Deconstruct());
            //    ___desList.Add((Designator)new Designator_Uninstall());
            //    ___desList.Add((Designator)new Designator_Haul());
            //    ___desList.Add((Designator)new Designator_Hunt());
            //    ___desList.Add((Designator)new Designator_Slaughter());
            //    ___desList.Add((Designator)new Designator_Tame());
            //    ___desList.Add((Designator)new Designator_PlantsCut());
            //    ___desList.Add((Designator)new Designator_PlantsHarvest());
            //    ___desList.Add((Designator)new Designator_PlantsHarvestWood());
            //    ___desList.Add((Designator)new Designator_Mine());
            //    ___desList.Add((Designator)new Designator_Strip());
            //    ___desList.Add((Designator)new Designator_Open());
            //    ___desList.Add((Designator)new Designator_SmoothSurface());
            //    ___desList.Add((Designator)new Designator_ReleaseAnimalToWild());
            //    ___desList.Add((Designator)new Designator_ExtractTree());
            //    ___desList.Add((Designator)new Designator_Study());
            //    ___desList.Add((Designator)new Designator_RemovePaint());
            //    ___desList.Add((Designator)new Designator_SelectBlastingWire());
            //    ___desList.Add((Designator)new Designator_DecorateWall());
            //    if (ModsConfig.BiotechActive)
            //    {
            //        ___desList.Add((Designator)new Designator_MechControlGroup());
            //        ___desList.Add((Designator)new Designator_Adopt());
            //    }
            //    if (ModsConfig.IdeologyActive)
            //        ___desList.Add((Designator)new Designator_ExtractSkull());
            //    ___desList.RemoveAll((Predicate<Designator>)(des => !Current.Game.Rules.DesignatorAllowed(des)));

            //    return false;
            //}

            public static void Postfix(ref List<Designator> ___desList)
            {
                ___desList.Add((Designator)new Designator_SelectBlastingWire());
                ___desList.Add((Designator)new Designator_DecorateWall());
            }
        }

        [HarmonyPatch(typeof(Recipe_RemoveBodyPart), "ApplyOnPawn")]
        internal static class Patch_RemoveBodyPart
        {
            // Assist with beard removal thoughts.
            private static bool Prefix(ref Recipe_RemoveBodyPart __instance,
                    ref Pawn pawn,
                    ref BodyPartRecord part,
                    ref Pawn billDoer,
                    ref List<Thing> ingredients,
                    ref Bill bill)
            {
                if (part.def != RH_TET_DwarfDefOf.RH_TET_Dwarfs_BP_Beard)
                {
                    return true;
                }

                if (billDoer != null)
                {
                    MedicalRecipesUtility.SpawnNaturalPartIfClean(pawn, part, billDoer.Position, billDoer.Map);
                    MedicalRecipesUtility.SpawnThingsFromHediffs(pawn, part, billDoer.Position, billDoer.Map);
                }

                __instance.DamagePart(pawn, part);
                pawn.Drawer.renderer.SetAllGraphicsDirty();

                return false;
            }
        }

        [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn", (new[] { typeof(PawnGenerationRequest) }))]
        internal static class Patch_GeneratePawn
        {
            private static void Postfix(Pawn __result)
            {
                // Remove beard on female dwarfs.
                if (!__result.RaceProps.Humanlike || __result.RaceProps.IsMechanoid || __result.gender == Gender.Female || !DwarfsUtil.IsDwarf(__result))
                {
                    if (!__result.health.hediffSet.PartIsMissing(__result.RaceProps.body.AllParts.FirstOrFallback(part => part.def == RH_TET_DwarfDefOf.RH_TET_Dwarfs_BP_Beard)))
                    {
                        List<BodyPartRecord> parts = __result.RaceProps.body.GetPartsWithDef(RH_TET_DwarfDefOf.RH_TET_Dwarfs_BP_Beard);
                        BodyPartRecord part;
                        if (parts.Count > 0)
                            part = parts.First();
                        else
                            return;

                        HediffDef hediffDefFromDamage = DamageDefOf.SurgicalCut.hediff;
                        Hediff_MissingPart hediffMissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, __result, (BodyPartRecord)null);
                        hediffMissingPart.lastInjury = hediffDefFromDamage;
                        hediffMissingPart.Part = part;
                        hediffMissingPart.IsFresh = false;

                        __result.health.AddHediff((Hediff)hediffMissingPart, part, new DamageInfo?(), (DamageWorker.DamageResult)null);

                        //__result.TakeDamage(new DamageInfo(DamageDefOf.SurgicalCut, 99999f, 999f, -1f, (Thing)null, part, (ThingDef)null, DamageInfo.SourceCategory.ThingOrUnknown, (Thing)null, false, false, QualityCategory.Normal, true));
                        //__result.health.RestorePart(part, (Hediff)null, true);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(InfestationCellFinder))]
        [HarmonyPatch("TryFindCell")]
        internal static class Patch_TryFindCell
        {
            private static void Postfix(bool __result, ref IntVec3 cell, Map map)
            {
                if (__result)
                {
                    bool checkSettingsChance = Verse.Rand.Chance((float)Settings.LureChance / 100f);

                    // If there's a lure, and it's powered.
                    if (checkSettingsChance && map.IsPlayerHome && (map.listerBuildings.ColonistsHaveBuilding((Func<Thing, bool>)(building => building is Building_InfestationLure))))
                    {
                        List<Thing> things = map.listerThings.ThingsOfDef(RH_TET_DwarfDefOf.RH_TET_Dwarfs_SteamInfestationLure);
                        Building_InfestationLure building = null;

                        if (!things.NullOrEmpty())
                            building = (Building_InfestationLure)things.RandomElement(); // Random, even though intention is to allow one per map.
                        else
                            return;

                        CompSteamUser steamComp = building.GetComp<CompSteamUser>();
                        if (steamComp != null && steamComp.isOperational())
                            cell = building.Position;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(BuildingProperties), "IsMortar", MethodType.Getter)]
        static class Patch_BuildingProperties_IsMortar
        {
            static void Postfix(ref BuildingProperties __instance, ref bool __result)
            {
                if (!__result)
                {
                    if (__instance != null && __instance.turretGunDef != null &&
                        (
                        __instance.turretGunDef.defName.Equals("RH_TET_Dwarfs_GrudgeThrower_Turret")
                        ))
                    {
                        __result = true;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Root), "Start")]
        internal static class Verse_Root_Start
        {
            private static void Postfix() => RH_TET_DwarfsMod.OnGameLoad();
        }

        [HarmonyPatch(typeof(IdeoStyleTracker), "StyleForThingDef", (new[] { typeof(ThingDef), typeof(Precept) }))]
        static class Patch_IdeoStyleTracker_StyleForThingDef
        {
            static bool Prefix(ref StyleCategoryPair __result, ref Dictionary<ThingDef, StyleCategoryPair> ___styleForThingDef, ref Ideo ___ideo, ref ThingDef thing, ref Precept precept)
            {
                if (thing ==null)
                {
                    __result = null;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(QuestUtility), "GenerateQuestAndMakeAvailable", (new[] { typeof(QuestScriptDef), typeof(Slate) }))]
        static class Patch_QuestUtility_GenerateQuestAndMakeAvailable
        {
            static bool Prefix(ref Quest __result, ref QuestScriptDef root, ref Slate vars)
            {
                Faction ofPlayer = Faction.OfPlayerSilentFail;

                // Should only happen to dwarfs.
                if (ofPlayer != null && root != null && root.defName != null && root.defName.Equals("RH_TET_Dwarfs_EndGame_CapturedHold"))
                {
                    if (!(ofPlayer.def.defName.Equals("RH_TET_Dwarf_PlayerColony")
                        || ofPlayer.def.defName.Equals("RH_TET_Dwarf_Slayer_PlayerColony")))
                    {
                        // For non dwarfs, swap it out with a different quest.
                        root = QuestScriptDefOf.OpportunitySite_ItemStash;
                    }
                }
                
                return true;
            }
        }

        [HarmonyPatch(typeof(IncidentWorker_WandererJoin), "TryExecuteWorker")]
        static class Patch_IncidentWorker_WandererJoin_TryExecuteWorker
        {
            static bool Prefix(IncidentWorker_WandererJoin __instance, ref bool __result, IncidentParms parms)
            {
                Faction ofPlayer = Faction.OfPlayer;
                if (ofPlayer != null)
                { 
                    // If this happens to a dwarf colony, update the event to be Dwarf in Black, and Dwarf Wanderer Joins.
                    if (ofPlayer.def.defName.Equals("RH_TET_Dwarf_PlayerColony"))
                    { 
                        if (__instance.def.defName.Equals("StrangerInBlackJoin"))
                        {
                            // Maybe it will be a slayer.
                            if (RH_TET_DwarfsMod.random.Next(0,2) == 0)
                            {
                                __instance.def.pawnKind = PawnKindDef.Named("RH_TET_DwarfSlayer");
                                __instance.def.letterText = "Sensing trouble, a wandering dwarf slayer has arrived.\n\nWill [PAWN_pronoun] be able to save the day, or will [PAWN_pronoun] meet an honourable death attempting?";
                                __instance.def.letterLabel = "Slayer Savior";
                            }
                            else
                            {
                                __instance.def.pawnKind = PawnKindDef.Named("RH_TET_DwarfInBlack");
                                __instance.def.letterText = "Sensing trouble, a mysterious [PAWN_kind] has arrived.\n\nWill [PAWN_pronoun] be able to set things right in these parts?";
                                __instance.def.letterLabel = "Dwarf in Black";
                            }
                        }
                        else if (__instance.def.defName.Equals("WandererJoin"))
                        {
                            // Maybe it will be a slayer.
                            if (RH_TET_DwarfsMod.random.Next(0, 4) == 0)
                            {
                                __instance.def.pawnKind = PawnKindDef.Named("RH_TET_DwarfSlayerVillager");
                            }
                            else
                            {
                                __instance.def.pawnKind = PawnKindDef.Named("RH_TET_DwarfVillager");
                            }
                        }
                        else if (__instance.def.defName.Equals("RefugeeChased"))
                        {
                            __instance.def.pawnKind = PawnKindDef.Named("RH_TET_DwarfVillager");
                        }
                    }
                    else if (ofPlayer.def.defName.Equals("RH_TET_Dwarf_Slayer_PlayerColony"))
                    {
                        if (__instance.def.defName.Equals("StrangerInBlackJoin"))
                        {
                            // Maybe it will not be a slayer.
                            if (RH_TET_DwarfsMod.random.Next(0, 3) == 0)
                            {
                                __instance.def.pawnKind = PawnKindDef.Named("RH_TET_DwarfInBlack");
                                __instance.def.letterText = "Sensing trouble, a mysterious [PAWN_kind] has arrived.\n\nWill [PAWN_pronoun] be able to set things right in these parts?";
                                __instance.def.letterLabel = "Dwarf in Black";
                            }
                            else
                            {
                                __instance.def.pawnKind = PawnKindDef.Named("RH_TET_DwarfSlayer");
                                __instance.def.letterText = "Sensing trouble, a wandering dwarf slayer has arrived.\n\nWill [PAWN_pronoun] be able to save the day, or will [PAWN_pronoun] meet an honourable death attempting?";
                                __instance.def.letterLabel = "Slayer Savior";
                            }
                        }
                        else if (__instance.def.defName.Equals("WandererJoin"))
                        {
                            // Maybe it will not be a slayer.
                            if (RH_TET_DwarfsMod.random.Next(0, 4) == 0)
                            {
                                __instance.def.pawnKind = PawnKindDef.Named("RH_TET_DwarfVillager");
                            }
                            else
                            {
                                __instance.def.pawnKind = PawnKindDef.Named("RH_TET_DwarfSlayerVillager");
                            }
                        }
                        else if (__instance.def.defName.Equals("RefugeeChased"))
                        {
                            __instance.def.pawnKind = PawnKindDef.Named("RH_TET_DwarfVillager");
                        }
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(QuestNode_GeneratePawn), "RunInt")]
        static class Patch_QuestNode_GeneratePawn_TryExecuteWorker
        {
            static bool Prefix(QuestNode_GeneratePawn __instance)
            {
                Faction ofPlayer = Faction.OfPlayer;
                if (ofPlayer != null)
                {
                    if (RH_TET_DwarfsMod.random.Next(0, 3) == 0 || ofPlayer.def.defName.Equals("RH_TET_Dwarf_Slayer_PlayerColony"))
                    {
                        PawnKindDefOf.SpaceRefugee = PawnKindDef.Named("RH_TET_DwarfSlayerVillager");
                    }
                    else if (ofPlayer.def.defName.Equals("RH_TET_Dwarf_PlayerColony"))
                    {
                        PawnKindDefOf.SpaceRefugee = PawnKindDef.Named("RH_TET_DwarfClansman");
                    }
                }

                return true;
            }

            static void Postfix()
            {
                Faction ofPlayer = Faction.OfPlayer;

                if (ofPlayer != null)
                { 
                    // If this happens to a dwarf colony, update the event to be Dwarf in Black, and Dwarf Wanderer Joins.
                    if (ofPlayer.def.defName.Equals("RH_TET_Dwarf_PlayerColony") || ofPlayer.def.defName.Equals("RH_TET_Dwarf_Slayer_PlayerColony"))
                    {
                        PawnKindDefOf.SpaceRefugee = PawnKindDef.Named("SpaceRefugee");
                    }
                }
            }
        }

        [HarmonyPatch(typeof(WildManUtility), "IsWildMan")]
        static class Patch_WildManUtility_IsWildMan
        {
            static bool Prepare()
            {
                return true;
            }

            static void Postfix(ref bool __result, Pawn p)
            {
                if (__result)
                    return;

                if (p.kindDef == PawnKindDef.Named("RH_TET_Dwarfs_WildDwarf") || p.kindDef == PawnKindDef.Named("RH_TET_Dwarfs_WildSlayer"))
                {
                    __result = true;
                }
            }
        }

        [HarmonyPatch(typeof(WildManUtility), "AnimalOrWildMan")]
        static class Patch_WildManUtility_AnimalOrWildMan
        {
            static void Postfix(ref bool __result, Pawn p)
            {
                if (__result)
                    return;

                if (p.kindDef == PawnKindDef.Named("RH_TET_Dwarfs_WildDwarf") || p.kindDef == PawnKindDef.Named("RH_TET_Dwarfs_WildSlayer"))
                {
                    __result = true;
                }
            }
        }

        [HarmonyPatch(typeof(WildManUtility), "NonHumanlikeOrWildMan")]
        static class Patch_WildManUtility_NonHumanlikeOrWildMan
        {
            static void Postfix(ref bool __result, Pawn p)
            {
                if (__result)
                    return;

                if (p.kindDef == PawnKindDef.Named("RH_TET_Dwarfs_WildDwarf") || p.kindDef == PawnKindDef.Named("RH_TET_Dwarfs_WildSlayer"))
                {
                    __result = true;
                }
            }
        }

        [HarmonyPatch(typeof(DownedRefugeeQuestUtility), "GenerateRefugee")]
        static class Patch_DownedRefugeeQuestUtility_GenerateRefugee
        {
            static bool Prefix(ref Pawn __result, PlanetTile tile,
                  PawnKindDef pawnKind,
                  float chanceForFaction)
            {
                Faction ofPlayer = Faction.OfPlayer;

                if (ofPlayer != null)
                { 
                    if (ofPlayer.def.defName.Equals("RH_TET_Dwarf_PlayerColony"))
                    {
                        PawnKindDefOf.SpaceRefugee = PawnKindDef.Named("RH_TET_DwarfClansman");
                    }
                    else if (ofPlayer.def.defName.Equals("RH_TET_Dwarf_Slayer_PlayerColony"))
                    {
                        PawnKindDefOf.SpaceRefugee = PawnKindDef.Named("RH_TET_DwarfSlayerVillager");
                    }
                }
                return true;
            }

            static void Postfix(ref Pawn __result, PlanetTile tile,
                  PawnKindDef pawnKind,
                  float chanceForFaction)
            {
                Faction ofPlayer = Faction.OfPlayer;

                // If this happens to a dwarf colony, update the event to be Dwarf in Black, and Dwarf Wanderer Joins.
                if (ofPlayer != null && (ofPlayer.def.defName.Equals("RH_TET_Dwarf_PlayerColony") || ofPlayer.def.defName.Equals("RH_TET_Dwarf_Slayer_PlayerColony")))
                {
                    PawnKindDefOf.SpaceRefugee = PawnKindDef.Named("SpaceRefugee");
                }
            }
        }

        [HarmonyPatch(typeof(FoodUtility), "ThoughtsFromIngesting")]
        static class Patch_FoodUtility_ThoughtsFromIngesting
        {
            static void Postfix(ref List<FoodUtility.ThoughtFromIngesting> __result, Pawn ingester, Thing foodSource, ThingDef foodDef)
            {
                FoodUtility.ThoughtFromIngesting thoughtFromIngesting = new FoodUtility.ThoughtFromIngesting();
                thoughtFromIngesting.thought = RH_TET_DwarfDefOf.RH_TET_Dwarfs_AteFoodInappropriateForHighBlood;

                if (DwarfsUtil.InappropriateForHighBlood(foodDef, ingester, false))
                    __result.Add(thoughtFromIngesting);
            }
        }

        [HarmonyPatch(typeof(Pawn), "Destroy")]
        static class Patch_Pawn_Destroy
        {
            static void Prefix(Pawn __instance, DestroyMode mode)
            {
                if (DwarfsUtil.IsDwarf(__instance))
                { 
                    if (RH_TET_DwarfsMod.assignedThrones != null)
                    {
                        List<ThingWithComps> thrones = RH_TET_DwarfsMod.assignedThrones.TryGetValue(__instance);
                        if (thrones != null && thrones.Count > 0)
                        {
                            foreach (Building_DwarfThrone throne in thrones)
                            {
                                throne.CompAssignableToPawn.ForceRemovePawn(__instance);
                            }

                            RH_TET_DwarfsMod.assignedThrones.Remove(__instance);
                        }
                    }

                    if (RH_TET_DwarfsMod.king != null && RH_TET_DwarfsMod.king.Equals(__instance))
                        RH_TET_DwarfsMod.king = null;
                    else if (RH_TET_DwarfsMod.thanes != null && RH_TET_DwarfsMod.thanes.Count > 0 && RH_TET_DwarfsMod.thanes.Contains(__instance))
                    {
                        RH_TET_DwarfsMod.thanes.Remove(__instance);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Pawn))]
        [HarmonyPatch("CombinedDisabledWorkTags", MethodType.Getter)]
        static class Patch_Pawn_CombinedDisabledWorkTags
        {
            static void Postfix(Pawn __instance, ref WorkTags __result)
            {
                CompDwarfHighBlood comp = __instance.TryGetComp<CompDwarfHighBlood>();

                if (comp == null || comp.highBloodComp == null || comp.highBloodComp.GetCurrentHighBlood() == null || comp.highBloodComp.GetCurrentHighBlood().DisabledWorkTypes.Count() == 0)
                    return;
                
                HighBlood highBlood = comp.highBloodComp.HighBloodInEffectForReading.First();
                
                //if (highBlood.highMaintenance)
                __result |= highBlood.def.disabledWorkTags;
            }
        }

        [HarmonyPatch(typeof(Pawn), "GetDisabledWorkTypes")]
        static class Patch_Pawn_GetDisabledWorkTypes
        {
            static void Postfix(Pawn __instance, ref List<WorkTypeDef> __result, bool permanentOnly)
            {
                if (!permanentOnly)
                { 
                    CompDwarfHighBlood comp = __instance.TryGetComp<CompDwarfHighBlood>();
                    
                    if (comp == null || comp.highBloodComp == null || comp.highBloodComp.GetCurrentHighBlood() == null || comp.highBloodComp.GetCurrentHighBlood().DisabledWorkTypes.Count() == 0)
                        return;
                    
                    HighBlood highBlood = comp.highBloodComp.HighBloodInEffectForReading.First();

                    foreach (WorkTypeDef disabledWorkType in highBlood.def.DisabledWorkTypes)
                    {
                        if (!__result.Contains(disabledWorkType))
                            __result.Add(disabledWorkType);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(GatheringsUtility), "FindRandomGatheringOrganizer")]
        static class Patch_GatheringsUtility_FindRandomGatheringOrganizer
        {
            static void Postfix(ref Pawn __result, Faction faction, Map map, GatheringDef gatheringDef)
            {
                if(gatheringDef.Equals(RH_TET_DwarfDefOf.RH_TET_Dwarf_HighBloodCourt))
                {
                    if(RH_TET_DwarfsMod.king != null || (RH_TET_DwarfsMod.thanes != null && RH_TET_DwarfsMod.thanes.Count > 0))
                    {
                        Faction ofPlayer = Faction.OfPlayer;

                        if (ofPlayer != null && (ofPlayer.def.defName.Equals("RH_TET_Dwarf_PlayerColony") || ofPlayer.def.defName.Equals("RH_TET_Dwarf_Slayer_PlayerColony")))
                        {
                            List<Pawn> pawnsToCheck = new List<Pawn>();
                            pawnsToCheck.AddRange(RH_TET_DwarfsMod.thanes);
                            if (RH_TET_DwarfsMod.king != null)
                            {
                                pawnsToCheck.Add(RH_TET_DwarfsMod.king);
                            }
                            if ((__result == null || !pawnsToCheck.Contains(__result)) && pawnsToCheck.Count > 0)
                            {
                                __result = pawnsToCheck.RandomElement();
                            }
                            else if (pawnsToCheck.Count == 0)
                            {
                                if (__result != null && (__result.royalty == null || !__result.royalty.AllTitlesInEffectForReading.NullOrEmpty()))
                                    __result = null;
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CharacterCardUtility), "GetWorkTypeDisableCauses")]
        static class Patch_CharacterCardUtility_GetWorkTypeDisableCauses
        {
            static void Postfix(ref List<object> __result, Pawn pawn, WorkTags workTag)
            {
                CompDwarfHighBlood comp = pawn.TryGetComp<CompDwarfHighBlood>();

                if (comp == null || comp.highBloodComp == null || comp.highBloodComp.GetCurrentHighBlood() == null || comp.highBloodComp.GetCurrentHighBlood().DisabledWorkTypes.Count() == 0)
                    return;

                HighBlood highBlood = comp.highBloodComp.HighBloodInEffectForReading.First();
                if ((highBlood.def.disabledWorkTags & workTag) != WorkTags.None)
                    __result.Add((object)highBlood);
            }
        }

        // Removed for 1.6.
        //[HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
        //static class Patch_FloatMenuMakerMap_AddHumanlikeOrders
        //{
        //    static void Postfix(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        //    {
        //        if (DwarfsUtil.IsDwarf(pawn))
        //        {
        //            if ((RH_TET_DwarfsMod.king != null && RH_TET_DwarfsMod.king.Equals(pawn))
        //                || (RH_TET_DwarfsMod.thanes != null && RH_TET_DwarfsMod.thanes.Count > 0 && RH_TET_DwarfsMod.thanes.Contains(pawn)))
        //            {
        //                CompDwarfHighBlood comp = pawn.TryGetComp<CompDwarfHighBlood>();

        //                if (comp == null)
        //                    Log.Error("Dwarf has null high blood comp.");
                        
        //                IntVec3 c = IntVec3.FromVector3(clickPos);

        //                foreach (Thing thing in c.GetThingList(pawn.Map))
        //                {
        //                    Thing t = thing;
        //                    if ((t.def.ingestible != null && pawn.RaceProps.CanEverEat(t) && t.IngestibleNow) && !(t.def.IsNonMedicalDrug && pawn.IsTeetotaler()))
        //                    {
        //                        if (DwarfsUtil.InappropriateForHighBlood(t.def, pawn, true))
        //                        {
        //                            FloatMenuOption fmoSave = null;
        //                            foreach (FloatMenuOption fmo in opts)
        //                            {
        //                                if (fmo.Label.Contains(t.LabelShort))
        //                                {
        //                                    fmoSave = fmo;
        //                                    break;
        //                                }
        //                            }

        //                            fmoSave.Label += ( ": " + "RH_TET_Dwarfs_HighBloodFoodBad".Translate((NamedArgument)comp.highBloodComp.GetCurrentHighBlood().GetLabelFor(pawn)));
        //                            fmoSave.Disabled = true;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        [HarmonyPatch(typeof(Mineable), "TrySpawnYield", new Type[] { typeof(Map), typeof(bool), typeof(Pawn)})]
        static class Patch_Mineable_TrySpawnYield
        {
            static void Postfix(Mineable __instance, Map map, bool moteOnWaste, Pawn pawn)
            {
                if (DwarfsUtil.IsDwarf(pawn))
                {
                    if (pawn.mindState.inspirationHandler.CurStateDef != null && pawn.mindState.inspirationHandler.CurStateDef.Equals(RH_TET_DwarfDefOf.RH_TET_Dwarfs_Mining_Frenzy_Yield))
                    {
                        List<Thing> thingsAtSpot = __instance.Position.GetThingList(map);
                        bool chunkFound = false;
                        bool resourceFound = false;

                        foreach (Thing t in thingsAtSpot)
                        {
                            if (t.def == null)
                                continue;

                            if (t.def.IsMetal || t.def.Equals(ThingDefOf.Jade) || t.def.Equals(ThingDefOf.ComponentIndustrial)
                                || t.def.Equals(RH_TET_DwarfDefOf.RH_TET_Coal) || t.def.Equals(RH_TET_DwarfDefOf.RH_TET_Saltpeter) 
                                || t.def.Equals(RH_TET_DwarfDefOf.RH_TET_Sulfur))
                            {
                                resourceFound = true;
                                int stackc = t.stackCount;
                                stackc += (int)((float)stackc * .5f);
                                stackc = Mathf.Max(stackc, __instance.def.building.mineableYield);
                                t.stackCount = stackc;
                            }
                            else if (t.def.defName.StartsWith("Chunk"))
                            {
                                chunkFound = true;
                            }
                        }

                        // Spawn some chunks, if there were no resources found. 
                        if (!resourceFound && __instance.def.IsNonResourceNaturalRock)
                        { 
                            if (!chunkFound)
                            {
                                if (0 == RH_TET_DwarfsMod.random.Next(0, 2))
                                {
                                    // make sure the new cell is empty before putting something in it.
                                    GenSpawn.Spawn(__instance.def.building.mineableThing, __instance.Position, map);
                                }
                            }
                            else
                            {
                                if (0 == RH_TET_DwarfsMod.random.Next(0, 3))
                                {
                                    // make sure the new cell is empty before putting something in it.
                                    GenSpawn.Spawn(__instance.def.building.mineableThing, __instance.Position, map, WipeMode.VanishOrMoveAside);
                                }
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(InspirationWorker), "InspirationCanOccur")]
        static class Patch_InspirationWorker_InspirationCanOccur
        {
            static void Postfix(InspirationWorker __instance, ref bool __result, Pawn pawn)
            {
                if (__result) 
                {
                    if (__instance.def.Equals(RH_TET_DwarfDefOf.RH_TET_Dwarfs_Mining_Frenzy_Yield)
                        || __instance.def.Equals(RH_TET_DwarfDefOf.RH_TET_Dwarfs_Inspired_Tending)
                        || __instance.def.Equals(RH_TET_DwarfDefOf.RH_TET_Dwarfs_Frenzy_Combat)
                        || __instance.def.Equals(RH_TET_DwarfDefOf.RH_TET_Dwarfs_Frenzy_GenWork)
                        || __instance.def.Equals(RH_TET_DwarfDefOf.RH_TET_Dwarfs_Frenzy_Constr)
                        || __instance.def.Equals(RH_TET_DwarfDefOf.RH_TET_Dwarfs_Frenzy_Dodge))
                    {
                        if (!DwarfsUtil.IsDwarf(pawn))
                            __result = false;
                    }
                }
            }
        }

        // Alter Ideos here, if they're present.
        [HarmonyPatch(typeof(Alert_IdeoBuildingDisrespected), "GetReport")]
        static class Patch_Alert_IdeoBuildingDisrespected_GetReport
        {
            static bool Prefix()
            {
                if (RH_TET_DwarfsMod.ideologyAlertPatched)
                    return true;

                Faction ofPlayer = Faction.OfPlayerSilentFail;
                if (ofPlayer.def.defName.Equals("RH_TET_Dwarf_PlayerColony") || ofPlayer.def.defName.Equals("RH_TET_Dwarf_Slayer_PlayerColony"))
                {
                    if (ModsConfig.IdeologyActive)
                    {
                        PreceptDef ideoBuilding = DefDatabase<PreceptDef>.GetNamed("IdeoBuilding");
                        if (ideoBuilding != null)
                        {
                            foreach (RoomRequirement rr in ideoBuilding.buildingRoomRequirements)
                            {
                                if (rr != null && rr is RoomRequirement_ThingCount)
                                {
                                    RoomRequirement_ThingCount rrtc = rr as RoomRequirement_ThingCount;
                                    if (rrtc.thingDef == ThingDefOf.Column)
                                        rrtc.thingDef = RH_TET_DwarfDefOf.RH_TET_Dwarfs_ColumnSmallB;
                                }
                            }
                        }
                        else
                            Log.Warning("Rimhammer Dwarfs could not find IdeoBuilding in order to update required buildings.");
                    }

                    RH_TET_DwarfsMod.ideologyAlertPatched = true;
                }


                return true;
            }
        }

        // Alter Royalty Titles here, if they're present.
        [HarmonyPatch(typeof(Alert_UndignifiedThroneroom), "GetReport")]
        static class Patch_Alert_UndignifiedThroneroom_GetReport
        {
            static bool Prefix()
            {
                if (RH_TET_DwarfsMod.royaltyAlertPatched)
                    return true;

                Faction ofPlayer = Faction.OfPlayerSilentFail;
                if (ofPlayer.def.defName.Equals("RH_TET_Dwarf_PlayerColony") || ofPlayer.def.defName.Equals("RH_TET_Dwarf_Slayer_PlayerColony"))
                {
                    if (ModsConfig.RoyaltyActive)
                    {
                        RoyalTitleDef acolyte = DefDatabase<RoyalTitleDef>.GetNamed("Acolyte");
                        RoyalTitleDef knight = DefDatabase<RoyalTitleDef>.GetNamed("Knight");
                        RoyalTitleDef praetor = DefDatabase<RoyalTitleDef>.GetNamed("Praetor");
                        RoyalTitleDef baron = DefDatabase<RoyalTitleDef>.GetNamed("Baron");
                        RoyalTitleDef count = DefDatabase<RoyalTitleDef>.GetNamed("Count");
                        List<RoyalTitleDef> titles = new List<RoyalTitleDef>();
                        titles.Add(acolyte);
                        titles.Add(knight);
                        titles.Add(praetor);
                        titles.Add(baron);
                        titles.Add(count);

                        ThingDef endTable = DefDatabase<ThingDef>.GetNamed("EndTable");
                        ThingDef dresser = DefDatabase<ThingDef>.GetNamed("Dresser");
                        ThingDef doubleBed = DefDatabase<ThingDef>.GetNamed("DoubleBed");

                        foreach (RoyalTitleDef rtd in titles)
                        {
                            if (rtd != null && rtd.throneRoomRequirements != null)
                            {
                                foreach (RoomRequirement rr in rtd.throneRoomRequirements)
                                {
                                    if (rr is RoomRequirement_ThingCount)
                                    {
                                        RoomRequirement_ThingCount rrtc = rr as RoomRequirement_ThingCount;

                                        if (rrtc.thingDef == ThingDefOf.Brazier)
                                            rrtc.thingDef = RH_TET_DwarfDefOf.RH_TET_Dwarfs_Brazier;
                                        else if (rrtc.thingDef == ThingDefOf.Column)
                                            rrtc.thingDef = RH_TET_DwarfDefOf.RH_TET_Dwarfs_ColumnBigB;
                                        else if (rrtc.thingDef == ThingDefOf.Drape)
                                            rrtc.thingDef = RH_TET_DwarfDefOf.RH_TET_Dwarfs_Banner;
                                    }
                                    else if (rr is RoomRequirement_ThingAnyOfCount)
                                    {
                                        RoomRequirement_ThingAnyOfCount rrtaoc = rr as RoomRequirement_ThingAnyOfCount;

                                        List<ThingDef> replace = new List<ThingDef>();

                                        foreach (ThingDef thing in rrtaoc.things)
                                        { 
                                            if (thing == ThingDefOf.Brazier)
                                                replace.Add(thing);
                                            else if (thing == ThingDefOf.Column)
                                                replace.Add(thing);
                                            else if (thing == ThingDefOf.Drape)
                                                replace.Add(thing);
                                        }

                                        foreach(ThingDef thing in replace)
                                        {
                                            if (thing == ThingDefOf.Brazier)
                                                rrtaoc.things.Replace<ThingDef>(thing, RH_TET_DwarfDefOf.RH_TET_Dwarfs_Brazier);
                                            else if (thing == ThingDefOf.Column)
                                                rrtaoc.things.Replace<ThingDef>(thing, RH_TET_DwarfDefOf.RH_TET_Dwarfs_ColumnBigB);
                                            else if (thing == ThingDefOf.Drape)
                                                rrtaoc.things.Replace<ThingDef>(thing, RH_TET_DwarfDefOf.RH_TET_Dwarfs_Banner);
                                        }
                                    }
                                    else if (rr is RoomRequirement_AllThingsAreGlowing)
                                    {
                                        RoomRequirement_AllThingsAreGlowing rrag = rr as RoomRequirement_AllThingsAreGlowing;

                                        if (rrag.thingDef == ThingDefOf.Brazier)
                                            rrag.thingDef = RH_TET_DwarfDefOf.RH_TET_Dwarfs_Brazier;
                                    }
                                    else if (rr is RoomRequirement_ThingAnyOf)
                                    {
                                        if (rtd.defName.Equals("Knight"))
                                        { 
                                            RoomRequirement_ThingAnyOf rrtao = rr as RoomRequirement_ThingAnyOf;
                                            rrtao.things.Add(RH_TET_DwarfDefOf.RH_TET_Dwarfs_Horn);
                                        }
                                    }
                                }
                            }
                            if (rtd != null && rtd.bedroomRequirements != null)
                            {
                                foreach (RoomRequirement rr in rtd.bedroomRequirements)
                                {
                                    if (rr is RoomRequirement_ThingAnyOf)
                                    {
                                        RoomRequirement_ThingAnyOf rrtao = rr as RoomRequirement_ThingAnyOf;

                                        if (rrtao.things.Contains(doubleBed))
                                        {
                                            rrtao.things.Add(RH_TET_DwarfDefOf.RH_TET_Dwarfs_DoubleBed);
                                        }
                                        if (rrtao.things.Contains(ThingDefOf.RoyalBed))
                                        { 
                                            rrtao.things.Add(RH_TET_DwarfDefOf.RH_TET_Dwarfs_RoyalBed);
                                        }
                                    }
                                    else if (rr is RoomRequirement_Thing)
                                    {
                                        RoomRequirement_Thing rrt = rr as RoomRequirement_Thing;

                                        if (rrt.thingDef.Equals(endTable))
                                        {
                                            rrt.thingDef = RH_TET_DwarfDefOf.RH_TET_Dwarfs_EndTable;
                                        }
                                        else if (rrt.thingDef.Equals(dresser))
                                        {
                                            rrt.thingDef = RH_TET_DwarfDefOf.RH_TET_Dwarfs_Dresser;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                RH_TET_DwarfsMod.royaltyAlertPatched = true;

                return true;
            }
        }

        [HarmonyPatch(typeof(ThingDef))]
        [HarmonyPatch("IsSmoothed", MethodType.Getter)]
        static class Patch_ThingDef_IsSmoothed
        {
            static bool Prefix(ref ThingDef __instance, ref bool __result)
            {
                if (__instance.defName.Contains("Dwarfs_Smoothed"))
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }
    }
}
