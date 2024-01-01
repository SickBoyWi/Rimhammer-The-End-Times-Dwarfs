using RimWorld;
using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace TheEndTimes_Dwarfs
{
    public class LordToil_HoldCourt : LordToil_Gathering
    {
        public Pawn organizer;

        public new LordToilData_HoldCourt Data
        {
            get
            {
                return (LordToilData_HoldCourt)this.data;
            }
        }

        public LordToil_HoldCourt(IntVec3 spot, GatheringDef gatheringDef, Pawn organizer)
          : base(spot, gatheringDef)
        {
            this.spot = spot;
            this.organizer = organizer;
            this.data = (LordToilData)new LordToilData_HoldCourt();
        }

        public override void Init()
        {
            base.Init();
            this.Data.spectateRect = CellRect.CenteredOn(this.spot, 0);
            Rot4 rotation = this.spot.GetFirstThing<Building_DwarfThrone>(this.organizer.MapHeld).Rotation;
            this.Data.spectateRectAllowedSides = SpectateRectSide.All & ~rotation.Opposite.AsSpectateSide;
            this.Data.spectateRectPreferredSide = rotation.AsSpectateSide;
        }

        public override ThinkTreeDutyHook VoluntaryJoinDutyHookFor(Pawn p)
        {
            if (p == this.organizer)
                return RH_TET_DwarfDefOf.RH_TET_Dwarfs_HoldCourtDuty.hook;
            return RH_TET_DwarfDefOf.RH_TET_Dwarfs_HoldCourtDuty.hook;
        }

        public override void UpdateAllDuties()
        {
            for (int index = 0; index < this.lord.ownedPawns.Count; ++index)
            {
                Pawn ownedPawn = this.lord.ownedPawns[index];
                if (ownedPawn == this.organizer)
                {
                    Building_DwarfThrone firstThing = this.spot.GetFirstThing<Building_DwarfThrone>(this.Map);
                    ownedPawn.mindState.duty = new PawnDuty(RH_TET_DwarfDefOf.RH_TET_Dwarfs_HoldCourtDuty, (LocalTargetInfo)this.spot, (LocalTargetInfo)((Thing)firstThing), -1f);
                    ownedPawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true, true);
                }
                else
                    ownedPawn.mindState.duty = new PawnDuty(RH_TET_DwarfDefOf.RH_TET_Dwarfs_ObserveCourtDuty)
                    {
                        spectateRect = this.Data.spectateRect,
                        spectateRectAllowedSides = this.Data.spectateRectAllowedSides,
                        spectateRectPreferredSide = this.Data.spectateRectPreferredSide
                    };
            }
        }
    }
}
