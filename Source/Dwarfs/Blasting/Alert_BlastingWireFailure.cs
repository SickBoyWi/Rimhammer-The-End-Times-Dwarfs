using RimWorld;
using RimWorld.Planet;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class Alert_BlastingWireFailure : Alert_Critical
    {
        private const float AutoExpireInSeconds = 8f;
        private static Alert_BlastingWireFailure instance;
        private int expireTick;
        private Fire wireFire;

        public static Alert_BlastingWireFailure Instance
        {
            get
            {
                return Alert_BlastingWireFailure.instance ?? (Alert_BlastingWireFailure.instance = new Alert_BlastingWireFailure());
            }
        }

        public Alert_BlastingWireFailure()
        {
            Alert_BlastingWireFailure.instance = this;
        }

        public void ReportFailure(Fire createdFire)
        {
            this.expireTick = (int)((double)Find.TickManager.TicksGame + 480.0);
            this.wireFire = createdFire;
        }

        public override string GetLabel()
        {
            return "RH_TET_Dwarfs_WireFailureTitle".Translate();
        }

        public override TaggedString GetExplanation()
        {
            return "RH_TET_Dwarfs_WireFailure".Translate();
        }

        public override AlertReport GetReport()
        {
            bool flag = this.wireFire != null && this.wireFire.Spawned;
            if (flag || this.expireTick > Find.TickManager.TicksGame)
                return flag ? AlertReport.CulpritIs((GlobalTargetInfo)((Verse.Thing)this.wireFire)) : AlertReport.Active;
            return (AlertReport)false;
        }
    }
}
