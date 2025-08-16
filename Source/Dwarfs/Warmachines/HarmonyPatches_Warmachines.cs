using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Text;
using UnityEngine;
using Verse;

namespace TheEndTimes_Dwarfs
{
    partial class HarmonyPatches_Warmachine
    {

        private static FloatRange smokeSize = new FloatRange(0.25f, 0.5f);
        private static int stackLimitSave = 0;

        [HarmonyPatch(typeof(Building_TurretGun), "BeginBurst")]
        static class Patch_Building_TurretGun_BeginBurst
        {
            static void Prefix(Building_TurretGun __instance)
            {
                if (__instance.def.defName.Equals("RH_TET_Dwarfs_OrganGun"))
                {
                    stackLimitSave = __instance.gun.TryGetComp<CompChangeableProjectile>().LoadedShell.stackLimit;

                    __instance.gun.TryGetComp<CompChangeableProjectile>().LoadedShell.stackLimit = 4;
                    __instance.gun.TryGetComp<CompChangeableProjectile>().LoadShell(ThingDef.Named("RH_TET_Dwarfs_OrganGun_Round"), 4);
                }
                else if (__instance.def.defName.Equals("RH_TET_Dwarfs_GoblinHewer"))
                {
                    stackLimitSave = __instance.gun.TryGetComp<CompChangeableProjectile>().LoadedShell.stackLimit;

                    __instance.gun.TryGetComp<CompChangeableProjectile>().LoadedShell.stackLimit = 6;
                    __instance.gun.TryGetComp<CompChangeableProjectile>().LoadShell(ThingDef.Named("RH_TET_Dwarfs_GoblinHewer_Round"), 6);
                }
            }

            static void Postfix(Building_TurretGun __instance)
            {
                if (__instance.def.defName.Equals("RH_TET_Dwarfs_Cannon")
                    || __instance.def.defName.Equals("RH_TET_Dwarfs_FlameCannon"))
                {
                    ResolveSmoke(__instance);
                }
                else if (__instance.def.defName.Equals("RH_TET_Dwarfs_OrganGun"))
                {
                    __instance.gun.TryGetComp<CompChangeableProjectile>().LoadedShell.stackLimit = stackLimitSave; 
                    ResolveSmoke(__instance);
                }
                else if (__instance.def.defName.Equals("RH_TET_Dwarfs_GoblinHewer"))
                {
                    __instance.gun.TryGetComp<CompChangeableProjectile>().LoadedShell.stackLimit = stackLimitSave;
                }
            }

            private static void ResolveSmoke(Building_TurretGun __instance)
            {
                IntVec3 position = __instance.Position;
                Map map = __instance.Map;
                float statValue = 2f;
                DamageDef smoke = DamageDefOf.Smoke;
                GenExplosion.DoExplosion(position, map, statValue, smoke, null, -1, -1f, null, null, null, null, null, 0f, 0, new GasType?(GasType.BlindSmoke), new float?(), (int)byte.MaxValue, false, null, 0f, 1, 0f, false);
            }
        }
    }
}
