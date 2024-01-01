using RimWorld;
using System.Collections.Generic;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class RoomRequirement_AllLit : RoomRequirement
    {
        public ThingDef thingDef;

        public override bool Met(Room r, Pawn p)
        {
            foreach (Thing containedThing in r.ContainedThings(this.thingDef))
            {
                if (!containedThing.TryGetComp<CompGlower>().Glows)
                    return false;
            }
            return true;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if (this.thingDef == null)
                yield return "thingDef is null";
            else if (this.thingDef.GetCompProperties<CompProperties_Glower>() == null)
                yield return "No comp glower on thingDef";
        }
    }
}
