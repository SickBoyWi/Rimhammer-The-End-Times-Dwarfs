using HugsLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TheEndTimes_Dwarfs
{
    [StaticConstructorOnStartup]
    public static class BlastingUtility
    {
        public static FloatMenuOption TryMakeDetonatorFloatMenuOption(
          Pawn pawn,
          IPawnDetonateable detonator)
        {
            Verse.Thing detonatorThing = detonator as Verse.Thing;
            if (pawn == null || detonatorThing == null || !pawn.IsColonistPlayerControlled || pawn.drafter == null)
                return (FloatMenuOption)null;
            FloatMenuOption floatMenuOption1 = new FloatMenuOption("RH_TET_Dwarfs_BlastNow".Translate(), (Action)(() =>
            {
                if (!pawn.IsColonistPlayerControlled)
                    return;
                if (!detonator.WantsDetonation)
                    detonator.WantsDetonation = true;
                pawn.jobs.TryTakeOrderedJob(new Verse.AI.Job(RH_TET_DwarfDefOf.RH_TET_Dwarfs_DetonateBlast, (LocalTargetInfo)detonatorThing), JobTag.Misc);
            }), MenuOptionPriority.Default, null, (Verse.Thing)null, 0.0f, (Func<Rect, bool>)null, (WorldObject)null);
            if (pawn.Map.reservationManager.IsReservedAndRespected((LocalTargetInfo)detonatorThing, pawn))
            {
                floatMenuOption1.Disabled = true;
                string toStringShort = pawn.Map.reservationManager.FirstRespectedReserver((LocalTargetInfo)detonatorThing, pawn).Name.ToStringShort;
                FloatMenuOption floatMenuOption2 = floatMenuOption1;
                floatMenuOption2.Label = floatMenuOption2.Label + " " + "RH_TET_Dwarfs_BlasterReserved".Translate((NamedArgument)toStringShort);
            }
            return floatMenuOption1;
        }

        public static bool IsEffectiveRoofBreakerPlacement(
          float explosiveRadius,
          IntVec3 center,
          Map map)
        {
            if ((double)explosiveRadius <= 0.0)
                return false;
            RoofGrid roofGrid = map.roofGrid;
            IntVec3[] cardinalDirections = GenAdj.CardinalDirections;
            int num = GenRadial.NumCellsInRadius(explosiveRadius);
            bool flag1 = false;
            bool flag2 = false;
            for (int index1 = 0; index1 < num; ++index1)
            {
                IntVec3 c1 = center + GenRadial.RadialPattern[index1];
                if (c1.InBounds(map))
                {
                    RoofDef roofDef1 = roofGrid.RoofAt(c1);
                    if (roofDef1 != null && roofDef1.isThickRoof)
                        flag2 = true;
                    if (!flag1)
                    {
                        for (int index2 = 0; index2 < cardinalDirections.Length; ++index2)
                        {
                            IntVec3 c2 = cardinalDirections[index2] + c1;
                            if (c2.InBounds(map))
                            {
                                RoofDef roofDef2 = roofGrid.RoofAt(c2);
                                if (roofDef2 == null || !roofDef2.isThickRoof)
                                {
                                    flag1 = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return flag2 & flag1;
        }

        public static float TryGetExplosiveRadius(ThingDef def)
        {
            if (def?.comps == null)
                return 0.0f;
            for (int index = 0; index < def.comps.Count; ++index)
            {
                if (def.comps[index] is CompProperties_Explosive comp)
                    return comp.explosiveRadius;
            }
            return 0.0f;
        }
    }
}
