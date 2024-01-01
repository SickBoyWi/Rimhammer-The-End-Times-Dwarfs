using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class CompAssignableToPawn_Throne : CompAssignableToPawn
    {
        public override IEnumerable<Pawn> AssigningCandidates
        {
            get
            {
                if (!this.parent.Spawned)
                    return Enumerable.Empty<Pawn>();
                return (IEnumerable<Pawn>)this.parent.Map.mapPawns.FreeColonists.Where<Pawn>((Func<Pawn, bool>)(p =>
                {
                    if (DwarfsUtil.IsDwarf(p) && p.health.capacities.CapableOf(PawnCapacityDefOf.Talking)
                            && !p.skills.GetSkill(SkillDefOf.Social).TotallyDisabled && !p.WorkTagIsDisabled(WorkTags.Violent))
                        return true;
                    return false;
                })).OrderByDescending<Pawn, bool>((Func<Pawn, bool>)(p => this.CanAssignTo(p).Accepted));
            }
        }

        public override string CompInspectStringExtra()
        {
            if (this.AssignedPawnsForReading.Count == 0)
                return (string)("Owner".Translate() + ": " + "Nobody".Translate());
            if (this.AssignedPawnsForReading.Count == 1)
                return (string)("Owner".Translate() + ": " + this.AssignedPawnsForReading[0].Label);
            return "";
        }

        public override bool AssignedAnything(Pawn pawn)
        {
            return RH_TET_DwarfsMod.assignedThrones.TryGetValue(pawn) != null;
        }

        public override void TryAssignPawn(Pawn pawn)
        {
            if (this.ValidateTryPawnAdd(pawn))
            {
                bool pawnIsThane = RH_TET_DwarfsMod.thanes.Contains(pawn);
                bool pawnIsKing = (RH_TET_DwarfsMod.king != null && RH_TET_DwarfsMod.king.Equals(pawn));

                if (!pawnIsKing && !pawnIsThane)
                {
                    Find.WindowStack.Add((Window)Dialog_MessageBox.CreateConfirmation((TaggedString)"RH_TET_Dwarfs_AssignHighBloodConf".Translate(),
                        (Action)(() =>
                        {
                            this.DoAssignPawn(pawn);
                        }), true, (string)null));
                }
                else
                {
                    this.DoAssignPawn(pawn);
                }
            }
        }

        private void DoAssignPawn(Pawn pawn)
        {
            if (pawn != null && !DwarfsUtil.IsDwarf(pawn))
            {
                Messages.Message((TaggedString)(Translator.Translate("RH_TET_Dwarfs_LeaderMustBeDwarf")), (MessageTypeDef)MessageTypeDefOf.NegativeEvent, true);
                return;
            }

            var building = this.parent as Building_DwarfThrone;
            
            if (building != null)
            {
                if (building.AssignedPawn == pawn)
                    return;
                else if (building.AssignedPawn != null)
                    this.TryUnassignPawn(building.AssignedPawn);

                // Either this throne had no owner, or the unassign step removed it.
                if (building.AssignedPawn == null)
                {
                    // Check for thane/king with no assigned throne.
                    if (!AllHighBloodHaveSeats(building, pawn))
                    {
                        Messages.Message((TaggedString)(Translator.Translate("RH_TET_Dwarfs_ThaneKingWithNoHighSeat")), (MessageTypeDef)MessageTypeDefOf.NegativeEvent, true);
                        return;
                    }
                    
                    // No king on thane throne.
                    if (building.def.defName.Equals("RH_TET_Dwarfs_Throne") && RH_TET_DwarfsMod.king != null && RH_TET_DwarfsMod.king.Equals(pawn))
                    {
                        Messages.Message((TaggedString)(Translator.Translate("RH_TET_Dwarfs_KingNoSwapThrone")), (MessageTypeDef)MessageTypeDefOf.NegativeEvent, true);
                        return;
                    }
                    
                    bool pawnWasThane = RH_TET_DwarfsMod.thanes.Contains(pawn);
                    bool pawnPromotedToKing = false;

                    List<ThingWithComps> thrones = RH_TET_DwarfsMod.assignedThrones.TryGetValue(pawn);

                    if (building.def.defName.Equals("RH_TET_Dwarfs_KingsThrone"))
                    {
                        bool wasKingBefore = (RH_TET_DwarfsMod.king != null && RH_TET_DwarfsMod.king.Equals(pawn));
                        pawnPromotedToKing = true;
                        
                        RH_TET_DwarfsMod.king = pawn;
                        
                        if (!wasKingBefore)
                        {
                            CompDwarfHighBlood comp = pawn.TryGetComp<CompDwarfHighBlood>();
                            if (comp == null)
                                Log.Error("Dwarf didn't have high blood comp.");

                            comp.highBloodComp.SetPawn(pawn);
                            comp.highBloodComp.SetHighBlood(RH_TET_DwarfDefOf.RH_TET_Dwarfs_HighBloodKing, false, pawnWasThane, building);

                            // Give all other pawns mood bumps when a new high blood is assigned.
                            List<Pawn> pawnsToCheck = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive;
                            //pawnsToCheck.Remove(pawn);
                            DwarfsUtil.GiveMoodToPawns(RH_TET_DwarfDefOf.RH_TET_Dwarfs_AKingWasCrowned, pawnsToCheck, pawn);
       
                        }
                        
                        if (pawnWasThane && pawnPromotedToKing)
                        {
                            RH_TET_DwarfsMod.king = pawn;
                            RH_TET_DwarfsMod.thanes.Remove(pawn);
                            List<ThingWithComps> removeThrones = thrones.ListFullCopy();

                            foreach (ThingWithComps currThrone in removeThrones)
                            {
                                var currentBuilding = currThrone as Building_DwarfThrone;
                                if (currentBuilding != null)
                                    currentBuilding.CompAssignableToPawn.ForceRemovePawn(pawn);
                            }

                            RH_TET_DwarfsMod.assignedThrones.TryGetValue(pawn).Clear();                            
                        }
                    }
                    else if (building.def.defName.Equals("RH_TET_Dwarfs_Throne"))
                    {
                        if (!pawnWasThane)
                        {
                            CompDwarfHighBlood comp = pawn.TryGetComp<CompDwarfHighBlood>();
                            if (comp == null)
                                Log.Error("Dwarf didn't have high blood comp.");

                            comp.highBloodComp.SetPawn(pawn);
                            comp.highBloodComp.SetHighBlood(RH_TET_DwarfDefOf.RH_TET_Dwarfs_HighBloodThane, false, pawnWasThane, building);

                            // Give all other pawns mood bumps when a new high blood is assigned.
                            List<Pawn> pawnsToCheck = pawn.Map.mapPawns.FreeColonistsSpawned;
                            DwarfsUtil.GiveMoodToPawns(RH_TET_DwarfDefOf.RH_TET_Dwarfs_AThaneWasCrowned, pawnsToCheck, pawn);
                        }
                        
                        RH_TET_DwarfsMod.thanes.Add(pawn);
                    }

                    building.CompAssignableToPawn.ForceAddPawn(pawn);

                    if (thrones != null && !thrones.Contains(this.parent))
                        thrones.Add(this.parent);
                    else
                    {
                        thrones = new List<ThingWithComps>();
                        thrones.Add(this.parent);
                        RH_TET_DwarfsMod.assignedThrones.Add(pawn, thrones);
                    }
                }
                else
                {
                    Messages.Message((TaggedString)(Translator.Translate("RH_TET_Dwarfs_ThaneKingThroneNotDead")), (MessageTypeDef)MessageTypeDefOf.NegativeEvent, true);
                }
            }
        }

        private bool ValidateTryPawnAdd(Pawn pawn)
        {
            if (pawn != null && !DwarfsUtil.IsDwarf(pawn))
            {
                Messages.Message((TaggedString)(Translator.Translate("RH_TET_Dwarfs_LeaderMustBeDwarf")), (MessageTypeDef)MessageTypeDefOf.NegativeEvent, true);
                return false;
            }

            var building = this.parent as Building_DwarfThrone;

            if (building != null)
            {
                Boolean canUnassign = false;
                if (building.AssignedPawn == pawn)
                    return false;
                
                else if (building.AssignedPawn != null)
                    canUnassign = this.ValidateTryUnassignPawn(building.AssignedPawn);

                // Either this throne had no owner, or the unassign step removed it.
                if (building.AssignedPawn == null)
                {
                    // Check for thane/king with no assigned throne.
                    if (!AllHighBloodHaveSeats(building, pawn))
                    {
                        Messages.Message((TaggedString)(Translator.Translate("RH_TET_Dwarfs_ThaneKingWithNoHighSeat")), (MessageTypeDef)MessageTypeDefOf.NegativeEvent, true);
                        return false;
                    }

                    // No king on thane throne.
                    if (building.def.defName.Equals("RH_TET_Dwarfs_Throne") && RH_TET_DwarfsMod.king != null && RH_TET_DwarfsMod.king.Equals(pawn))
                    {
                        Messages.Message((TaggedString)(Translator.Translate("RH_TET_Dwarfs_KingNoSwapThrone")), (MessageTypeDef)MessageTypeDefOf.NegativeEvent, true);
                        return false;
                    }
                }
                else
                {
                    if (!canUnassign) 
                    { 
                        Messages.Message((TaggedString)(Translator.Translate("RH_TET_Dwarfs_ThaneKingThroneNotDead")), (MessageTypeDef)MessageTypeDefOf.NegativeEvent, true);
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        private bool AllHighBloodHaveSeats(Building_DwarfThrone building, Pawn newAssignee)
        {
            // Safe to ignore king, as king will get auto assigned by throne tick.
            //Pawn kingPawn = RH_TET_DwarfsMod.king;

            bool pawnHasNoThrone = false;

            if ((RH_TET_DwarfsMod.king != null && !RH_TET_DwarfsMod.king.Equals(newAssignee)) || building.def.defName.Equals("RH_TET_Dwarfs_Throne"))
            {
                List<Pawn> thaneList = RH_TET_DwarfsMod.thanes;
                
                foreach (Pawn p in thaneList)
                {
                    if (!p.Equals(newAssignee))
                    {
                        if (RH_TET_DwarfsMod.assignedThrones.TryGetValue(p) == null || RH_TET_DwarfsMod.assignedThrones.TryGetValue(p).Count == 0)
                        {
                            pawnHasNoThrone = true;
                            break;
                        }
                    }
                }
            }
            return !pawnHasNoThrone;
        }

        public override void TryUnassignPawn(Pawn pawn, bool sort = true, bool uninstall = false)
        {
            var building = this.parent as Building_DwarfThrone;

            if (this.AssignedPawns == null || this.AssignedPawns.Count() == 0)
                return;

            if (building != null)
            {
                List<ThingWithComps> thrones = RH_TET_DwarfsMod.assignedThrones.TryGetValue(pawn);
                
                // Remove throne from dead pawn.
                if (pawn.Dead)  
                { 
                    if (building.def.defName.Equals("RH_TET_Dwarfs_KingsThrone"))
                    {
                        RH_TET_DwarfsMod.king = null;

                        if (!RH_TET_DwarfsMod.deadHighBloods.Contains(pawn))
                        {
                            RH_TET_DwarfsMod.deadHighBloods.Add(pawn);
                            // Give all other pawns mood issues when a king dies.
                            List<Pawn> pawnsToCheck = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive;
                            DwarfsUtil.GiveMoodToPawns(RH_TET_DwarfDefOf.RH_TET_Dwarfs_DeadKing, pawnsToCheck, pawn);
                        }
                    }
                    else if (building.def.defName.Equals("RH_TET_Dwarfs_Throne"))
                    {
                        RH_TET_DwarfsMod.thanes.Remove(pawn);

                        if (!RH_TET_DwarfsMod.deadHighBloods.Contains(pawn))
                        {
                            RH_TET_DwarfsMod.deadHighBloods.Add(pawn);
                            // Give all other pawns mood issues when a thane dies.
                            List<Pawn> pawnsToCheck = building.Map.mapPawns.FreeColonistsSpawned;
                            //pawnsToCheck.Remove(pawn);
                            DwarfsUtil.GiveMoodToPawns(RH_TET_DwarfDefOf.RH_TET_Dwarfs_DeadThane, pawnsToCheck, pawn);
                        }
                    }

                    if (thrones == null || thrones.Count == 0)
                        return;
                    building.CompAssignableToPawn.ForceRemovePawn(pawn);
                    if (thrones.Contains(this.parent))
                        thrones.Remove(this.parent);
                }
                // Remove pawn from thane throne. 
                else if (building.def.defName.Equals("RH_TET_Dwarfs_Throne") && thrones.Count > 1)
                {
                    if ((RH_TET_DwarfsMod.king != null && !RH_TET_DwarfsMod.king.Equals(pawn)) || (RH_TET_DwarfsMod.king == null))
                    {
                        bool removePawnFromThrone = false;
                        foreach (Building_DwarfThrone currThrone in thrones)
                        {
                            if(building.Map.Equals(currThrone.Map))
                            {
                                removePawnFromThrone = true;
                                break;
                            }
                        }

                        if (removePawnFromThrone)
                        { 
                            RH_TET_DwarfsMod.thanes.Remove(pawn);
                            building.CompAssignableToPawn.ForceRemovePawn(pawn);
                            thrones.Remove(this.parent);
                        }
                        else
                        {
                            Messages.Message((TaggedString)(Translator.Translate("RH_TET_Dwarfs_ThaneSwapThrone")), (MessageTypeDef)MessageTypeDefOf.NegativeEvent, true);
                        }   
                    }
                    else if (RH_TET_DwarfsMod.king != null)
                    {
                        Messages.Message((TaggedString)(Translator.Translate("RH_TET_Dwarfs_KingNoSwapThrone")), (MessageTypeDef)MessageTypeDefOf.NegativeEvent, true);
                    }
                }
                else
                {
                    Messages.Message((TaggedString)(Translator.Translate("RH_TET_Dwarfs_ThaneKingThroneNotDead")), (MessageTypeDef)MessageTypeDefOf.NegativeEvent, true);
                }
            }
        }

        private bool ValidateTryUnassignPawn(Pawn pawn)
        {
            var building = this.parent as Building_DwarfThrone;

            if (this.AssignedPawns == null || this.AssignedPawns.Count() == 0)
                return false;

            if (building != null)
            {
                List<ThingWithComps> thrones = RH_TET_DwarfsMod.assignedThrones.TryGetValue(pawn);

                // Remove throne from dead pawn.
                if (pawn.Dead)
                {
                }
                // Remove pawn from thane throne. 
                else if (building.def.defName.Equals("RH_TET_Dwarfs_Throne") && thrones.Count > 1)
                {
                    if ((RH_TET_DwarfsMod.king != null && !RH_TET_DwarfsMod.king.Equals(pawn)) || (RH_TET_DwarfsMod.king == null))
                    {
                        bool removePawnFromThrone = false;
                        foreach (Building_DwarfThrone currThrone in thrones)
                        {
                            if (building.Map.Equals(currThrone.Map))
                            {
                                removePawnFromThrone = true;
                                break;
                            }
                        }

                        if (!removePawnFromThrone)
                        {
                            Messages.Message((TaggedString)(Translator.Translate("RH_TET_Dwarfs_ThaneSwapThrone")), (MessageTypeDef)MessageTypeDefOf.NegativeEvent, true);
                            return false;
                        }
                    }
                    else if (RH_TET_DwarfsMod.king != null)
                    {
                        Messages.Message((TaggedString)(Translator.Translate("RH_TET_Dwarfs_KingNoSwapThrone")), (MessageTypeDef)MessageTypeDefOf.NegativeEvent, true);
                        return false;
                    }
                }
                else
                {
                    Messages.Message((TaggedString)(Translator.Translate("RH_TET_Dwarfs_ThaneKingThroneNotDead")), (MessageTypeDef)MessageTypeDefOf.NegativeEvent, true);
                    return false;
                }

                return true;
            }

            return false;
        }
    }
}
