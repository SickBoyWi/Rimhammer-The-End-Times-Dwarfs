using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace TheEndTimes_Dwarfs.RandomHarmonyStuff
{
    class Harmony_Patch_Plant
    {
        [HarmonyPatch(typeof(RimWorld.Plant))]
        [HarmonyPatch("HasEnoughLightToGrow", MethodType.Getter)]
        static class Patch_Plant_HasEnoughLightToGrow
        {
            static void Postfix(Plant __instance, ref bool __result)
            {
                if (__instance.def.defName == "RH_TET_Dwarf_Plant_Stoneroot")
                {
                    __result = true;
                }
            }
        }

        [HarmonyPatch(typeof(RimWorld.Plant))]
        [HarmonyPatch("GrowthRateFactor_Light", MethodType.Getter)]
        static class Patch_Plant_GrowthRateFactor_Light
        {
            static void Postfix(Plant __instance, ref float __result)
            {
                if (__instance.def.defName == "RH_TET_Dwarf_Plant_Stoneroot")
                {
                    float glowAmount = __instance.Map.glowGrid.GroundGlowAt(__instance.Position);
                    if (__instance.def.plant.growMinGlow == __instance.def.plant.growOptimalGlow && glowAmount == __instance.def.plant.growOptimalGlow)
                    {
                        __result = 1f;
                        return;
                    }

                    //  Subtract the light factor from perfect growing conditions, 1.0.  If in 1.0 light/full sun, then the plant will stop growing...
                    __result = 1f - glowAmount;
                    return;
                }
            }
        }
    }
}
