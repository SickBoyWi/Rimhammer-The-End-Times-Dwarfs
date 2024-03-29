﻿using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class Building_PlanterGrower : Building_PlantGrower
    {
        public override string GetInspectString()
        {
            string str = "";
            if (this.Spawned)
                str = !PlantUtility.GrowthSeasonNow(this.Position, this.Map, true) ? (string)(str + ("CannotGrowBadSeasonTemperature".Translate())) : (string)(str + ("GrowSeasonHereNow".Translate()));
            return str;
        }
    }
}
