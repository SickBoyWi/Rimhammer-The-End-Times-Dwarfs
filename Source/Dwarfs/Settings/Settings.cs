using RimWorld;
using System.Text;
using UnityEngine;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class SettingsController : Mod
    {
        public SettingsController(ModContentPack content) : base(content)
        {
            base.GetSettings<Settings>();
        }

        public override string SettingsCategory()
        {
            return "Rimhammer - Dwarfs";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoSettingsWindowContents(inRect);
        }
    }

    public enum ImpassableShape
    {
        Square,
        Round
    }

    public enum NSEW
    {
        North,
        South,
        East,
        West
    }

    public enum MountainShape
    {
        Concave,
        Convex
    }

    public class Settings : ModSettings
    {
        private const int GAP_SIZE = 24;
        private const float DEFAULT_PERCENT_OFFSET = 5f;
        private const int DEFAULT_OPEN_AREA_SIZE = 54;
        private const int DEFAULT_WALLS_SMOOTHNESS = 10;
        private const int DEFAULT_PEREMETER_BUFFER = 6;
        private const int DEFAULT_QUARY_SIZE = 5;
        private const float DEFAULT_MOVEMENT_DIFFICULTY = 4.5f;
        private const int DEFAULT_OUTER_RADIUS = 1;
        private const int DEFAULT_ZONE_COUNT = 25;
        private const bool DEFAULT_USE_VANILLAMAP = true;
        private const float VANILLA_MAP_STONE_DENSITY = 0f;
        public const float VANILLA_MAP_STONE_DENSITY_BASE = 1.65f;
        private static Vector2 scrollPosition = Vector2.zero;
        private const bool DEFAULT_USE_ANYONEIMPASSIBLE = false;
        private const int DEFAULT_LURECHANCE = 100;

        private static float percentOffset = DEFAULT_PERCENT_OFFSET;
        public static float PercentOffset { get { return percentOffset; } }
        public static ImpassableShape OpenAreaShape = ImpassableShape.Square;
        public static int OpenAreaSizeX = DEFAULT_OPEN_AREA_SIZE;
        public static int OpenAreaSizeZ = DEFAULT_OPEN_AREA_SIZE;
        public static int MiddleWallSmoothness = 5;
        public static int PeremeterBuffer = DEFAULT_PEREMETER_BUFFER;
        public static bool HasMiddleArea = true;
        public static ImpassableShape OuterShape = ImpassableShape.Square;
        public static bool ScatteredRocks = true;
        public static bool IncludeQuarySpot = false;
        public static int OuterRadius = 1;
        public static int QuarySize = DEFAULT_QUARY_SIZE;
        public static bool TrueRandom = false;
        public static float MovementDifficulty = DEFAULT_MOVEMENT_DIFFICULTY;
        private static string movementDifficultyBuffer = DEFAULT_MOVEMENT_DIFFICULTY.ToString();
        public static int ZoneCount = DEFAULT_ZONE_COUNT;
        public static bool UseVanillaMap = DEFAULT_USE_VANILLAMAP;
        public static float VanillaMapStoneDensithy = VANILLA_MAP_STONE_DENSITY;
        public static bool AnyOneImpossible = DEFAULT_USE_ANYONEIMPASSIBLE;

        public static int LureChance = DEFAULT_LURECHANCE;

        public override void ExposeData()
        {
            base.ExposeData();

            string s = OuterShape.ToString();
            string openAreaShape = OpenAreaShape.ToString();

            Scribe_Values.Look<bool>(ref HasMiddleArea, "RH_TET_Dwarfs_CreateCenterArea", true, false);
            Scribe_Values.Look<int>(ref OpenAreaSizeX, "RH_TET_Dwarfs_CenterAreaSizeX", DEFAULT_OPEN_AREA_SIZE, false);
            Scribe_Values.Look<int>(ref OpenAreaSizeZ, "RH_TET_Dwarfs_CenterAreaSizeZ", DEFAULT_OPEN_AREA_SIZE, false);

            Scribe_Values.Look<float>(ref percentOffset, "RH_TET_Dwarfs_percentOffset", DEFAULT_PERCENT_OFFSET, false);
            Scribe_Values.Look<string>(ref openAreaShape, "RH_TET_Dwarfs_OpenAreaShape", ImpassableShape.Square.ToString(), false);
            Scribe_Values.Look<int>(ref PeremeterBuffer, "RH_TET_Dwarfs_PeremeterBuffer", DEFAULT_PEREMETER_BUFFER, false);
            Scribe_Values.Look<int>(ref MiddleWallSmoothness, "RH_TET_Dwarfs_MakeWallsSmooth", DEFAULT_WALLS_SMOOTHNESS, false);
            Scribe_Values.Look<string>(ref s, "RH_TET_Dwarfs_Shape", ImpassableShape.Square.ToString(), false);
            Scribe_Values.Look<bool>(ref ScatteredRocks, "RH_TET_Dwarfs_scatteredRocks", true, false);
            Scribe_Values.Look<bool>(ref IncludeQuarySpot, "RH_TET_Dwarfs_IncludeQuarySpot", false, false);
            Scribe_Values.Look<int>(ref QuarySize, "RH_TET_Dwarfs_QuarySize", DEFAULT_QUARY_SIZE, false);
            Scribe_Values.Look<float>(ref MovementDifficulty, "RH_TET_Dwarfs_MovementDifficulty", DEFAULT_MOVEMENT_DIFFICULTY, false);
            Scribe_Values.Look<int>(ref OuterRadius, "RH_TET_Dwarfs_OuterRadius", DEFAULT_OUTER_RADIUS, false);
            Scribe_Values.Look<bool>(ref TrueRandom, "RH_TET_Dwarfs_TrueRandom", false, false);
            Scribe_Values.Look<bool>(ref UseVanillaMap, "RH_TET_Dwarfs_UseVanillaMap", true, false);
            Scribe_Values.Look<float>(ref VanillaMapStoneDensithy, "RH_TET_Dwarfs_VanillaDensity", VANILLA_MAP_STONE_DENSITY, false);
            Scribe_Values.Look<bool>(ref AnyOneImpossible, "RH_TET_Dwarfs_AnyOneImpossible", false, false);
            Scribe_Values.Look<int>(ref LureChance, "RH_TET_Dwarfs_LureChance", DEFAULT_LURECHANCE, false);

            movementDifficultyBuffer = MovementDifficulty.ToString();

            if (Scribe.mode != LoadSaveMode.Saving)
            {
                if (ImpassableShape.Round.ToString().Equals(s))
                {
                    OuterShape = ImpassableShape.Round;
                }
                else
                {
                    OuterShape = ImpassableShape.Square;
                }

                if (ImpassableShape.Round.ToString().Equals(openAreaShape))
                {
                    OpenAreaShape = ImpassableShape.Round;
                }
                else
                {
                    OpenAreaShape = ImpassableShape.Square;
                }
            }
        }

        public static void DoSettingsWindowContents(Rect rect)
        {
            Rect scroll = new Rect(5f, 45f, 430, rect.height - 40);
            Rect view = new Rect(0, 45, 400, 1200);

            Widgets.BeginScrollView(scroll, ref scrollPosition, view, true);
            Listing_Standard ls = new Listing_Standard();
            ls.Begin(view);

            ls.Label("RH_TET_Dwarfs_InfestLure_Fail_SettingsChance".Translate() + ": " + LureChance);
            LureChance = (int)ls.Slider(LureChance, 0, 100);
            ls.Gap(GAP_SIZE);

            ls.Label("RH_TET_Dwarfs_ImpassableMap".Translate());
            ls.Gap(GAP_SIZE);

            ls.CheckboxLabeled("RH_TET_Dwarfs_ImpassibleMapForAll".Translate(), ref AnyOneImpossible);
            ls.Gap(GAP_SIZE);

            ls.CheckboxLabeled("RH_TET_Dwarfs_UseVanillaMapGen".Translate(), ref UseVanillaMap);
            ls.Label("RH_TET_Dwarfs_UseVanillaMapGenDesc".Translate());
            ls.Gap(GAP_SIZE);
            if (UseVanillaMap)
            {
                ls.Label("RH_TET_Dwarfs_VanillaMountainDensity".Translate() + ": " + VanillaMapStoneDensithy);
                VanillaMapStoneDensithy = (int)ls.Slider(VanillaMapStoneDensithy, 0, 30);
                ls.Gap(GAP_SIZE);
            }
            else
            { 
                ls.CheckboxLabeled("RH_TET_Dwarfs_CreateCenterArea".Translate(), ref HasMiddleArea);
                ls.Gap(GAP_SIZE);

                if (HasMiddleArea)
                {
                    ls.Label("RH_TET_Dwarfs_CenterAreaSize".Translate() + ": " + OpenAreaSizeZ);
                    OpenAreaSizeX = (int)ls.Slider(OpenAreaSizeX, 5, 150);
                    OpenAreaSizeZ = OpenAreaSizeX;
                    ls.Gap(GAP_SIZE);

                    if (ls.ButtonText("RH_TET_Dwarfs_Default".Translate()))
                    {
                        OpenAreaSizeX = DEFAULT_OPEN_AREA_SIZE;
                        OpenAreaSizeZ = DEFAULT_OPEN_AREA_SIZE;
                    }
                    ls.Gap(GAP_SIZE);
                }
            }

            ls.End();
            Widgets.EndScrollView();
        }
    }
}