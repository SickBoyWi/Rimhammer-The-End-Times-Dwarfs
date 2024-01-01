using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class CompSpawnerTunnels : ThingComp
    {
        private int nextTunnelSpawnTick = -1;
        public bool canSpawnTunnels = true;
        private bool wasActivated;
        public const int MaxTunnelsPerMap = 35;

        private CompProperties_SpawnerTunnels Props
        {
            get
            {
                return (CompProperties_SpawnerTunnels)this.props;
            }
        }

        private bool CanSpawnChildTunnel
        {
            get
            {
                if (this.canSpawnTunnels)
                    return TunnelUtility.TotalSpawnedTunnelsCount(this.parent.Map) < 30;
                return false;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (respawningAfterLoad)
                return;
            this.CalculateNextTunnelSpawnTick();
        }

        public override void CompTick()
        {
            base.CompTick();
            CompCanBeDormant comp = this.parent.GetComp<CompCanBeDormant>();
            if ((comp == null ? 1 : (comp.Awake ? 1 : 0)) != 0 && !this.wasActivated)
            {
                this.CalculateNextTunnelSpawnTick();
                this.wasActivated = true;
            }
            if (comp != null && !comp.Awake || Find.TickManager.TicksGame < this.nextTunnelSpawnTick)
                return;
            SkavenTunnel newTunnel;
            if (this.TrySpawnChildTunnel(false, out newTunnel))
                Messages.Message((string)"RH_TET_Dwarfs_SkavenTunnelNew".Translate(), (LookTargets)((Thing)newTunnel), MessageTypeDefOf.NegativeEvent, true);
            else
                this.CalculateNextTunnelSpawnTick();
        }

        public override string CompInspectStringExtra()
        {
            if (!this.canSpawnTunnels)
                return (string)"RH_TET_Dwarfs_SkavenTunnel_Inactive".Translate();
            if (this.CanSpawnChildTunnel)
                return (string)("RH_TET_Dwarfs_SkavenTunnel_NewIn".Translate() + ": " + (this.nextTunnelSpawnTick - Find.TickManager.TicksGame).ToStringTicksToPeriod(true, false, true, true));
            return (string)null;
        }

        public void CalculateNextTunnelSpawnTick()
        {
            Room room = this.parent.GetRoom(RegionType.Set_Passable);
            int num1 = 0;
            int num2 = GenRadial.NumCellsInRadius(9f);
            for (int index = 0; index < num2; ++index)
            {
                IntVec3 intVec3 = this.parent.Position + GenRadial.RadialPattern[index];
                if (intVec3.InBounds(this.parent.Map) && intVec3.GetRoom(this.parent.Map) == room && intVec3.GetThingList(this.parent.Map).Any<Thing>((Predicate<Thing>)(t => t is SkavenTunnel)))
                    ++num1;
            }
            this.nextTunnelSpawnTick = Find.TickManager.TicksGame + (int)((double)this.Props.TunnelSpawnIntervalDays.RandomInRange * 60000.0 / ((double)this.Props.ReproduceRateFactorFromNearbyTunnelCountCurve.Evaluate((float)num1) * (double)Find.Storyteller.difficulty.enemyReproductionRateFactor));
        }

        public bool TrySpawnChildTunnel(bool ignoreRoofedRequirement, out SkavenTunnel newTunnel)
        {
            if (!this.CanSpawnChildTunnel)
            {
                newTunnel = (SkavenTunnel)null;
                return false;
            }
            IntVec3 childTunnelLocation = CompSpawnerTunnels.FindChildTunnelLocation(this.parent.Position, this.parent.Map, this.parent.def, this.Props, ignoreRoofedRequirement, false);
            if (!childTunnelLocation.IsValid)
            {
                newTunnel = (SkavenTunnel)null;
                return false;
            }
            newTunnel = (SkavenTunnel)ThingMaker.MakeThing(this.parent.def, (ThingDef)null);
            if (newTunnel.Faction != this.parent.Faction)
                newTunnel.SetFaction(this.parent.Faction, (Pawn)null);
            SkavenTunnel parent = this.parent as SkavenTunnel;
            if (parent != null)
            {
                if (parent.CompDormant.Awake)
                    newTunnel.CompDormant.WakeUp();
                newTunnel.questTags = parent.questTags;
            }
            GenSpawn.Spawn((Thing)newTunnel, childTunnelLocation, this.parent.Map, WipeMode.FullRefund);
            this.CalculateNextTunnelSpawnTick();
            return true;
        }

        public static IntVec3 FindChildTunnelLocation(
          IntVec3 pos,
          Map map,
          ThingDef parentDef,
          CompProperties_SpawnerTunnels props,
          bool ignoreRoofedRequirement,
          bool allowUnreachable)
        {
            IntVec3 result = IntVec3.Invalid;
            for (int index = 0; index < 3; ++index)
            {
                float minDist = props.TunnelSpawnPreferredMinDist;
                bool flag;
                if (index < 2)
                {
                    if (index == 1)
                        minDist = 0.0f;
                    flag = CellFinder.TryFindRandomReachableCellNear(pos, map, props.TunnelSpawnRadius, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), (Predicate<IntVec3>)(c => CompSpawnerTunnels.CanSpawnTunnelAt(c, map, pos, parentDef, minDist, ignoreRoofedRequirement)), (Predicate<Region>)null, out result, 999999);
                }
                else
                    flag = allowUnreachable && CellFinder.TryFindRandomCellNear(pos, map, (int)props.TunnelSpawnRadius, (Predicate<IntVec3>)(c => CompSpawnerTunnels.CanSpawnTunnelAt(c, map, pos, parentDef, minDist, ignoreRoofedRequirement)), out result, -1);
                if (flag)
                {
                    result = CellFinder.FindNoWipeSpawnLocNear(result, map, parentDef, Rot4.North, 2, (Predicate<IntVec3>)(c => CompSpawnerTunnels.CanSpawnTunnelAt(c, map, pos, parentDef, minDist, ignoreRoofedRequirement)));
                    break;
                }
            }
            return result;
        }

        private static bool CanSpawnTunnelAt(
          IntVec3 c,
          Map map,
          IntVec3 parentPos,
          ThingDef parentDef,
          float minDist,
          bool ignoreRoofedRequirement)
        {
            if (!ignoreRoofedRequirement && !c.Roofed(map) || !c.Walkable(map) || ((double)minDist != 0.0 && (double)c.DistanceToSquared(parentPos) < (double)minDist * (double)minDist || (c.GetFirstThing(map, ThingDefOf.InsectJelly) != null || c.GetFirstThing(map, ThingDefOf.GlowPod) != null)))
                return false;
            for (int index1 = 0; index1 < 9; ++index1)
            {
                IntVec3 c1 = c + GenAdj.AdjacentCellsAndInside[index1];
                if (c1.InBounds(map))
                {
                    List<Thing> thingList = c1.GetThingList(map);
                    for (int index2 = 0; index2 < thingList.Count; ++index2)
                    {
                        if (thingList[index2] is SkavenTunnel || thingList[index2] is SkavenTunnelSpawner)
                            return false;
                    }
                }
            }
            List<Thing> thingList1 = c.GetThingList(map);
            for (int index = 0; index < thingList1.Count; ++index)
            {
                Thing thing = thingList1[index];
                if ((thing.def.category != ThingCategory.Building ? 0 : (thing.def.passability == Traversability.Impassable ? 1 : 0)) != 0 && GenSpawn.SpawningWipes((BuildableDef)parentDef, (BuildableDef)thing.def))
                    return true;
            }
            return true;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Prefs.DevMode)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "Dev: Expand";
                command_Action.icon = TexCommand.GatherSpotActive;
                SkavenTunnel newTunnel;
                command_Action.action = (Action)(() => this.TrySpawnChildTunnel(false, out newTunnel));
                yield return (Gizmo)command_Action;
                command_Action = (Command_Action)null;
            }
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look<int>(ref this.nextTunnelSpawnTick, "nextTunnelSpawnTick", 0, false);
            Scribe_Values.Look<bool>(ref this.canSpawnTunnels, "canSpawnTunnels", true, false);
            Scribe_Values.Look<bool>(ref this.wasActivated, "wasTunnelActivated", true, false);
        }
    }
}
