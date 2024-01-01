using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace TheEndTimes_Dwarfs
{
    public class CompAbilityEffect_HoldCourt : CompAbilityEffect
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            this.parent.pawn.drafter.Drafted = false;
            this.TryStartHoldCourt(this.parent.pawn);
        }

        public override bool GizmoDisabled(out string reason)
        {
            LordJob_Joinable_Court lordJob = this.parent.pawn.GetLord()?.LordJob as LordJob_Joinable_Court;
            if (lordJob != null && lordJob.Organizer == this.parent.pawn)
            {
                reason = (string)"RH_TET_Dwarfs_AbilityCourtDisabledAlreadyHolding".Translate();
                return true;
            }

            List<ThingWithComps> thrones = RH_TET_DwarfsMod.assignedThrones.TryGetValue(this.parent.pawn);
            Building_DwarfThrone assignedThrone = null;
            if (thrones != null || thrones.Count > 0)
            { 
                foreach(Building_DwarfThrone throne in thrones)
                {
                    if (throne != null && throne.Map.Equals(this.parent.pawn.MapHeld))
                    {
                        assignedThrone = throne;
                        break;
                    }
                }
            }
            if (assignedThrone == null)
            {
                reason = (string)"RH_TET_Dwarfs_AbilityCourtDisabledNoHighSeat".Translate();
                return true;
            }
            if (!this.parent.pawn.CanReserveAndReach((LocalTargetInfo)((Thing)assignedThrone), PathEndMode.InteractionCell, this.parent.pawn.NormalMaxDanger(), 1, -1, (ReservationLayerDef)null, false))
            {
                reason = (string)"RH_TET_Dwarfs_AbilityCourtDisabledNoSeatAvailable".Translate();
                return true;
            }

            CompDwarfHighBlood comp = this.parent.pawn.TryGetComp<CompDwarfHighBlood>();
            if (comp == null)
            {
                Log.Error("Dwarf has null high blood comp.");
                reason = "Dwarf has null high blood comp.";
                return true;
            }

            if (comp.highBloodComp.GetUnmetThroneroomRequirements(this.parent.pawn, true, false).Any<string>())
            {
                reason = (string)"RH_TET_Dwarfs_AbilityCourtDisabledInvalidHighRoom".Translate();
                return true;
            }
            reason = (string)null;
            return false;
        }
        
        public bool TryStartHoldCourt(Pawn pawn)
        {
            CompDwarfHighBlood comp = this.parent.pawn.TryGetComp<CompDwarfHighBlood>();
            if (comp.highBloodComp.GetUnmetThroneroomRequirements(this.parent.pawn, true, false).Any<string>())
                return false;
            if (!RH_TET_DwarfDefOf.RH_TET_Dwarf_HighBloodCourt.CanExecute(pawn.Map, pawn, true))
                return false;
            RH_TET_DwarfDefOf.RH_TET_Dwarf_HighBloodCourt.Worker.TryExecute(pawn.Map, pawn);
            return true;
        }
    }
}
