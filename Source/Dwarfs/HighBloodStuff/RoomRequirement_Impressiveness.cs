using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class RoomRequirement_Quality : RoomRequirement
    {
        public int impressiveness;

        public override string Label(Room r = null)
        {
            return (string)((!this.labelKey.NullOrEmpty() ? this.labelKey : "RH_TET_Dwarfs_RoomRequirementQuality").Translate() + " " + (r != null ? ((double)Mathf.Round(r.GetStat(RoomStatDefOf.Impressiveness))).ToString() + "/" : "")) + (object)this.impressiveness;
        }

        public override bool Met(Room r, Pawn p)
        {
            return (double)r.GetStat(RoomStatDefOf.Impressiveness) >= (double)this.impressiveness;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string configError in base.ConfigErrors())
                yield return configError;
            if (this.impressiveness <= 0)
                yield return "impressiveness must be larger than 0";
        }
    }
}
