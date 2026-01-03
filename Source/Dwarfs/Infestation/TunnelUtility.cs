using RimWorld;
using System.Collections.Generic;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public static class TunnelUtility
    {
        private const float TunnelPreventsClaimingInRadius = 2f;

        public static int TotalSpawnedTunnelsCount(Map map)
        {
            return map.listerThings.ThingsOfDef(RH_TET_DwarfDefOf.RH_TET_SkavenTunnel).Count;
        }

        public static bool AnyTunnelPreventsClaiming(Thing thing)
        {
            if (!thing.Spawned)
                return false;
            int num = GenRadial.NumCellsInRadius(2f);
            for (int index = 0; index < num; ++index)
            {
                IntVec3 c = thing.Position + GenRadial.RadialPattern[index];
                if (c.InBounds(thing.Map) && c.GetFirstThing<SkavenTunnel>(thing.Map) != null)
                    return true;
            }
            return false;
        }

        public static void Notify_TunnelDespawned(SkavenTunnel tunnel, Map map)
        {
            int num = GenRadial.NumCellsInRadius(2f);
            for (int index1 = 0; index1 < num; ++index1)
            {
                IntVec3 c = tunnel.Position + GenRadial.RadialPattern[index1];
                if (c.InBounds(map))
                {
                    List<Thing> thingList = c.GetThingList(map);
                    for (int index2 = 0; index2 < thingList.Count; ++index2)
                    {
                        if (thingList[index2].Faction == DwarfsUtil.GetInsectsOrSkavenFaction() && !TunnelUtility.AnyTunnelPreventsClaiming(thingList[index2]) && !(thingList[index2] is Pawn))
                            thingList[index2].SetFaction((Faction)null, (Pawn)null);
                    }
                }
            }
        }
    }
}
