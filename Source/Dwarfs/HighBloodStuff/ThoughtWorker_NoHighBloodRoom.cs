using RimWorld;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class ThoughtWorker_NoHighBloodRoom : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            CompDwarfHighBlood comp = p.TryGetComp<CompDwarfHighBlood>();

            if (comp == null || comp.highBloodComp == null || comp.highBloodComp.GetCurrentHighBlood() == null || p.MapHeld == null || (!p.MapHeld.IsPlayerHome || comp.highBloodComp.HighestHighBloodWithThroneRoomRequirements() == null))
                return (ThoughtState)false;
            
            bool noThroneRoom = true;

            // If throne.
            if (!(RH_TET_DwarfsMod.assignedThrones.TryGetValue(p) == null || RH_TET_DwarfsMod.assignedThrones.TryGetValue(p).Count == 0))
            {
                foreach (Building_DwarfThrone throne in RH_TET_DwarfsMod.assignedThrones.TryGetValue(p))
                {
                    Room room = throne.GetRoom(RegionType.Set_Passable);
                    if (room != null && !room.PsychologicallyOutdoors)
                    {
                        noThroneRoom = false;
                        break;
                    }
                }
            }

            return (ThoughtState)noThroneRoom;
        }
    }
}