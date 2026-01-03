using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TheEndTimes_Dwarfs
{
    public static class DwarfsUtil
    {
        private static List<TraitDef> HighMaintenanceTraits = new List<TraitDef>()
              {
                TraitDefOf.Abrasive,
                TraitDefOf.Greedy,
                TraitDefOf.Jealous
              };

        public static void GiveMoodToPawns(ThoughtDef thought, IEnumerable<Pawn> pawns, Pawn otherpawn = null)
        {
            if (pawns != null && thought != null)
            { 
                foreach (Pawn podsAliveColonist in pawns)
                {
                    if (otherpawn == null || !podsAliveColonist.Equals(otherpawn))
                    { 
                        if (podsAliveColonist != null && podsAliveColonist.needs != null && podsAliveColonist.needs.mood != null && podsAliveColonist.needs.mood.thoughts != null && podsAliveColonist.needs.mood.thoughts.memories != null)
                            podsAliveColonist.needs.mood.thoughts.memories.TryGainMemory(thought, otherpawn);
                    }
                }
            }
        }

        public static void RemoveMoodFromPawns(ThoughtDef thought, IEnumerable<Pawn> pawns)
        {
            if (pawns != null && thought != null)
            {
                foreach (Pawn podsAliveColonist in pawns)
                {
                    if (podsAliveColonist != null && podsAliveColonist.needs != null && podsAliveColonist.needs.mood != null && podsAliveColonist.needs.mood.thoughts != null && podsAliveColonist.needs.mood.thoughts.memories != null)
                        podsAliveColonist.needs.mood.thoughts.memories.RemoveMemoriesOfDef(thought);
                }
            }
        }

        public static bool IsDwarf(Pawn p)
        {
            if (p == null)
                return false;

            if (p.RaceProps.Humanlike 
                && ((p.def.defName.Equals("RH_TET_Dwarf_Race_Standard")
                    || p.def.defName.Equals("RH_TET_Dwarf_Slayer_Race_Standard"))))
            {
                return true;
            }
            
            return false;
        }

        public static bool IsDwarf(ThingDef def)
        {
            if ((def.defName.Equals("RH_TET_Dwarf_Race_Standard")
                    || def.defName.Equals("RH_TET_Dwarf_Slayer_Race_Standard")))
            {
                return true;
            }

            return false;
        }

        public static bool IsSlayer(Pawn p)
        {
            if (p.RaceProps.Humanlike
                && p.def.defName.Equals("RH_TET_Dwarf_Slayer_Race_Standard"))
            {
                return true;
            }

            return false;
        }

        internal static bool IsHighBlood(Pawn pawn)
        {
            if (RH_TET_DwarfsMod.king != null && RH_TET_DwarfsMod.king.Equals(pawn))
                return true;
            else if (RH_TET_DwarfsMod.thanes != null && RH_TET_DwarfsMod.thanes.Count > 0 && RH_TET_DwarfsMod.thanes.Contains(pawn))
                return true;

            return false;
        }

        internal static bool IsKing(Pawn pawn)
        {
            if (RH_TET_DwarfsMod.king != null && RH_TET_DwarfsMod.king.Equals(pawn))
                return true;

            return false;
        }

        internal static bool IsThane(Pawn pawn)
        {
            if (RH_TET_DwarfsMod.thanes != null && RH_TET_DwarfsMod.thanes.Count > 0 && RH_TET_DwarfsMod.thanes.Contains(pawn))
                return true;

            return false;
        }
        
        public static Building_DwarfThrone FindBestUsableThrone(Pawn pawn)
        {
            List<ThingWithComps> thrones = RH_TET_DwarfsMod.assignedThrones.TryGetValue(pawn);

            Building_DwarfThrone buildingThrone = null;

            if (thrones != null && thrones.Count > 0)
            {
                foreach (Building_DwarfThrone throne in thrones)
                {
                    if (throne != null
                        && (throne.Spawned || !throne.IsForbidden(pawn) 
                        || pawn.CanReserveAndReach((LocalTargetInfo)((Thing)throne), PathEndMode.InteractionCell, pawn.NormalMaxDanger(), 1, -1, (ReservationLayerDef)null, false))
                        && RoomRoleWorker_DwarfRulingRoom.Validate(throne.GetRoom(RegionType.Set_Passable)) == null)
                    {
                        buildingThrone = throne;
                    }
                }
            }

            return buildingThrone;
        }

        public static Faction GetInsectsOrSkavenFaction()
        {
            Faction newFact = Faction.OfInsects;

            if (Find.FactionManager.AllFactions.Any(f => f == newFact))
                return newFact;

            Faction newSFact = Find.FactionManager.AllFactions.First(f => f.def.defName == "RH_TET_Skaven_UnderEmpireFaction");

            if (newSFact != null)
                newFact = newSFact;

            return newFact;
        }

        public static bool ShouldBecomeHighMaintenanceOnHighBlood(Pawn p)
        {
            TraitSet traits = p.story?.traits;
            if (traits != null && traits.HasTrait(TraitDefOf.Ascetic))
                return false;
            if (p.Faction != null && p.Faction.IsPlayer)
                return DwarfsUtil.GetHighMaintenanceTraits(p).Any<Trait>();
            return true;
        }

        public static IEnumerable<Trait> GetHighMaintenanceTraits(Pawn p)
        {
            TraitSet traits = p.story?.traits;
            if (traits != null)
            {
                for (int i = 0; i < DwarfsUtil.HighMaintenanceTraits.Count; ++i)
                {
                    Trait trait = traits.GetTrait(DwarfsUtil.HighMaintenanceTraits[i]);
                    if (trait != null)
                        yield return trait;
                }
            }
        }

        public static void SpawnItemsNear(List<ThingDef> itemsToSpawn, Building_DwarfThrone throne)
        {
            IntVec3 spawnCell;
            CellRect cellRectAroundThrone = new CellRect(throne.InteractionCell.x - 3, throne.InteractionCell.z - 3, 5, 5);
            Map map = throne.Map;

            foreach (ThingDef thingDef in itemsToSpawn)
            { 
                TryFindSpawnCellForItem(cellRectAroundThrone, map, out spawnCell);

                Thing thingToSpawn = null;
                if (thingDef.IsWeapon)
                    thingToSpawn = ThingMaker.MakeThing(thingDef);
                else
                    thingToSpawn = ThingMaker.MakeThing(thingDef, RH_TET_DwarfDefOf.RH_TET_Dwarf_Gromril);

                thingToSpawn.TryGetComp<CompQuality>().SetQuality(QualityCategory.Legendary, ArtGenerationContext.Outsider);
                GenSpawn.Spawn(thingToSpawn, spawnCell, map);
            }
        }

        private static bool TryFindSpawnCellForItem(CellRect rect, Map map, out IntVec3 result)
        {
            return CellFinder.TryFindRandomCellInsideWith(rect, (Predicate<IntVec3>)(c =>
            {
                if (c.GetFirstItem(map) != null)
                    return false;
                if (!c.Standable(map))
                {
                    switch (c.GetSurfaceType(map))
                    {
                        case SurfaceType.Item:
                        case SurfaceType.Eat:
                            break;
                        default:
                            return false;
                    }
                }
                return true;
            }), out result);
        }

        public static bool InappropriateForHighBlood(ThingDef food, Pawn p, bool allowIfStarving)
        {
            if (allowIfStarving && p.needs.food.Starving || p.story != null && p.story.traits.HasTrait(TraitDefOf.Ascetic) || (p.IsPrisoner || food.ingestible.joyKind != null && (double)food.ingestible.joy > 0.0))
                return false;

            CompDwarfHighBlood comp = p.TryGetComp<CompDwarfHighBlood>();

            if (comp != null && comp.highBloodComp != null && comp.highBloodComp.GetCurrentHighBlood() != null)
            { 
                HighBlood highBlood = comp.highBloodComp.HighestHighBloodWithBedroomRequirements();
                if (highBlood != null && highBlood.highMaintenance && highBlood.def.foodRequirement.Defined)
                    return !highBlood.def.foodRequirement.Acceptable(food);
            }
            return false;
        }

        public static bool IsWallRockDoorOrSolid(Building b)
        {
            if (b == null)
                return false;
            if (b.def.defName.Contains("Wall"))
                return true;
            else if (b.def.building.isNaturalRock)
                return true;
            else if (b.def.fillPercent > .75f)
                return true;
            if (b.def.defName.Contains("Door") || b.def.defName.Contains("Gate"))
                return true;
            return false;
        }
    }
}