using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace TheEndTimes_Dwarfs
{
    public class MentalStateWorker_LonelyGrumbling : MentalStateWorker
    {
        public override bool StateCanOccur(Pawn pawn)
        {
            if (!base.StateCanOccur(pawn))
                return false;
            
            if (!DwarfsUtil.IsDwarf(pawn))
                return false;
            
            Building_Bed ownedBed = pawn.ownership.OwnedBed;

            return ownedBed != null && ownedBed.GetRoom(RegionType.Set_Passable) != null && !ownedBed.GetRoom(RegionType.Set_Passable).PsychologicallyOutdoors;
        }
    }
}
