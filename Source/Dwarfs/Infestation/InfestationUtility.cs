using RimWorld;
using System;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public static class InfestationUtility
    {
        public static Thing SpawnTunnels(
          int tunnelCount,
          Map map,
          bool spawnAnywhereIfNoGoodCell = false,
          string questTag = null)
        {
            IntVec3 childTunnelLocation;
            if (!InfestationCellFinder.TryFindCell(out childTunnelLocation, map) && (!spawnAnywhereIfNoGoodCell || !RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith((Predicate<IntVec3>)(x => x.Standable(map)), map, out childTunnelLocation)))
                return (Thing)null;
            Thing thing = GenSpawn.Spawn(ThingMaker.MakeThing(RH_TET_DwarfDefOf.RH_TET_SkavenTunnelSpawner, (ThingDef)null), childTunnelLocation, map, WipeMode.FullRefund);
            QuestUtility.AddQuestTag((object)thing, questTag);
            for (int index = 0; index < tunnelCount - 1; ++index)
            {
                childTunnelLocation = CompSpawnerTunnels.FindChildTunnelLocation(thing.Position, map, RH_TET_DwarfDefOf.RH_TET_SkavenTunnel, RH_TET_DwarfDefOf.RH_TET_SkavenTunnel.GetCompProperties<CompProperties_SpawnerTunnels>(), true, false);
                if (childTunnelLocation.IsValid)
                {
                    thing = GenSpawn.Spawn(ThingMaker.MakeThing(RH_TET_DwarfDefOf.RH_TET_SkavenTunnelSpawner, (ThingDef)null), childTunnelLocation, map, WipeMode.FullRefund);
                    QuestUtility.AddQuestTag((object)thing, questTag);
                }
            }
            return thing;
        }
    }
}
