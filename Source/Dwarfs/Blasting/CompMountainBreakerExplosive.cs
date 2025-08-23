using HugsLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.Sound;

namespace TheEndTimes_Dwarfs
{
    public class CompMountainBreakerExplosive : CompBlastingExplosive
    {
        private readonly IntRange CollapseDelay = new IntRange(0, 120);
        private const int RoofFilthAmount = 3;

        protected override void Detonate()
        {
            Map map = this.parentMap;
            IntVec3 parentPosition = this.parentPosition;
            base.Detonate();
            if (map == null)
                return;
            CompProperties_Explosive props = this.props as CompProperties_Explosive;
            if (props == null)
                return;
            bool flag1 = BlastingUtility.IsEffectiveRoofBreakerPlacement(props.explosiveRadius, parentPosition, map);
            bool flag2 = false;
            foreach (IntVec3 c in GenRadial.RadialCellsAround(parentPosition, props.explosiveRadius, true))
            {
                if (c.InBounds(map))
                {
                    RoofDef roofDef = map.roofGrid.RoofAt(c);

                    if (roofDef != null && (!roofDef.isThickRoof || flag1))
                    {
                        if (roofDef.filthLeaving != null)
                        {
                            for (int index = 0; index < 3; ++index)
                                FilthMaker.TryMakeFilth(c, map, roofDef.filthLeaving, 1);
                        }
                        if (roofDef.isThickRoof)
                        {
                            flag2 = true;
                            IntVec3 roofCell = c;
                            HugsLibController.Instance.TickDelayScheduler.ScheduleCallback((Action)(() =>
                            {
                                this.CollapseRockOnCell(roofCell, map);
                                SoundDefOf.Roof_Collapse.PlayOneShot((SoundInfo)new TargetInfo(roofCell, map, false));
                            }), this.CollapseDelay.RandomInRange, (Verse.Thing)null, false);
                        }
                        map.roofGrid.SetRoof(c, (RoofDef)null);
                    }
                }
            }
            if (!flag2)
                return;
            RH_TET_DwarfDefOf.RH_TET_Dwarfs_MountainDrop.PlayOneShot((SoundInfo)new TargetInfo(parentPosition, map, false));
        }

        private void CollapseRockOnCell(IntVec3 cell, Map map)
        {
            this.CrushThingsUnderCollapsingRock(cell, map);
            Verse.Thing thing = GenSpawn.Spawn(RH_TET_DwarfDefOf.RH_TET_Dwarfs_CollapsedMountainRocks, cell, map, WipeMode.Vanish);
            if (!thing.def.rotatable)
                return;
            thing.Rotation = Rot4.Random;
        }

        private void CrushThingsUnderCollapsingRock(IntVec3 cell, Map map)
        {
            for (int index1 = 0; index1 < 2; ++index1)
            {
                List<Verse.Thing> thingList = cell.GetThingList(map);
                for (int index2 = thingList.Count - 1; index2 >= 0; --index2)
                {
                    Verse.Thing thing = thingList[index2];
                    Pawn pawn = thing as Pawn;
                    DamageInfo dinfo;
                    if (pawn != null)
                    {
                        BodyPartRecord brain = pawn.health.hediffSet.GetBrain();
                        dinfo = new DamageInfo(DamageDefOf.Crush, 99999f, 1f, -1f, (Verse.Thing)null, brain, (ThingDef)null, DamageInfo.SourceCategory.ThingOrUnknown, (Verse.Thing)null);
                    }
                    else
                    {
                        dinfo = new DamageInfo(DamageDefOf.Crush, 99999f, 0.0f, -1f, (Verse.Thing)null, (BodyPartRecord)null, (ThingDef)null, DamageInfo.SourceCategory.ThingOrUnknown, (Verse.Thing)null);
                        dinfo.SetBodyRegion(BodyPartHeight.Top, BodyPartDepth.Outside);
                    }
                    thing.TakeDamage(dinfo);
                    if (!thing.Destroyed && thing.def.destroyable)
                        thing.Destroy(DestroyMode.Vanish);
                }
            }
        }
    }
}
