using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace TheEndTimes_Dwarfs
{
    public class CaravanArrivalAction_CapturedHold : CaravanArrivalAction_EnterMap
    {
        IntVec3 PLAYER_ENTER_LOC = new IntVec3(158, 0, 235);
        // Note: If we had multiple starting locs, we could randomize that for variety.
        //IntVec3 PLAYER_ENTER_LOC = new IntVec3(82, 0, 5);

        public override IntVec3 MapSize
        {
            get
            {
                return new IntVec3(250, 1, 250);
            }
        }

        public CaravanArrivalAction_CapturedHold()
        {
        }

        public CaravanArrivalAction_CapturedHold(MapParent mapParent)
          : base(mapParent)
        {
        }

        public override FloatMenuAcceptanceReport CanVisit(
          Caravan caravan,
          MapParent mapParent)
        {
            if (mapParent == null || !mapParent.Spawned)
                return (FloatMenuAcceptanceReport)false;
            return (FloatMenuAcceptanceReport)true;
        }

        public override Map GetOrGenerateMap(
          int tile,
          IntVec3 mapSize,
          WorldObjectDef suggestedMapParentDef)
        {
            return Current.Game.FindMap(tile) ?? Verse.MapGenerator.GenerateMap(mapSize, this.MapParent, RH_TET_DwarfDefOf.RH_TET_Dwarfs_EmptyMap, (IEnumerable<GenStepWithParams>)null, (Action<Map>)null);
        }

        public override void DoEnter(Caravan caravan)
        {
            Pawn pawn = caravan.PawnsListForReading[0];
            int num = !this.MapParent.HasMap ? 1 : 0;
            Map orGenerateMap = this.GetOrGenerateMap(this.MapParent.Tile, this.MapSize, (WorldObjectDef)null);
            Map map = orGenerateMap;
            IntVec3 enterPos = PLAYER_ENTER_LOC;
            CaravanEnterMapUtility.Enter(caravan, map, (Func<Pawn, IntVec3>)(p => enterPos), CaravanDropInventoryMode.DoNotDrop, true);
            CapturedHoldComp component = this.MapParent.GetComponent<CapturedHoldComp>();
            
            if (!CapturedHoldSite.playerWon)
            { 
                Log.Clear();
                Faction f = RH_TET_DwarfDefOf.GetFinalEnemyFaction();
                this.SetMapSpecificJobs(map.mapPawns.SpawnedPawnsInFaction(f), f, map);
            }
        }

        private void SetMapSpecificJobs(List<Pawn> pawnList, Faction f, Map map)
        {

            List<IntVec3> locs = new List<IntVec3>();
            List<IntVec3> locs2 = new List<IntVec3>();
            List<IntVec3> locs3 = new List<IntVec3>();

            // Entry N&S & GTs
            locs.Add(new IntVec3(106, 0, 137));
            locs.Add(new IntVec3(111, 0, 137));
            locs.Add(new IntVec3(123, 0, 142));
            locs.Add(new IntVec3(123, 0, 148));
            locs.Add(new IntVec3(166, 0, 47));
            locs.Add(new IntVec3(172, 0, 39));
            locs.Add(new IntVec3(158, 0, 152));
            locs.Add(new IntVec3(158, 0, 151));

            // N & S Paths
            locs2.Add(new IntVec3(189, 0, 172));
            locs2.Add(new IntVec3(135, 0, 115));
            locs2.Add(new IntVec3(134, 0, 125));
            locs2.Add(new IntVec3(161, 0, 125));
            locs2.Add(new IntVec3(162, 0, 115));

            // Final
            locs3.Add(new IntVec3(235, 0, 155));
            locs3.Add(new IntVec3(235, 0, 150));
            locs3.Add(new IntVec3(235, 0, 147));
            locs3.Add(new IntVec3(235, 0, 144));
            locs3.Add(new IntVec3(235, 0, 139));

            List<Pawn> turretPawns = new List<Pawn>();
            List<Pawn> turretPawns2 = new List<Pawn>();
            List<Pawn> turretPawns3 = new List<Pawn>();

            foreach (Pawn p in pawnList)
            {
                if (locs.Contains(p.Position))
                {
                    turretPawns.Add(p);
                }
                else if (locs2.Contains(p.Position))
                {
                    turretPawns2.Add(p);
                }
                else if (locs3.Contains(p.Position))
                {
                    turretPawns3.Add(p);
                }
            }
            
            foreach (Pawn pawn in turretPawns)
                pawn.GetLord()?.Notify_PawnLost(pawn, PawnLostCondition.ForcedToJoinOtherLord, new DamageInfo?());
            Lord lord = LordMaker.MakeNewLord(f, (LordJob)new LordJob_ManTurrets(), map, turretPawns);

            foreach (Pawn pawn in turretPawns2)
                pawn.GetLord()?.Notify_PawnLost(pawn, PawnLostCondition.ForcedToJoinOtherLord, new DamageInfo?());
            Lord lord2 = LordMaker.MakeNewLord(f, (LordJob)new LordJob_ManTurrets(), map, turretPawns2);

            foreach (Pawn pawn in turretPawns3)
                pawn.GetLord()?.Notify_PawnLost(pawn, PawnLostCondition.ForcedToJoinOtherLord, new DamageInfo?());
            Lord lord3 = LordMaker.MakeNewLord(f, (LordJob)new LordJob_ManTurrets(), map, turretPawns3);

            foreach (Pawn pawn in turretPawns)
            {
                if (pawn.Spawned)
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
            }
            foreach (Pawn pawn in turretPawns2)
            {
                if (pawn.Spawned)
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
            }
            foreach (Pawn pawn in turretPawns3)
            {
                if (pawn.Spawned)
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
            }
        }
    }
}
