using RimWorld;
using Verse;
using Verse.AI;

namespace TheEndTimes_Dwarfs
{
    public class JobGiver_MaintainTunnels : JobGiver_AIFightEnemies
    {
        private static readonly float CellsInScanRadius = (float)GenRadial.NumCellsInRadius(7.9f);
        private bool onlyIfDamagingState;

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_MaintainTunnels giverMaintainTunnels = (JobGiver_MaintainTunnels)base.DeepCopy(resolve);
            giverMaintainTunnels.onlyIfDamagingState = this.onlyIfDamagingState;
            return (ThinkNode)giverMaintainTunnels;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            Room room = pawn.GetRoom(RegionType.Set_Passable);
            for (int index = 0; (double)index < (double)JobGiver_MaintainTunnels.CellsInScanRadius; ++index)
            {
                IntVec3 intVec3 = pawn.Position + GenRadial.RadialPattern[index];
                if (intVec3.InBounds(pawn.Map) && intVec3.GetRoom(pawn.Map) == room)
                {
                    SkavenTunnel thing = (SkavenTunnel)pawn.Map.thingGrid.ThingAt(intVec3, RH_TET_DwarfDefOf.RH_TET_SkavenTunnel);
                    if (thing != null && pawn.CanReserve((LocalTargetInfo)((Thing)thing), 1, -1, (ReservationLayerDef)null, false))
                    {
                        CompMaintainable comp = thing.TryGetComp<CompMaintainable>();
                        if (comp.CurStage != MaintainableStage.Healthy && (!this.onlyIfDamagingState || comp.CurStage == MaintainableStage.Damaging))
                            return JobMaker.MakeJob(JobDefOf.Maintain, (LocalTargetInfo)((Thing)thing));
                    }
                }
            }
            return (Job)null;
        }
    }
}
