using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class Building_DwarfThrone : Building
    {
        public bool giftGranted = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.giftGranted, "giftGranted", false);
        }

        public Pawn AssignedPawn
        {
            get
            {
                if (this.CompAssignableToPawn == null || !this.CompAssignableToPawn.AssignedPawnsForReading.Any<Pawn>())
                    return (Pawn)null;
                return this.CompAssignableToPawn.AssignedPawnsForReading[0];
            }
        }

        public CompAssignableToPawn_Throne CompAssignableToPawn
        {
            get
            {
                return this.GetComp<CompAssignableToPawn_Throne>();
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            
            // If there's any thrones, this will set up the code in the mod class right.
            if (this.def.defName.Equals("RH_TET_Dwarfs_KingsThrone"))
            {
                if (this.AssignedPawn != null)
                {
                    List<ThingWithComps> thrones = RH_TET_DwarfsMod.assignedThrones.TryGetValue(this.AssignedPawn);
                    if (thrones == null)
                    {
                        thrones = new List<ThingWithComps>();
                        thrones.Add(this);
                        RH_TET_DwarfsMod.assignedThrones.Add(this.AssignedPawn, thrones);
                    }
                    if (!thrones.Contains(this))
                        thrones.Add(this);
                }
            }
            else if (this.def.defName.Equals("RH_TET_Dwarfs_Throne"))
            {
                if (this.AssignedPawn != null)
                {
                    List<ThingWithComps> thrones = RH_TET_DwarfsMod.assignedThrones.TryGetValue(this.AssignedPawn);
                    if (thrones == null)
                    {
                        thrones = new List<ThingWithComps>();
                        thrones.Add(this);
                        RH_TET_DwarfsMod.assignedThrones.Add(this.AssignedPawn, thrones);
                    }
                    else if (!thrones.Contains(this))
                        thrones.Add(this);
                }
            }
        }
        
        public override string GetInspectString()
        {
            string inspectString = base.GetInspectString();
            Room room = this.GetRoom(RegionType.Set_Passable);
            Pawn p = this.CompAssignableToPawn.AssignedPawnsForReading.Count == 1 ? this.CompAssignableToPawn.AssignedPawnsForReading[0] : (Pawn)null;
            HighBloodDef highBloodDef = RH_TET_DwarfDefOf.RH_TET_Dwarfs_HighBloodKing;
            if (this.def.Equals(RH_TET_DwarfDefOf.RH_TET_Dwarfs_Throne))
                highBloodDef = RH_TET_DwarfDefOf.RH_TET_Dwarfs_HighBloodThane;
            string str1 = (string)(inspectString + ("\n" + "RH_TET_Dwarfs_HighBloodSeatDesc".Translate((NamedArgument)(highBloodDef == null ? "None".Translate() : highBloodDef.GetLabelCapFor(p) + " " + "RH_TET_Dwarfs_HighBloodRulingRoomQuality".Translate((NamedArgument)highBloodDef.MinThroneRoomQuality.ToString())))));
            string str2 = RoomRoleWorker_DwarfRulingRoom.Validate(room);
            if (str2 != null)
                return str1 + "\n" + str2;
            return str1;
        }
        
        public override IEnumerable<Gizmo> GetGizmos()
        {
            Building_DwarfThrone buildingThrone = this;
            foreach (Gizmo gizmo in base.GetGizmos())
                yield return gizmo;
        }

        protected override void Tick()
        {
            base.Tick();
            if (this.def.defName.Equals("RH_TET_Dwarfs_KingsThrone"))
            {
                // See if any other king thrones exist on any player maps, if so, find the ones with assigned pawns, and set this one to match. 
                // The assign/unassign logic will prevent players from messing things up.
                if (this.AssignedPawn == null && RH_TET_DwarfsMod.king != null)
                {
                    IEnumerable<Map> maps =  Find.Maps.Where<Map>((Func<Map, bool>)(x => x.IsPlayerHome));
                    this.CompAssignableToPawn.TryAssignPawn(RH_TET_DwarfsMod.king);
                }
            }
            // If a pawn has died, handle it.
            if (this.AssignedPawn != null && this.AssignedPawn.Dead)
            {
                this.GetComp<CompAssignableToPawn_Throne>().TryUnassignPawn(this.AssignedPawn);
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (this.AssignedPawn != null && !this.AssignedPawn.Dead)
            {
                Pawn assPawn = this.AssignedPawn;
                List<ThingWithComps> thrones = RH_TET_DwarfsMod.assignedThrones.TryGetValue(assPawn);
                
                if (thrones != null && thrones.Count > 0)
                {
                    thrones.Remove(this);
                }
                
                this.CompAssignableToPawn.ForceRemovePawn(assPawn);

                if (RH_TET_DwarfsMod.assignedThrones.TryGetValue(assPawn) == null || RH_TET_DwarfsMod.assignedThrones.TryGetValue(assPawn).Count == 0)
                    Messages.Message((TaggedString)(Translator.Translate("RH_TET_Dwarfs_ThroneDestroyed", assPawn.Name)), (MessageTypeDef)MessageTypeDefOf.NegativeEvent, true);
                    
            }

            base.Destroy(mode);
        }
    }
}
