using DoorsExpanded;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace TheEndTimes_Dwarfs
{
    public static class MapGenHandler
    {
        private static FloatRange hitPointsDamage = new FloatRange(.33F, .80F);

        public static void GenerateMap(
          MapGenDef mapGenerator,
          Map map,
          out List<Pawn> spawnedPawns,
          bool damageBuildings = false,
          bool clearMap = false,
          bool setTerrain = false,
          bool fog = true,
          bool unFogRoom = false,
          bool spawnPawns = true,
          bool createRoof = false,
          bool generatePlants = false,
          Faction forceFaction = null)
        {
            spawnedPawns = new List<Pawn>();

            map.regionAndRoomUpdater.Enabled = false;
            if (clearMap)
            {
                Thing.allowDestroyNonDestroyable = true;
                foreach (IntVec3 allCell in map.AllCells)
                {
                    if (allCell.Roofed(map))
                        map.roofGrid.SetRoof(allCell, (RoofDef)null);
                }
                foreach (Thing thing in map.listerThings.AllThings.ToList<Thing>())
                    thing.Destroy(DestroyMode.Vanish);
                Thing.allowDestroyNonDestroyable = false;
            }
            if (generatePlants)
                ((GenStep)Activator.CreateInstance(typeof(GenStep_Plants))).Generate(map, new GenStepParams());
            if (setTerrain)
            {
                MapGenHandler.ClearCells(mapGenerator.MapData, map);
                MapGenHandler.SetTerrain(mapGenerator.MapData, map);
                MapGenHandler.SetUnderTerrain(mapGenerator.UnderTerrainData, map);
            }
            MapGenHandler.PlaceBuildingsAndItems(mapGenerator.MapData, map, forceFaction, damageBuildings);
            if (spawnPawns)
                MapGenHandler.SpawnPawns(mapGenerator.MapData, map, forceFaction, spawnedPawns);
            map.powerNetManager.UpdatePowerNetsAndConnections_First();
            map.regionAndRoomUpdater.Enabled = true;
            map.regionAndRoomUpdater.RebuildAllRegionsAndRooms();
            if (createRoof && mapGenerator.RoofData != null)
                MapGenHandler.CreateRoof(mapGenerator.RoofData, map);
            if (fog)
                MapGenHandler.RefogMap(mapGenerator.defogPosition, map);
            if (!unFogRoom)
                return;
            foreach (IntVec3 allCell in map.AllCells)
            {
                Room room = allCell.GetRoom(map);
                if (room != null && !room.TouchesMapEdge)
                    map.fogGrid.Unfog(allCell);
            }

            if (fog)
                MapGenHandler.RefogMap(mapGenerator.defogPosition, map);
        }

        private static void ClearCells(List<MapObject> mapObjects, Map map)
        {
            foreach (MapObject mapObject in mapObjects)
            {
                foreach (IntVec3 c in mapObject.value)
                {
                    // KEEP. Used to test a new map config. You probably missed a comma in a thing's location in the XML file.
                    //if (c.y == -1000)
                    //{
                    //    if (mapObject.key.Kind != null)
                    //        Log.Error("Err MapObject:" + mapObject.key.Kind);
                    //    else if (mapObject.key.Thing != null)
                    //        Log.Error("Err MapObject:" + mapObject.key.Thing);
                    //    else if (mapObject.key.Terrain != null)
                    //        Log.Error("Err MapObject:" + mapObject.key.Terrain);
                    //    else
                    //        Log.Error("Err AllNull:");
                    //}

                    List<Thing> thingList = map.thingGrid.ThingsListAt(c);
                    if (thingList != null)
                    {
                        for (int index = 0; index < thingList.Count; ++index)
                        {
                            if (thingList[index].def.destroyable)
                                thingList[index].Destroy(DestroyMode.Vanish);
                        }
                    }
                }
            }
        }

        private static void AddRoomCentersToRootsToUnfog(List<Room> allRooms)
        {
            if (Current.ProgramState != ProgramState.MapInitializing)
                return;
            List<IntVec3> rootsToUnfog = Verse.MapGenerator.rootsToUnfog;
            for (int index = 0; index < allRooms.Count; ++index)
                rootsToUnfog.Add(allRooms[index].Cells.RandomElement<IntVec3>());
        }

        public static void RefogMap(IntVec3 defogPosition, Map map)
        {
            CellIndices cellIndices = map.cellIndices;
            if (map.fogGrid == null)
                map.fogGrid.fogGrid = new bool[cellIndices.NumGridCells];
            foreach (IntVec3 allCell in map.AllCells)
                map.fogGrid.fogGrid[cellIndices.CellToIndex(allCell)] = true;
            if (Current.ProgramState == ProgramState.Playing)
                map.roofGrid.Drawer.SetDirty();
            foreach (IntVec3 allCell in map.AllCells)
                map.mapDrawer.MapMeshDirty(allCell, MapMeshFlag.FogOfWar);
            if (defogPosition == null)
                FloodFillerFog.FloodUnfog(CellFinder.RandomEdgeCell(map), map);
            else
                FloodFillerFog.FloodUnfog(defogPosition, map);
        }

        private static void AddRoomsToFog(List<Room> allRooms, Map map, bool fogDoors = false)
        {
            CellIndices cellIndices = map.cellIndices;
            foreach (Room allRoom in allRooms)
            {
                foreach (IntVec3 cell in allRoom.Cells)
                {
                    if (cell.GetDoor(map) == null || fogDoors)
                    {
                        map.fogGrid.fogGrid[cellIndices.CellToIndex(cell)] = true;
                        map.mapDrawer.MapMeshDirty(cell, MapMeshFlag.FogOfWar);
                    }
                }
            }
        }

        public static void CreateRoof(List<RoofObject> roofData, Map map)
        {
            foreach (RoofObject roofObject in roofData)
            {
                foreach (IntVec3 position in roofObject.Positions)
                    map.roofGrid.SetRoof(position, roofObject.RoofDef);
            }
        }

        public static void SpawnPawns(
          List<MapObject> mapObjects,
          Map map,
          Faction forceFaction,
          List<Pawn> spawnedPawns)
        {
            foreach (MapObject mapObject in mapObjects)
            {
                ThingData key = mapObject.key;
                if (key.Kind != null)
                {
                    Faction faction = forceFaction ?? Find.FactionManager.FirstFactionOfDef(key.Faction);
                    if (faction != null)
                    {
                        foreach (IntVec3 loc in mapObject.value)
                        {
                            Pawn pawn = PawnGenerator.GeneratePawn(key.Kind, faction);
                            if (pawn.RaceProps.Animal)// && key.Faction == null
                                pawn.SetFaction((Faction)null, (Pawn)null);
                            if (pawn.RaceProps.FleshType == FleshTypeDefOf.Insectoid)
                                pawn.SetFaction(Faction.OfInsects, (Pawn)null);
                            if (pawn.RaceProps.IsMechanoid)
                                pawn.SetFaction(Faction.OfMechanoids, (Pawn)null);
                            Pawn p = GenSpawn.Spawn((Thing)pawn, loc, map, WipeMode.Vanish) as Pawn;
                            spawnedPawns.Add(p);
                            LordJob_DefendPoint lordJobDefendPoint = new LordJob_DefendPoint(p.Position);
                            Lord lord = LordMaker.MakeNewLord(faction, (LordJob)lordJobDefendPoint, map, (IEnumerable<Pawn>)null);
                            lord.numPawnsLostViolently = int.MaxValue;
                            lord.AddPawn(p);
                        }
                    }
                    else
                    {
                        foreach (IntVec3 loc in mapObject.value)
                            GenSpawn.Spawn((Thing)PawnGenerator.GeneratePawn(key.Kind, faction), loc, map, WipeMode.Vanish);
                    }
                }
            }
        }

        public static void PlaceBuildingsAndItems(
          List<MapObject> mapObjects,
          Map map,
          Faction forceFaction,
          bool damageBuildings)
        {
            foreach (MapObject mapObject in mapObjects)
            {
                try
                {
                    ThingData key = mapObject.key;
                    if (key.Thing != null)
                    {
                        foreach (IntVec3 loc in mapObject.value)
                        {
                            Thing thing1 = ThingMaker.MakeThing(key.Thing, key.Stuff);
                            thing1.TryGetComp<CompQuality>()?.SetQuality(key.Quality, ArtGenerationContext.Colony);
                            if (thing1.def.stackLimit != 1)
                                thing1.stackCount = key.Count;
                            Thing thing2 = GenSpawn.Spawn(thing1, loc, map, key.Rotate, WipeMode.Vanish, false);
                            CompGatherSpot comp1 = thing2.TryGetComp<CompGatherSpot>();
                            if (comp1 != null)
                                comp1.Active = false;
                            if (thing2.def != ThingDefOf.Wall && thing2.def.CanHaveFaction && forceFaction != null)
                            {
                                thing2.SetFaction(forceFaction, (Pawn)null);
                            }
                            CompRefuelable comp2 = thing2.TryGetComp<CompRefuelable>();
                            comp2?.Refuel(comp2.Props.fuelCapacity);

                            if (damageBuildings && thing2.def.IsBuildingArtificial)
                            {
                                ReduceHitPoints(thing2);
                            }
                        }
                    }
                }
                catch
                {
                    //Log.Error("ERRORHAPPENED:" + mapObject.ToString());
                    //Log.Error("ERRORHAPPENED=:" + mapObject.key.ToString());
                }
            }
        }

        private static void ReduceHitPoints(Thing thing)
        {
            thing.HitPoints = (int)(thing.MaxHitPoints * hitPointsDamage.RandomInRange);
        }

        public static void SetTerrain(List<MapObject> mapObjects, Map map)
        {
            foreach (MapObject mapObject in mapObjects)
            {
                ThingData key = mapObject.key;
                if (key.Terrain != null)
                {
                    foreach (IntVec3 c in mapObject.value)
                        map.terrainGrid.SetTerrain(c, key.Terrain);
                }
            }
        }

        public static void SetUnderTerrain(List<MapObject> mapObjects, Map map)
        {
            foreach (MapObject mapObject in mapObjects)
            {
                ThingData key = mapObject.key;
                if (key.Terrain != null)
                {
                    foreach (IntVec3 c in mapObject.value)
                        map.terrainGrid.SetUnderTerrain(c, key.Terrain);
                }
            }
        }
    }
}
