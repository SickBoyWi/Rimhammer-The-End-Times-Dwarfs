using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace TheEndTimes_Dwarfs
{
    internal interface ITerrainOverride
    {
        int HighZ { get; }
        int LowZ { get; }
        int HighX { get; }
        int LowX { get; }

        bool IsInside(IntVec3 i, int rand = 0);
    }

    internal class TerrainOverrideRound : ITerrainOverride
    {
        public readonly IntVec3 Center;
        public readonly int Radius;

        public TerrainOverrideRound(IntVec3 center, int radius)
        {
            this.Center = center;
            this.Radius = radius;
        }

        public bool IsInside(IntVec3 i, int rand = 0)
        {
            if (i.x <= this.Center.x - Radius || i.x >= this.Center.x + Radius ||
                i.z <= this.Center.z - Radius || i.z >= this.Center.z + Radius)
            {
                return false;
            }

            int x = i.x - this.Center.x;
            int z = i.z - this.Center.z;
            return Math.Pow(x, 2) + Math.Pow(z, 2) < Math.Pow(this.Radius, 2);
        }

        public int LowX { get { return this.Center.x - this.Radius; } }
        public int HighX { get { return this.Center.x + this.Radius; } }
        public int LowZ { get { return this.Center.z - this.Radius; } }
        public int HighZ { get { return this.Center.z + this.Radius; } }
    }

    internal class TerrainOverrideSquare : ITerrainOverride
    {
        public readonly IntVec3 Low, High;
        public TerrainOverrideSquare(int lowX, int lowZ, int highX, int highZ)
        {
            this.Low = new IntVec3(lowX, 0, lowZ);
            this.High = new IntVec3(highX, 0, highZ);
        }

        public bool IsInside(IntVec3 i, int rand = 0)
        {
            return (
                i.x >= this.Low.x + rand && i.x <= this.High.x + rand &&
                i.z >= this.Low.z + rand && i.z <= this.High.z + rand);
        }

        public int LowX { get { return this.Low.x; } }
        public int HighX { get { return this.High.x; } }
        public int LowZ { get { return this.Low.z; } }
        public int HighZ { get { return this.High.z; } }
    }

    [HarmonyPatch(typeof(GenStep_ElevationFertility), "Generate")]
    static class Patch_GenStep_ElevationFertility
    {
        // These are the minimum settings for this stuff to work correct.
        public static int DEEPEST_CLEARING_SIZE_DEFAULT = 17;
        public static int SHALLOWEST_CLEARING_SIZE_DEFAULT = 5;
        // These are the maximum settings for this stuff to work correct..
        public static int DEEPEST_CLEARING_SIZE_MAX = 128;
        public static int SHALLOWEST_CLEARING_SIZE_MAX = 116;
        public static int DEEPEST_CLEARING_SIZE = DEEPEST_CLEARING_SIZE_DEFAULT;
        public static int SHALLOWEST_CLEARING_SIZE = SHALLOWEST_CLEARING_SIZE_DEFAULT;
        private static float SAVED_VANILLA_MOUNTAINOUS_ELEVATION_FACTOR = 0f;

        private static bool debug_WarnedMissingTerrain;

        static void Prefix(Map map)
        {
            if (Settings.UseVanillaMap)
            {
                SAVED_VANILLA_MOUNTAINOUS_ELEVATION_FACTOR = MapGenTuning.ElevationFactorImpassableMountains;
                MapGenTuning.ElevationFactorImpassableMountains = Settings.VANILLA_MAP_STONE_DENSITY_BASE + (Settings.VanillaMapStoneDensithy * .1f);
            }
        }

        //static void Postfix(IEnumerable<GenStepWithParams> genStepDefs, Map map, int seed)
        static void Postfix(Map map)
        {
            if (Settings.UseVanillaMap)
            {
                MapGenTuning.ElevationFactorImpassableMountains = SAVED_VANILLA_MOUNTAINOUS_ELEVATION_FACTOR;
            }
            else
            {
                if (map.TileInfo.hilliness == Hilliness.Impassable)
                {
                    int radius = (int)(((float)map.Size.x + map.Size.z) * 0.25f) + Settings.OuterRadius;

                    int middleWallSmoothness = Settings.MiddleWallSmoothness;

                    //if (Settings.TrueRandom)
                    //{ 
                    //    r = new Random(Guid.NewGuid().GetHashCode());
                    //    trueRandom = new Random(Guid.NewGuid().GetHashCode());
                    //}
                    //else
                    //{ 
                    //    r = new Random((Find.World.info.name + map.Tile).GetHashCode());
                    //    trueRandom = new Random(Guid.NewGuid().GetHashCode());
                    //}

                    // Not happy with round, needs more noise around edges. 
                    //Settings.OpenAreaShape = GetShape(trueRandom.Next(0, 2));

                    NSEW nsew = GetNSEW(RH_TET_DwarfsMod.random.Next(0, 5));
                    MountainShape mountainShape = GetMountainShape(RH_TET_DwarfsMod.random.Next(0, 2));

                    // If there's an ocean neighbor, set us to convex.
                    bool isCoastalSideMatch = false;
                    Rot4 rot = Find.World.CoastDirectionAt(map.Tile);
                    isCoastalSideMatch = DetermineCoastalSideMatchesPawnStartSide(rot, nsew);
                    NSEW deepOpeningSide = NSEW.South;

                    // Beach only works well with convex mountain shape when pawns start on that side.
                    if (isCoastalSideMatch)
                    {
                        mountainShape = MountainShape.Convex;
                        deepOpeningSide = DetermineWhichSideToMakeDeep(rot);
                    }

                    int deepestClearingSize = RH_TET_DwarfsMod.random.Next(DEEPEST_CLEARING_SIZE - 5, DEEPEST_CLEARING_SIZE + 5);
                    int shallowestClearingSize = RH_TET_DwarfsMod.random.Next(SHALLOWEST_CLEARING_SIZE - 2, SHALLOWEST_CLEARING_SIZE + 2);

                    ITerrainOverride middleArea = null;
                    if (Settings.HasMiddleArea)
                    {
                        middleArea = GenerateMiddleArea(map);
                    }

                    MapGenFloatGrid fertility = Verse.MapGenerator.Fertility;
                    MapGenFloatGrid elevation = Verse.MapGenerator.Elevation;
                    TerrainGrid terrainGrid = map.terrainGrid;

                    // Save the ocean as generated for vanilla map.
                    //BeachMaker.Init(map);
                    WorldGenStep_Mutators.AddMutatorsFromTile(Find.WorldGrid.Surface);

                    foreach (IntVec3 current in map.AllCells)
                    {
                        float elev = 0;
                        int useClearingSize = deepestClearingSize;

                        int cellsPerZoneX = map.Size.x / Settings.ZoneCount;
                        int cellsPerZoneZ = map.Size.z / Settings.ZoneCount;

                        int currentZone = -1;

                        if (nsew == NSEW.North || nsew == NSEW.South)
                            currentZone = (current.x / cellsPerZoneX) + 1;
                        else
                            currentZone = (current.z / cellsPerZoneZ) + 1;

                        if (currentZone > (Settings.ZoneCount / 2))
                            currentZone = Settings.ZoneCount - currentZone + 1;

                        bool currentTileNSEWMatchesStartNSEW = GetCurrentNSEWMatchesStartNSEW(nsew, current, map);
                        TerrainDef terrainDef = TerrainFrom(current, map, elevation[current], fertility[current], false);

                        if (currentTileNSEWMatchesStartNSEW && isCoastalSideMatch)
                        {
                            if (GetCurrentNSEWMatchesStartNSEW(deepOpeningSide, current, map))
                                useClearingSize = 50;
                        }

                        // Keep deep ocean tiles in place for flavor.
                        if (currentTileNSEWMatchesStartNSEW || terrainDef != TerrainDefOf.WaterOceanDeep)
                        {
                            // Keep half the shallow water tiles.
                            int shallowDetermine = RH_TET_DwarfsMod.random.Next(0, 2);

                            if (currentTileNSEWMatchesStartNSEW || terrainDef != TerrainDefOf.WaterOceanDeep || shallowDetermine == 0)
                            {
                                if (mountainShape == MountainShape.Concave && IsMountainConcave(current, map, nsew, useClearingSize, currentZone))
                                {
                                    // SEE: MapGenTuning
                                    // Overhead mountain: elevation > 1.14f(?)
                                    elev = 3.40282347E+38f;
                                }
                                else if (mountainShape == MountainShape.Convex && IsMountainConvex(current, map, nsew, useClearingSize, shallowestClearingSize, currentZone))
                                {
                                    // Overhead mountain: elevation > 1.14f(?)
                                    elev = 3.40282347E+38f;
                                }
                                // Rock, rock roof:  elevation > 1.0f(?)
                                else if (Settings.ScatteredRocks && IsScatteredRock(current, map, radius))
                                {
                                    // Mineable stone, no overhead mountain: elevation >= 0.61f && elevation < ???
                                    elev = 0.75f;
                                }
                                else
                                {
                                    // Gravel: elevation > 0.55f && elevation < 0.61f
                                    elev = 0.57f;
                                }
                                // Normal ground: elevation > 0.0f && (?)elevation <= 0.55f
                                // Sand covered: elevation < 1.0f??
                                // Water covered: elevation < 0.0f
                                // Shallow water: elevation < 0.45f
                                // Deep water: elevation < 0.1f

                                if (middleArea != null)
                                {
                                    int i = (middleWallSmoothness == 0) ? 0 : RH_TET_DwarfsMod.random.Next(middleWallSmoothness);
                                    if (middleArea.IsInside(current, i))
                                    {
                                        // Normal ground.
                                        elev = 0;
                                    }
                                }

                                elevation[current] = elev;
                            }
                        }
                    }
                }
            }
        }

        private static NSEW DetermineWhichSideToMakeDeep(Rot4 rot)
        {
            NSEW returnVal = NSEW.North;

            if (rot.IsValid)
            {
                if (rot == Rot4.East || rot == Rot4.West)
                {
                    if (RH_TET_DwarfsMod.random.Next(0, 2) == 1)
                        returnVal = NSEW.North;
                    else
                        returnVal = NSEW.South;
                }
                else if (rot == Rot4.North || rot == Rot4.South)
                {
                    if (RH_TET_DwarfsMod.random.Next(0, 2) == 1)
                        returnVal = NSEW.East;
                    else
                        returnVal = NSEW.West;
                }
            }

            return returnVal;
        }

        private static bool DetermineCoastalSideMatchesPawnStartSide(Rot4 rot, NSEW nsew)
        {
            bool returnVal = false;

            if (rot.IsValid)
            {
                if (rot == Rot4.East && nsew == NSEW.East)
                    returnVal = true;
                else if (rot == Rot4.North && nsew == NSEW.North)
                    returnVal = true;
                else if (rot == Rot4.West && nsew == NSEW.West)
                    returnVal = true;
                else if (rot == Rot4.South && nsew == NSEW.South)
                    returnVal = true;
            }

            return returnVal;
        }

        // Copy from core GenStep_Terrain
        public static TerrainDef TerrainFrom(
          IntVec3 c,
          Map map,
          float elevation,
          float fertility,
          bool preferRock)
        {
            TerrainDef terrainDef = (TerrainDef)null;
            BiomeDef biomeDef = map.BiomeAt(c);
            bool flag = map.TileInfo.Mutators.Any<TileMutatorDef>((Func<TileMutatorDef, bool>)(m => m.preventsPondGeneration));
            if (!map.TileInfo.Mutators.Any<TileMutatorDef>((Func<TileMutatorDef, bool>)(m => m.preventPatches)))
            {
                foreach (TerrainPatchMaker terrainPatchMaker in biomeDef.terrainPatchMakers)
                {
                    if (!flag || !terrainPatchMaker.isPond)
                    {
                        terrainDef = terrainPatchMaker.TerrainAt(c, map, fertility);
                        if (terrainDef != null)
                            break;
                    }
                }
            }
            if (terrainDef == null)
            {
                if ((double)elevation > 0.550000011920929 && (double)elevation < 0.610000014305115 && !biomeDef.noGravel)
                    terrainDef = biomeDef.gravelTerrain ?? TerrainDefOf.Gravel;
                else if ((double)elevation >= 0.610000014305115)
                    terrainDef = GenStep_RocksFromGrid.RockDefAt(c).building.naturalTerrain;
            }
            if (terrainDef == null)
                terrainDef = TerrainThreshold.TerrainAtValue(biomeDef.terrainsByFertility, fertility);
            if (terrainDef == null)
            {
                if (!Patch_GenStep_ElevationFertility.debug_WarnedMissingTerrain)
                {
                    Log.Error("No terrain found in biome " + biomeDef.defName + " for elevation=" + elevation.ToString() + ", fertility=" + fertility.ToString());
                    Patch_GenStep_ElevationFertility.debug_WarnedMissingTerrain = true;
                }
                terrainDef = TerrainDefOf.Sand;
            }
            if (preferRock && terrainDef.supportsRock)
                terrainDef = GenStep_RocksFromGrid.RockDefAt(c).building.naturalTerrain;
            return terrainDef;
        }

        private static bool GetCurrentNSEWMatchesStartNSEW(NSEW startNSEW, IntVec3 current, Map map)
        {
            if (current.z < map.Size.z / 2 && startNSEW == NSEW.South)
            {
                return true;
            }
            else if (current.z >= map.Size.z / 2 && startNSEW == NSEW.North)
            {
                return true;
            }
            else if (current.x < map.Size.x / 2 && startNSEW == NSEW.West)
            {
                return true;
            }
            else if (current.x >= map.Size.x / 2 && startNSEW == NSEW.North)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static ITerrainOverride GenerateMiddleArea(Map map)
        {
            int basePatchX = RandomBasePatch(map.Size.x);
            int basePatchZ = RandomBasePatch(map.Size.z);

            if (Settings.OpenAreaShape == ImpassableShape.Square)
            {
                int halfXSize = Settings.OpenAreaSizeX / 2;
                int halfZSize = Settings.OpenAreaSizeZ / 2;
                return new TerrainOverrideSquare(basePatchX - halfXSize, basePatchZ - halfZSize, basePatchX + halfXSize, basePatchZ + halfZSize);
            }
            return new TerrainOverrideRound(new IntVec3(basePatchX, 0, basePatchZ), Settings.OpenAreaSizeX);
        }

        private static bool IsMountainConcave(IntVec3 i, Map map, NSEW nsew, int deepestClearingSize, int currentZone)
        {
            float currentDeepestClearingSizeF = ((((float)currentZone / (float)Settings.ZoneCount) * 2) * (float)deepestClearingSize);
            int currentDeepestClearingSize = (int)currentDeepestClearingSizeF;

            // Default South.
            int currentValue = i.z;
            if (nsew == NSEW.North)
                currentValue = map.Size.z - i.z;
            else if (nsew == NSEW.West)
                currentValue = i.x;
            else if (nsew == NSEW.East)
                currentValue = map.Size.x - i.x;

            if (currentValue > currentDeepestClearingSize)
            {
                return true;
            }
            else
            {
                // Add a little edge variety.
                if (currentValue == (currentDeepestClearingSize - 2))
                {
                    if (RH_TET_DwarfsMod.random.Next(0, 5) == 0)
                    {
                        return true;
                    }
                }
                else if (currentValue == (currentDeepestClearingSize - 1))
                {
                    if (RH_TET_DwarfsMod.random.Next(0, 3) == 0)
                    {
                        return true;
                    }
                }
                else if (currentValue == (currentDeepestClearingSize))
                {
                    if (RH_TET_DwarfsMod.random.Next(0, 2) == 0)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private static bool IsMountainConvex(IntVec3 i, Map map, NSEW nsew, int deepestClearingSize, int shallowestClearingSize, int currentZone)
        {
            float currentDeepestClearingSizeF = ((1 - (((float)currentZone / (float)Settings.ZoneCount) * 2)) * (float)deepestClearingSize);
            int currentDeepestClearingSize = (int)currentDeepestClearingSizeF;
            currentDeepestClearingSize = currentDeepestClearingSize + 4;

            // Default South.
            int currentValue = i.z;
            if (nsew == NSEW.North)
                currentValue = map.Size.z - i.z;
            else if (nsew == NSEW.West)
                currentValue = i.x;
            else if (nsew == NSEW.East)
                currentValue = map.Size.x - i.x;

            if (currentValue > currentDeepestClearingSize)
            {
                return true;
            }
            else
            {
                // Add a little edge variety.
                if (currentValue == (currentDeepestClearingSize - 2))
                {
                    if (RH_TET_DwarfsMod.random.Next(0, 6) == 0)
                    {
                        return true;
                    }
                }
                else if (currentValue == (currentDeepestClearingSize - 1))
                {
                    if (RH_TET_DwarfsMod.random.Next(0, 3) == 0)
                    {
                        return true;
                    }
                }
                else if (currentValue == (currentDeepestClearingSize))
                {
                    if (RH_TET_DwarfsMod.random.Next(0, 2) == 0)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private static bool IsScatteredRock(IntVec3 i, Map map, int radius)
        {
            if (RH_TET_DwarfsMod.random.Next(50) >= 5)
            {
                return false;
            }

            if (Settings.OuterShape == ImpassableShape.Round)
            {
                // Round
                int x = i.x - (int)(map.Size.x * 0.5f);
                int z = i.z - (int)(map.Size.z * 0.5f);
                return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(z, 2)) < radius + 8;
            }

            // Square
            return true;
        }

        private static MountainShape GetMountainShape(int v)
        {
            if (v == 0)
                return MountainShape.Concave;
            else
                return MountainShape.Convex;
        }

        private static ImpassableShape GetShape(int v)
        {
            if (v == 0)
                return ImpassableShape.Square;
            else
                return ImpassableShape.Round;
        }

        private static NSEW GetNSEW(int v)
        {
            if (v == 0)
                return NSEW.North;
            else if (v == 1)
                return NSEW.South;
            else if (v == 2)
                return NSEW.East;
            else
                return NSEW.West;
        }

        static int RandomBasePatch(int size)
        {
            int half = size / 2;
            int delta = RH_TET_DwarfsMod.random.Next((int)(0.01 * Settings.PercentOffset * half));
            int sign = RH_TET_DwarfsMod.random.Next(2);
            if (sign == 0)
            {
                delta *= -1;
            }
            return half + delta;
        }
    }
}