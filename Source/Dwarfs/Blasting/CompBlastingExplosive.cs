using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TheEndTimes_Dwarfs
{
    public class CompBlastingExplosive : CompCustomBlast
    {
        private const int MinAffectedCellsToTriggerCaveInSound = 6;
        private List<IntVec3> customArea;

        public CompProperties_BlastingExplosive MiningProps
        {
            get
            {
                return (CompProperties_BlastingExplosive)this.props;
            }
        }

        public void AssignCustomMiningArea(List<IntVec3> cells)
        {
            this.customArea = cells;
        }

        protected override void Detonate()
        {
            base.Detonate();
            if (this.parentMap == null)
                return;
            IOrderedEnumerable<IntVec3> orderedEnumerable = (this.customArea ?? GenRadial.RadialCellsAround(this.parentPosition, Mathf.Clamp(Mathf.Round(this.MiningProps.miningRadius), 0.0f, 25f), true).ToList<IntVec3>()).OrderBy<IntVec3, float>((Func<IntVec3, float>)(c =>
            {
                IntVec3 intVec3 = c - this.parentPosition;
                return Mathf.Pow((float)intVec3.x, 2f) + Mathf.Pow((float)intVec3.z, 2f);
            }));
            int num = 0;
            float breakingPower = this.MiningProps.breakingPower;
            foreach (IntVec3 c in (IEnumerable<IntVec3>)orderedEnumerable)
            {
                foreach (Verse.Thing thing in this.parentMap.thingGrid.ThingsListAt(c).ToArray())
                {
                    if (this.TryAffectThing(thing, (Verse.Thing)this.parent, ref breakingPower))
                        ++num;
                }
                if ((double)breakingPower <= 0.0)
                    break;
            }
            if (num < 6)
                return;
            RH_TET_DwarfDefOf.RH_TET_Dwarfs_MountainDrop.PlayOneShot((SoundInfo)new TargetInfo(this.parentPosition, this.parentMap, false));
        }

        private bool TryAffectThing(Verse.Thing thing, Verse.Thing explosive, ref float breakingPowerRemaining)
        {
            if (thing.def == null || thing.Map == null)
                return false;
            Map map = thing.Map;
            bool flag = false;
            if (thing.def.mineable)
            {
                BuildingProperties building = thing.def.building;
                if (building == null)
                    return false;
                Mineable mineable = thing as Mineable;
                if (building.isResourceRock && mineable != null)
                {
                    breakingPowerRemaining -= (float)thing.HitPoints * this.MiningProps.resourceBreakingCost;
                    this.BreakMineableAndYieldResources(mineable, explosive);
                    flag = true;
                }
                else if (building.isNaturalRock)
                {
                    breakingPowerRemaining -= (float)thing.HitPoints;
                    thing.Destroy(DestroyMode.Vanish);
                    flag = true;
                    if (thing.def.filthLeaving != null)
                        FilthMaker.TryMakeFilth(thing.Position, map, thing.def.filthLeaving, Rand.RangeInclusive(1, 3));                    
                    if (building.mineableThing != null && (double)Rand.Value < (double)explosive.GetStatValue(RH_TET_DwarfDefOf.RH_TET_Dwarfs_RockYield, true))
                    {
                        Verse.Thing thing1 = ThingMaker.MakeThing(building.mineableThing, (ThingDef)null);
                        thing1.stackCount = thing1.def.stackLimit != 1 ? Mathf.CeilToInt((float)building.mineableYield) : 1;
                        GenPlace.TryPlaceThing(thing1, thing.Position, map, ThingPlaceMode.Direct, (Action<Verse.Thing, int>)null, (Predicate<IntVec3>)null);
                    }
                }
                else
                {
                    breakingPowerRemaining -= (float)thing.HitPoints;
                    thing.Destroy(DestroyMode.KillFinalize);
                    flag = true;
                }
            }
            else if (thing.def.plant != null && thing.def.plant.IsTree)
            {
                breakingPowerRemaining -= (float)thing.HitPoints * this.MiningProps.woodBreakingCost;
                Plant plant = (Plant)thing;
                int num = plant.YieldNow();
                // TODO JEH 1.3 - This line was removed because there's no way to get the pawn that kicked off the explosion. Maybe patch?
                //plant.PlantCollected();
                if (num > 0)
                {
                    Verse.Thing thing1 = ThingMaker.MakeThing(thing.def.plant.harvestedThingDef, (ThingDef)null);
                    
                    float? yieldFactor = explosive.GetStatValue(RH_TET_DwarfDefOf.RH_TET_Dwarfs_WoodYield, true);
                    if (yieldFactor != null)
                        num = (int)(num * yieldFactor);

                    thing1.stackCount = num;
                    GenPlace.TryPlaceThing(thing1, thing.Position, map, ThingPlaceMode.Direct, (Action<Verse.Thing, int>)null, (Predicate<IntVec3>)null);
                }
            }
            return flag;
        }

        private void BreakMineableAndYieldResources(Mineable mineable, Verse.Thing explosive)
        {

            if (mineable?.def?.building?.mineableThing == null)
                return;
            if (mineable == null || mineable.Destroyed)
            {
                return;
            }
            IntVec3 position = mineable.Position;
            Map map = mineable.Map;
            int hitPoints = mineable.HitPoints;
            Verse.Thing thing = map.thingGrid.ThingAt(position, mineable.def.building.mineableThing);
            mineable.Destroy(DestroyMode.KillFinalize);
            if (thing != null && !thing.Destroyed)
                thing.Destroy(DestroyMode.Vanish);
            if ((double)Rand.Value > (double)mineable.def.building.mineableDropChance)
                return;
            int num1 = mineable.def.building.mineableYield;
            float num2 = (float)hitPoints / (float)mineable.MaxHitPoints;
            if (mineable.def.building.mineableYieldWasteable)
                num1 = Mathf.Max(1, GenMath.RoundRandom((float)num1 * num2));

            float? yieldFactor = explosive.GetStatValue(RH_TET_DwarfDefOf.RH_TET_Dwarfs_ResourceYield, true);
            if (yieldFactor != null)
                num1 = (int)(num1 * yieldFactor);

            Verse.Thing newThing = ThingMaker.MakeThing(mineable.def.building.mineableThing, (ThingDef)null);
            newThing.stackCount = num1;
            GenSpawn.Spawn(newThing, position, map, WipeMode.Vanish);
        }
    }
}
