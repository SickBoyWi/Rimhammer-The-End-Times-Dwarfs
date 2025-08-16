using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class Building_PlanterGrower : Building_PlantGrower
    {
        public override string GetInspectString()
        {
            string str = base.GetInspectString();
            if (this.Spawned)
            {
                if (!str.NullOrEmpty())
                    str += "\n";
                str = !PlantUtility.GrowthSeasonNow(this.Position, this.Map, this.GetPlantDefToGrow()) ? (string)(str + "CannotGrowBadSeasonTemperature".Translate()) : (string)(str + "GrowSeasonHereNow".Translate());
            }
            return str;
        }
    }
}
