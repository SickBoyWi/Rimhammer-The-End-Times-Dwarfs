using RimWorld;
using System.Collections.Generic;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class RoomRequirement_Size : RoomRequirement
    {
        public int area;

        public override string Label(Room r = null)
        {
            return (string)(!this.labelKey.NullOrEmpty() ? this.labelKey : "RH_TET_Dwarfs_RoomRequirementSize").Translate((NamedArgument)((r != null ? (object)(r.CellCount.ToString() + "/") : (object)"").ToString() + (object)this.area));
        }

        public override bool Met(Room r, Pawn p)
        {
            return r.CellCount >= this.area;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string configError in base.ConfigErrors())
                yield return configError;
            if (this.area <= 0)
                yield return "area must be larger than 0";
        }
    }
}
