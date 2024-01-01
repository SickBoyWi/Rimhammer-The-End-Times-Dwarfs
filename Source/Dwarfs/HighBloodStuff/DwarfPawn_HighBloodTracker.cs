using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace TheEndTimes_Dwarfs
{
    [StaticConstructorOnStartup]
    public class DwarfPawn_HighBloodTracker : IExposable
    {
        private static List<HighBlood> EmptyHighBloods = new List<HighBlood>();
        public const float CourtMTB = 10f;
        public const float RulingMTB = 3f;

        public const int CourtCheckInterval = 60;
        public const int RulingCheckInterval = 1000;

        public int lastCourtTicks = -999999;
        public int lastRuledTicks = -999999;
        public int applyRuledThoughtsTick = 0;
        public bool allowRoomRequirements = true;
        public bool allowApparelRequirements = true;
        public string godGranted = null;
        private Pawn pawn;
        private List<HighBlood> highBloods = new List<HighBlood>();

        public DwarfPawn_HighBloodTracker(Pawn pawnP)
        {
            this.pawn = pawnP;
        }

        public void SetHighBlood(HighBloodDef highBlooDefp, bool grantRewards, bool pawnWasThane, Building_DwarfThrone throne)
        {
            this.OnPreHighBloodChanged(highBlooDefp, grantRewards, pawnWasThane, throne);

            if (this.highBloods.Count > 0)
                this.highBloods.Clear();
            
            if (pawn.abilities == null)
                pawn.abilities = new Pawn_AbilityTracker(pawn);
            
            for (int index = 0; index < highBlooDefp.grantedAbilities.Count; ++index)
                this.pawn.abilities.GainAbility(highBlooDefp.grantedAbilities[index]);

            this.highBloods.Add(new HighBlood
            {
                def = highBlooDefp,
                receivedTick = GenTicks.TicksGame,
                highMaintenance = DwarfsUtil.ShouldBecomeHighMaintenanceOnHighBlood(this.pawn)
            });
            this.OnPostHighBloodChanged(highBlooDefp);
        }

        public void SetPawn(Pawn pawnP)
        {
            this.pawn = pawnP;
        }

        private void OnPreHighBloodChanged(
          HighBloodDef newHighBloodDef,
          bool grantRewards,
          bool pawnWasThane,
          Building_DwarfThrone throne)
        {
            if (!this.pawn.IsColonist)
                return;
            TaggedString taggedString1 = (TaggedString)((string)null);
            TaggedString taggedString2 = (TaggedString)((string)null);
            TaggedString taggedString3;
            TaggedString label;
            
            LookTargets lookTargets = (LookTargets)((Thing)pawn);
            
            if (newHighBloodDef.defName.Contains("Thane"))
            {
                label = "RH_TET_Dwarfs_NewThaneLabel".Translate();

                StringBuilder sb = new StringBuilder();
                sb.Append("RH_TET_Dwarfs_NewThane".Translate(this.pawn.Named("PAWN")));

                this.pawn.health.AddHediff(RH_TET_DwarfDefOf.RH_TET_Dwarfs_HighBloodThaneHediff, (BodyPartRecord)null, new DamageInfo?(), (DamageWorker.DamageResult)null);

                // Spawn thane armor and magic weapon!
                if (DwarfsUtil.IsSlayer(this.pawn))
                {
                    if (!throne.giftGranted)
                    { 
                        // Gazul
                        List<ThingDef> itemsToSpawn = new List<ThingDef> { RH_TET_DwarfDefOf.RH_TET_Dwarf_Helmet_MagicGazul, RH_TET_DwarfDefOf.RH_TET_Dwarf_Armor_MagicGazul, RH_TET_DwarfDefOf.RH_TET_Dwarfs_MagicWeapon_GazulHammer };

                        DwarfsUtil.SpawnItemsNear(itemsToSpawn, throne);
                        throne.giftGranted = true;
                        RH_TET_DwarfsMod.godsUsed.Add("Gazul");
                    }
                }
                else
                {
                    if (!throne.giftGranted)
                    {
                        // Grungi or Smednir
                        String god = "Grungi";
                        String otherGod = "Smednir";
                        if (RH_TET_DwarfsMod.random.Next(0, 2) == 0)
                        {
                            god = "Smednir";
                            otherGod = "Grungi";
                        }

                        if (RH_TET_DwarfsMod.godsUsed.Contains(god))
                        {
                            if (!RH_TET_DwarfsMod.godsUsed.Contains(otherGod))
                            {
                                string godSave = god;
                                god = otherGod;
                                otherGod = godSave;
                            }
                        }

                        List<ThingDef> itemsToSpawn = new List<ThingDef>();
                        if (god.Equals("Smednir"))
                        {
                            itemsToSpawn.Add(RH_TET_DwarfDefOf.RH_TET_Dwarf_Helmet_MagicSmednir);
                            itemsToSpawn.Add(RH_TET_DwarfDefOf.RH_TET_Dwarf_Armor_MagicSmednir);
                            itemsToSpawn.Add(RH_TET_DwarfDefOf.RH_TET_Dwarfs_MagicWeapon_SmednirAxe);
                        }
                        else
                        {
                            // Grungi
                            itemsToSpawn.Add(RH_TET_DwarfDefOf.RH_TET_Dwarf_Helmet_MagicGrungi);
                            itemsToSpawn.Add(RH_TET_DwarfDefOf.RH_TET_Dwarf_Armor_MagicGrungi);
                            itemsToSpawn.Add(RH_TET_DwarfDefOf.RH_TET_Dwarfs_MagicWeapon_GrungniPickaxe);
                        }

                        DwarfsUtil.SpawnItemsNear(itemsToSpawn, throne);
                        throne.giftGranted = true;

                        sb.Append("RH_TET_Dwarfs_NewHighBlood_Gift".Translate());
                    }
                }

                taggedString3 = sb.ToString();
            }
            else if (newHighBloodDef.defName.Contains("King"))
            {
                String highBlood = "RH_TET_Dwarfs_King".Translate();
                if (pawn.gender == Gender.Female)
                    highBlood = "RH_TET_Dwarfs_Queen".Translate();
                
                label = "RH_TET_Dwarfs_NewKingLabel".Translate(highBlood);

                StringBuilder sb = new StringBuilder();

                if (pawnWasThane)
                {
                    sb.Append("RH_TET_Dwarfs_NewKingPromotedStart".Translate(pawn.Name, highBlood));

                    Hediff thane = pawn.health.hediffSet.GetFirstHediffOfDef(RH_TET_DwarfDefOf.RH_TET_Dwarfs_HighBloodThaneHediff, false);
                    if (thane != null)
                        this.pawn.health.RemoveHediff(thane);
                }
                else
                {
                    sb.Append("RH_TET_Dwarfs_NewKingStart".Translate(pawn.Name, highBlood));
                    
                    this.pawn.health.AddHediff(RH_TET_DwarfDefOf.RH_TET_Dwarfs_HighBloodKingHediff, (BodyPartRecord)null, new DamageInfo?(), (DamageWorker.DamageResult)null);
                }

                sb.Append("RH_TET_Dwarfs_NewKingEnd".Translate(highBlood));

                if (!throne.giftGranted)
                { 
                    // Spawn king armor and magic weapon!
                    if (DwarfsUtil.IsSlayer(this.pawn))
                    {
                        // Grimnir
                        List<ThingDef> itemsToSpawn = new List<ThingDef> { RH_TET_DwarfDefOf.RH_TET_Dwarf_Helmet_MagicGrimnir, RH_TET_DwarfDefOf.RH_TET_Dwarf_Armor_MagicGrimnir, RH_TET_DwarfDefOf.RH_TET_Dwarfs_MagicWeapon_GrimnirAxe };

                        DwarfsUtil.SpawnItemsNear(itemsToSpawn, throne);
                        throne.giftGranted = true;
                        RH_TET_DwarfsMod.godsUsed.Add("Grimnir");
                    }
                    else
                    {
                        // Valaya
                        List<ThingDef> itemsToSpawn = new List<ThingDef> { RH_TET_DwarfDefOf.RH_TET_Dwarf_Helmet_MagicValaya, RH_TET_DwarfDefOf.RH_TET_Dwarf_Armor_MagicValaya, RH_TET_DwarfDefOf.RH_TET_Dwarfs_MagicWeapon_ValayaHammer };

                        DwarfsUtil.SpawnItemsNear(itemsToSpawn, throne);
                        throne.giftGranted = true;
                        RH_TET_DwarfsMod.godsUsed.Add("Valaya");
                    }
                    sb.Append("RH_TET_Dwarfs_NewHighBlood_Gift".Translate(highBlood));
                }

                taggedString3 = sb.ToString();
            }

            List<Thing> thingList = new List<Thing>()
                  {
                    (Thing) this.pawn
                  };

            TaggedString text = (TaggedString)taggedString3.Resolve().TrimEndNewlines();

            Find.LetterStack.ReceiveLetter(label, text, RH_TET_DwarfDefOf.RH_TET_Dwarfs_CapturedHold, lookTargets, (Faction)null, (Quest)null, (List<ThingDef>)null, (string)null);
        }

        private void OnPostHighBloodChanged(HighBloodDef newHighBloodDef)
        {
            this.pawn.Notify_DisabledWorkTypesChanged();
            this.pawn.needs?.AddOrRemoveNeedsAsAppropriate();
            if (newHighBloodDef == null)
                return;
            if (newHighBloodDef.disabledJoyKinds != null && this.pawn.jobs != null)
            {
                foreach (JoyKindDef disabledJoyKind in newHighBloodDef.disabledJoyKinds)
                    this.pawn.jobs.Notify_JoyKindDisabled(disabledJoyKind);
            }

            this.CleanupThoughts(newHighBloodDef);
            if (newHighBloodDef.awardThought != null && this.pawn.needs != null && this.pawn.needs.mood != null)
            {
                Thought_MemoryHighBlood memoryHighBlood = (Thought_MemoryHighBlood)ThoughtMaker.MakeThought(newHighBloodDef.awardThought);
                memoryHighBlood.highBlood = newHighBloodDef;
                this.pawn.needs.mood.thoughts.memories.TryGainMemory((Thought_Memory)memoryHighBlood, (Pawn)null);
            }
        }

        public IEnumerable<string> GetUnmetThroneroomRequirements(Pawn pawn,
          bool includeOnGracePeriod = true,
          bool onlyOnGracePeriod = false)
        {
            List<ThingWithComps> thrones = null;

            if (RH_TET_DwarfsMod.assignedThrones != null 
                    && RH_TET_DwarfsMod.assignedThrones.TryGetValue(pawn) != null 
                    && RH_TET_DwarfsMod.assignedThrones.TryGetValue(pawn).Count > 0)
                thrones = RH_TET_DwarfsMod.assignedThrones.TryGetValue(pawn);

            if (thrones != null && this.highBloods.Count > 0)
            {
                HighBlood highBlood = this.highBloods.First();
                if (highBlood != null)
                {
                    foreach (Building_DwarfThrone throne in thrones)
                    {
                        Room throneRoom = throne.GetRoom(RegionType.Set_Passable);
                        if (throneRoom != null)
                        {
                            foreach (RoomRequirement throneRoomRequirement in highBlood.def.highBloodRoomRequirements)
                            {
                                if (!throneRoomRequirement.Met(throneRoom))
                                {
                                    bool flag1 = highBlood.RoomRequirementGracePeriodActive(this.pawn);
                                    if ((!onlyOnGracePeriod || flag1) && (!flag1 || includeOnGracePeriod))
                                        yield return throneRoomRequirement.LabelCap(throneRoom);
                                }
                            }
                        }
                    }
                }
            }
        }

        public bool CanRequireBedroom()
        {
            if (this.allowRoomRequirements)
                return !this.pawn.IsPrisoner;
            return false;
        }

        public HighBlood HighestHighBloodWithBedroomRequirements()
        {
            if (!this.CanRequireBedroom())
                return (HighBlood)null;
            HighBlood highBlood = (HighBlood)null;
            List<HighBlood> highBloodsForReading = this.HighBloodInEffectForReading;
            for (int index = 0; index < highBloodsForReading.Count; ++index)
            {
                if (!highBloodsForReading[index].def.GetBedroomRequirements(this.pawn).EnumerableNullOrEmpty<RoomRequirement>() && (highBlood == null || highBloodsForReading[index].def.seniority > highBlood.def.seniority))
                    highBlood = highBloodsForReading[index];
            }
            return highBlood;
        }

        public IEnumerable<string> GetUnmetBedroomRequirements(
          bool includeOnGracePeriod = true,
          bool onlyOnGracePeriod = false)
        {
            if (this.highBloods.Count > 0)
            {
                HighBlood highBlood = this.highBloods.First();
                if (highBlood != null)
                {
                    bool gracePeriodActive = highBlood.RoomRequirementGracePeriodActive(this.pawn);

                    if (this.HasPersonalBedroom())
                    {
                        Room bedroom = this.pawn.ownership.OwnedRoom;
                        foreach (RoomRequirement bedroomRequirement in highBlood.def.GetBedroomRequirements(this.pawn))
                        {
                            if (!bedroomRequirement.Met(bedroom))
                            {
                                if ((!onlyOnGracePeriod || gracePeriodActive) && (!gracePeriodActive || includeOnGracePeriod))
                                    yield return bedroomRequirement.LabelCap(bedroom);
                            }
                        }
                    }
                }
            }
        }

        public bool CanRequireThroneroom()
        {
            if (this.pawn.IsFreeColonist && this.allowRoomRequirements)
                return !this.pawn.IsQuestLodger();
            return false;
        }

        public HighBlood HighestHighBloodWithThroneRoomRequirements()
        {
            if (!this.CanRequireThroneroom())
                return (HighBlood)null;
            HighBlood highBlood = (HighBlood)null;
            List<HighBlood> effectForReading = this.HighBloodInEffectForReading;
            for (int index = 0; index < effectForReading.Count; ++index)
            {
                if (!effectForReading[index].def.highBloodRoomRequirements.EnumerableNullOrEmpty<RoomRequirement>() && (highBlood == null || effectForReading[index].def.seniority > highBlood.def.seniority))
                    highBlood = effectForReading[index];
            }
            return highBlood;
        }

        public List<HighBlood> HighBloodInEffectForReading
        {
            get
            {
                if (!this.pawn.IsWildMan())
                {
                    List<HighBlood> retHighBloods = new List<HighBlood>();
                    retHighBloods.AddRange(this.highBloods);
                    return retHighBloods;
                }
                return DwarfPawn_HighBloodTracker.EmptyHighBloods;
            }
        }

        public bool HasPersonalBedroom()
        {
            Building_Bed ownedBed = this.pawn.ownership.OwnedBed;
            if (ownedBed == null)
                return false;
            Room ownedRoom = this.pawn.ownership.OwnedRoom;
            if (ownedRoom == null)
                return false;
            foreach (Building_Bed containedBed in ownedRoom.ContainedBeds)
            {
                if (containedBed != ownedBed && containedBed.OwnersForReading.Any<Pawn>((Predicate<Pawn>)(p =>
                {
                    if (p != this.pawn)
                        return !LovePartnerRelationUtility.LovePartnerRelationExists(p, this.pawn);
                    return false;
                })))
                    return false;
            }
            return true;
        }
        
        public void ExposeData()
        {
            Scribe_Collections.Look<HighBlood>(ref this.highBloods, "highBloods", LookMode.Deep, (object[])Array.Empty<object>());
            Scribe_Values.Look<int>(ref this.lastCourtTicks, "lastCourtTicks", -999999, false);
            Scribe_Values.Look<int>(ref this.lastRuledTicks, "lastRuledTicks", -999999, false); 
            Scribe_Values.Look<int>(ref this.applyRuledThoughtsTick, "applyRuledThoughtsTick", 0, false);
            Scribe_Values.Look<bool>(ref this.allowRoomRequirements, "allowRoomRequirements", true, false);
            Scribe_Values.Look<bool>(ref this.allowApparelRequirements, "allowApparelRequirements", true, false);
            Scribe_Values.Look<string>(ref this.godGranted, "godGranted", null, false);
            if (Scribe.mode != LoadSaveMode.PostLoadInit)
                return;
            if (this.highBloods != null && this.highBloods.Count > 0)
            { 
                foreach (HighBlood highBlood in this.highBloods)
                { 
                    if (highBlood.def == null)
                    { 
                        Log.Error("Some High Bloods had null defs after loading.", false);
                        break;
                    }

                    if (highBlood.def.Equals(RH_TET_DwarfDefOf.RH_TET_Dwarfs_HighBloodThane))
                        RH_TET_DwarfsMod.thanes.AddDistinct(pawn);
                    else if (highBlood.def.Equals(RH_TET_DwarfDefOf.RH_TET_Dwarfs_HighBloodKing))
                        RH_TET_DwarfsMod.king = pawn;
                }
            }
            if (this.godGranted != null)
            {
                RH_TET_DwarfsMod.godsUsed.Add(this.godGranted);
            }
        }

        public void HighBloodTrackerTick()
        {
            // REMOVE ALL BELOW THIS IN TO MAKE PAWNS NOT AUTO HOLD COURT ONCE IN A WHILE AGAIN. ALSO, THE CODE THAT CALLS THIS METHOD CAN BE COMMENTED OUT.
            if (!this.pawn.Spawned || this.pawn.RaceProps.Animal)
                return;
            if (this.pawn.IsHashIntervalTick(CourtCheckInterval) && this.pawn.Map.IsPlayerHome
                    && (DwarfsHighBloodGetTogetherUtility.AcceptableGameConditionsToStartGathering(this.pawn.Map))
                    && (Rand.MTBEventOccurs(CourtMTB, 60000f, CourtCheckInterval)
                    && (double)(Find.TickManager.TicksGame - this.lastCourtTicks) >= (CourtMTB * 60000f) && this.TryStartHoldCourt(this.pawn)))
                this.lastCourtTicks = Find.TickManager.TicksGame;
        }

        public bool TryStartHoldCourt(Pawn pawn)
        {
            if (!RH_TET_DwarfDefOf.RH_TET_Dwarf_HighBloodCourt.CanExecute(pawn.Map, pawn, true))
                return false;
            RH_TET_DwarfDefOf.RH_TET_Dwarf_HighBloodCourt.Worker.TryExecute(pawn.Map, pawn);
            return true;
        }

        public HighBloodDef MainHighBlood()
        {
            if (this.highBloods == null || this.highBloods.Count == 0)
                return (HighBloodDef)null;
            return this.highBloods.First().def;
        }

        public HighBloodDef GetCurrentHighBlood()
        {
            if (this.highBloods == null || this.highBloods.Count == 0)
                return (HighBloodDef)null;
            return this.highBloods.First().def;
        }

        private void CleanupThoughts(HighBloodDef highBloodDef)
        {
            if (highBloodDef == null)
                return;
            if (highBloodDef.awardThought != null && this.pawn.needs.mood != null)
                this.pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(highBloodDef.awardThought);
        }
    }
}
