using System.Collections.Generic;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class PlaceWorker_BlastingWire : PlaceWorker
    {
        private readonly System.Type compTypeTransmitter = typeof(CompWiredBlasterTransmitter);

        public override AcceptanceReport AllowsPlacing(
          BuildableDef checkingDef,
          IntVec3 loc,
          Rot4 rot,
          Map map,
          Verse.Thing thingToIgnore = null,
          Thing thing = null)
        {
            List<Verse.Thing> thingList = loc.GetThingList(map);
            for (int index = 0; index < thingList.Count; ++index)
            {
                Verse.Thing thingFromList = thingList[index];
                if (thingFromList.def == null)
                    return (AcceptanceReport)false;
                if (thingFromList.def.HasComp(this.compTypeTransmitter))
                    return (AcceptanceReport)false;
                if (thingFromList.def.entityDefToBuild != null)
                {
                    ThingDef entityDefToBuild = thingList[index].def.entityDefToBuild as ThingDef;
                    if (entityDefToBuild != null && (entityDefToBuild.HasComp(this.compTypeTransmitter)))
                        return (AcceptanceReport)false;
                }
            }
            return (AcceptanceReport)true;
        }
    }
}
