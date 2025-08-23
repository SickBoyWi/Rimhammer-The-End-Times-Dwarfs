using HugsLib.Utils;
using RimWorld;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TheEndTimes_Dwarfs
{
    [DefOf]
    public static class RH_TET_DwarfDefOf
    {
        public static WorldObjectDef RH_TET_Dwarfs_CapturedHoldWorldObject;
        public static MapGeneratorDef RH_TET_Dwarfs_EmptyMap;
        public static MapGenDef RH_TET_Dwarfs_CapturedHoldMap;
        public static FactionDef TribeRough;
        public static FactionDef RH_TET_Dwarf_KarakMountain;

        public static DesignationDef RH_TET_Dwarfs_BlastingWireDryOut;
        public static DesignationDef RH_TET_Dwarfs_DecorateWallDes;

        public static JobDef RH_TET_Dwarfs_DetonateBlast;
        public static JobDef RH_TET_Dwarfs_DryBlastingWire;
        public static JobDef RH_TET_Dwarfs_Rule;
        public static JobDef RH_TET_Dwarfs_HoldCourt;
        public static JobDef RH_TET_Dwarfs_DecorateWallJob;

        public static RecipeDef RH_TET_Dwarfs_RepairApparel;
        public static RecipeDef RH_TET_Dwarfs_RepairArmor;
        public static RecipeDef RH_TET_Dwarfs_RepairMeleeWeapon;
        public static RecipeDef RH_TET_Dwarfs_RepairRangedWeapon;

        public static SoundDef RH_TET_Dwarfs_DetonatorLever;
        public static SoundDef RH_TET_Dwarfs_MountainDrop;
        public static SoundDef RH_TET_Dwarf_Fanfare;
        public static SoundDef RH_TET_Dwarf_Victory;
        public static SoundDef RH_TET_Dwarfs_PlayDice;

        public static StatDef RH_TET_Dwarfs_RockYield;
        public static StatDef RH_TET_Dwarfs_ResourceYield;
        public static StatDef RH_TET_Dwarfs_WoodYield;

        public static ThingDef RH_TET_Dwarfs_CollapsedMountainRocks;
        public static ThingDef RH_TET_Dwarf_Gromril;
        public static ThingDef RH_TET_Coal;
        public static ThingDef RH_TET_Saltpeter;
        public static ThingDef RH_TET_Sulfur;
        public static ThingDef RH_TET_Dwarfs_Throne;
        public static ThingDef RH_TET_Dwarfs_KingsThrone;
        public static ThingDef RH_TET_Dwarfs_DicingTable;
        public static ThingDef RH_TET_SkavenTunnel;
        public static ThingDef RH_TET_SkavenTunnelSpawner;
        public static ThingDef RH_TET_Dwarf_MiningHelmetLight;
        public static ThingDef RH_TET_Dwarfs_SmoothedSandstone_Decorated;
        public static ThingDef RH_TET_Dwarfs_SmoothedGranite_Decorated;
        public static ThingDef RH_TET_Dwarfs_SmoothedLimestone_Decorated;
        public static ThingDef RH_TET_Dwarfs_SmoothedSlate_Decorated;
        public static ThingDef RH_TET_Dwarfs_SmoothedMarble_Decorated;
        public static ThingDef RH_TET_Dwarfs_Horn;
        public static ThingDef RH_TET_Dwarfs_ColumnSmallA;
        public static ThingDef RH_TET_Dwarfs_ColumnSmallB;
        public static ThingDef RH_TET_Dwarfs_ColumnLitSmallA;
        public static ThingDef RH_TET_Dwarfs_ColumnLitSmallB;
        public static ThingDef RH_TET_Dwarfs_ColumnBigA;
        public static ThingDef RH_TET_Dwarfs_ColumnBigB;
        public static ThingDef RH_TET_Dwarfs_ColumnLitBigA;
        public static ThingDef RH_TET_Dwarfs_ColumnLitBigB;
        public static ThingDef RH_TET_Dwarfs_Brazier;
        public static ThingDef RH_TET_Dwarfs_Banner;
        public static ThingDef RH_TET_Dwarfs_OathStone;
        public static ThingDef RH_TET_Dwarfs_DoubleBed;
        public static ThingDef RH_TET_Dwarfs_RoyalBed;
        public static ThingDef RH_TET_Dwarfs_EndTable;
        public static ThingDef RH_TET_Dwarfs_Dresser;
        public static ThingDef RH_TET_Dwarfs_SteamInfestationLure;

        public static BodyPartDef RH_TET_Dwarfs_BP_Beard;

        // Magic weapons and armor.
        // Thane
        public static ThingDef RH_TET_Dwarf_Helmet_MagicGrungi;
        public static ThingDef RH_TET_Dwarf_Armor_MagicGrungi;
        public static ThingDef RH_TET_Dwarfs_MagicWeapon_GrungniPickaxe;
        public static ThingDef RH_TET_Dwarf_Helmet_MagicSmednir;
        public static ThingDef RH_TET_Dwarf_Armor_MagicSmednir;
        public static ThingDef RH_TET_Dwarfs_MagicWeapon_SmednirAxe;
        // Slayer Thane
        public static ThingDef RH_TET_Dwarf_Helmet_MagicGazul;
        public static ThingDef RH_TET_Dwarf_Armor_MagicGazul;
        public static ThingDef RH_TET_Dwarfs_MagicWeapon_GazulHammer;
        // King
        public static ThingDef RH_TET_Dwarf_Helmet_MagicValaya;
        public static ThingDef RH_TET_Dwarf_Armor_MagicValaya;
        public static ThingDef RH_TET_Dwarfs_MagicWeapon_ValayaHammer;
        // Slayer King
        public static ThingDef RH_TET_Dwarf_Helmet_MagicGrimnir;
        public static ThingDef RH_TET_Dwarf_Armor_MagicGrimnir;
        public static ThingDef RH_TET_Dwarfs_MagicWeapon_GrimnirAxe;
        
        // Thoughts
        public static ThoughtDef RH_TET_Dwarfs_FailedToReclaimCapturedHold;
        public static ThoughtDef RH_TET_Dwarfs_ReclaimedCapturedHold;
        public static ThoughtDef RH_TET_Dwarfs_ReclaimingCapturedHold;
        public static ThoughtDef RH_TET_Dwarfs_AteFoodInappropriateForHighBlood;
        public static ThoughtDef RH_TET_Dwarfs_AThaneWasCrowned;
        public static ThoughtDef RH_TET_Dwarfs_AKingWasCrowned;
        public static ThoughtDef RH_TET_Dwarfs_DeadThane;
        public static ThoughtDef RH_TET_Dwarfs_DeadKing;
        public static ThoughtDef RH_TET_Dwarfs_Ruled;
        public static ThoughtDef RH_TET_Dwarfs_AwfulHeldCourt;
        public static ThoughtDef RH_TET_Dwarfs_BoringHeldCourt;
        public static ThoughtDef RH_TET_Dwarfs_DecentHeldCourt;
        public static ThoughtDef RH_TET_Dwarfs_GreatHeldCourt;

        public static TaleDef RH_TET_Dwarf_ReclaimedHold;

        public static LetterDef RH_TET_Dwarfs_CapturedHold;

        public static HighBloodDef RH_TET_Dwarfs_HighBloodThane;
        public static HighBloodDef RH_TET_Dwarfs_HighBloodKing;

        public static RoomRoleDef RH_TET_Dwarfs_RulingRoom;

        public static HediffDef RH_TET_Dwarfs_HighBloodThaneHediff;
        public static HediffDef RH_TET_Dwarfs_HighBloodKingHediff;

        public static GatheringDef RH_TET_Dwarf_HighBloodCourt;
        
        public static DutyDef RH_TET_Dwarfs_HoldCourtDuty;
        public static DutyDef RH_TET_Dwarfs_ObserveCourtDuty;
        public static DutyDef RH_TET_SkavenDefendAndExpandTunnel;
        public static DutyDef RH_TET_SkavenDefendTunnelAggressively;

        public static AbilityDef RH_TET_Dwarfs_HoldCourtAbility;

        public static ResearchProjectDef RH_TET_Dwarf_WallsAdvanced;

        public static PawnKindDef RH_TET_DwarfSlave;
        public static PawnKindDef RH_TET_DwarfSlayerVillager;

        public static InspirationDef RH_TET_Dwarfs_Mining_Frenzy_Yield;
        public static InspirationDef RH_TET_Dwarfs_Inspired_Tending;
        public static InspirationDef RH_TET_Dwarfs_Frenzy_Combat;
        public static InspirationDef RH_TET_Dwarfs_Frenzy_GenWork;
        public static InspirationDef RH_TET_Dwarfs_Frenzy_Constr;
        public static InspirationDef RH_TET_Dwarfs_Frenzy_Dodge;

        public static Faction GetFinalEnemyFaction()
        {
            Faction f = Find.FactionManager.FirstFactionOfDef(RH_TET_DwarfDefOf.TribeRough);

            if (f is null)
            {
                foreach (Faction fact in Find.FactionManager.AllFactionsVisible)
                    if (fact.HostileTo(Faction.OfPlayer) && fact.def.techLevel.Equals(TechLevel.Neolithic))
                    {
                        f = fact;
                        break;
                    }
            }

            return f;
        }

        [StaticConstructorOnStartup]
        public static class Textures
        {
            public static Texture2D CommandButton_DryOutUI;
            public static Texture2D CommandButton_DetonateUI;
            public static Texture2D CommandButton_DetonateManualUI;
            public static Texture2D CommandButton_SelectDesigWireUI;

            static Textures()
            {
                foreach (FieldInfo field in typeof(RH_TET_DwarfDefOf.Textures).GetFields((BindingFlags)HugsLibUtility.AllBindingFlags))
                {
                    if (!field.IsInitOnly)
                        field.SetValue((object)null, (object)ContentFinder<Texture2D>.Get(field.Name, true));
                }
            }
        }
    } 
}