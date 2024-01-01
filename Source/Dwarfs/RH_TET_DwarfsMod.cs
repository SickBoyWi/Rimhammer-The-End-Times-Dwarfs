using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace TheEndTimes_Dwarfs
{
    [StaticConstructorOnStartup]
    public class RH_TET_DwarfsMod : Mod
    {
        public static System.Random random = new System.Random(Guid.NewGuid().GetHashCode());
        public static Dictionary<Pawn, List<ThingWithComps>> assignedThrones = new Dictionary<Pawn, List<ThingWithComps>>();
        public static Pawn king = null;
        public static List<Pawn> thanes = new List<Pawn>();
        public static List<Pawn> deadHighBloods = new List<Pawn>();
        public static List<string> godsUsed = new List<string>();
        private static bool anyOneCanActive = false;
        public static bool royaltyAlertPatched = false;
        public static bool ideologyAlertPatched = false;

        public static bool AnyOneCanDwarfModActive
        {
            get
            {
                if (ModsConfig.IsActive("SickBoyWi.TheEndTimes.DwarfsAnyOne"))
                {
                    anyOneCanActive = true;
                }
                else
                    anyOneCanActive = false;

                return anyOneCanActive;
            }
        }

        internal static void OnGameLoad()
        {
            RH_TET_DwarfsMod.king = null;
            RH_TET_DwarfsMod.assignedThrones = new Dictionary<Pawn, List<ThingWithComps>>();
            RH_TET_DwarfsMod.thanes = new List<Pawn>();
            RH_TET_DwarfsMod.godsUsed = new List<string>();
        }
        
        public RH_TET_DwarfsMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("rimworld.theendtimes.dwarfs");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            
            //InitializeDecoratedWalls();
            LongEventHandler.ExecuteWhenFinished(new Action(RH_TET_DwarfsMod.SetupRepairStuff));

        }

        private static void SetupRepairStuff()
        {
            var defsWithValues = DefDatabase<ThingDef>.AllDefs.Where<ThingDef>((Func<ThingDef, bool>)(def =>
            {
                if (!def.useHitPoints || def.ingestible != null)
                    return false;
                if (!def.IsApparel)
                {
                    if (def.IsWeapon)
                    {
                        if (def.thingCategories != null && def.thingCategories.Where<ThingCategoryDef>((Func<ThingCategoryDef, bool>)(cat => (cat.defName.Equals("RH_TET_MagicItems") || cat.defName.Equals("ResourcesRaw")))).Any())
                        {
                            return false;
                        }
                        else if (def.defName.Equals("ThrumboHorn") || def.defName.Equals("ElephantTusk")) 
                        {
                            return false;
                        }
                        return true;
                    }
                    return def.IsWeapon;
                }
                if (def.thingCategories != null && def.thingCategories.Where<ThingCategoryDef>((Func<ThingCategoryDef, bool>)(cat => (cat.defName.Equals("RH_TET_Artifacts") || cat.defName.Equals("RH_TET_WeaponsMagic")))).Any())
                    return false;
                return true;
            })).Select(def => new
                {
                    def = def,
                    isComplex = IsArmorOrFireArm(def),
                    isApparel = def.IsApparel
                });
            ThingFilter cleanFilter1 = GetEmptyFilter(RH_TET_DwarfDefOf.RH_TET_Dwarfs_RepairArmor);
            ThingFilter cleanFilter2 = GetEmptyFilter(RH_TET_DwarfDefOf.RH_TET_Dwarfs_RepairApparel);
            ThingFilter cleanFilter3 = GetEmptyFilter(RH_TET_DwarfDefOf.RH_TET_Dwarfs_RepairRangedWeapon);
            ThingFilter cleanFilter4 = GetEmptyFilter(RH_TET_DwarfDefOf.RH_TET_Dwarfs_RepairMeleeWeapon);

            foreach (var data in defsWithValues)
                (!data.isApparel ? (!data.isComplex ? cleanFilter4 : cleanFilter3) : (!data.isComplex ? cleanFilter2 : cleanFilter1)).SetAllow(data.def, true);
        }

        private static bool IsArmorOrFireArm(ThingDef def)
        {
            List<ThingDefCountClass> costList = def.costList;
            if (def.defName.Contains("Helmet") || def.defName.Contains("Armor") || def.defName.Contains("Gun") || def.defName.Contains("Rifle"))
                return true;
            if (costList == null)
                return false;
            return costList.Any<ThingDefCountClass>((Predicate<ThingDefCountClass>)(t =>
            {
                if (t.thingDef != ThingDefOf.ComponentIndustrial)
                    return t.thingDef == ThingDefOf.ComponentSpacer;
                return true;
            }));
        }

        private static ThingFilter GetEmptyFilter(RecipeDef recipe)
        {
            IngredientCount ingredientCount = new IngredientCount();
            if (recipe.ingredients == null)
                recipe.ingredients = new List<IngredientCount>();
            else
                recipe.ingredients.Clear();
            recipe.ingredients.Add(ingredientCount);
            ThingFilter filter = ingredientCount.filter;
            filter.SetDisallowAll((IEnumerable<ThingDef>)null, (IEnumerable<SpecialThingFilterDef>)null);
            filter.AllowedHitPointsPercents = new FloatRange()
            {
                min = 0.0f,
                max = 0.99f
            };
            recipe.defaultIngredientFilter = filter;
            recipe.fixedIngredientFilter = filter;
            return filter;
        }
    }
}