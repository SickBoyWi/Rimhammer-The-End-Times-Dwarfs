using RimWorld;
using System.Collections.Generic;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class RecipeWorkerCounter_MakeStoneWool : RecipeWorkerCounter
    {
        public override bool CanCountProducts(Bill_Production bill)
        {
            return true;
        }

        public override int CountProducts(Bill_Production bill)
        {
            int num = 0;
            List<ThingDef> childThingDefs = RH_TET_DwarfDefOf.RH_TET_Dwarfs_StoneWools.childThingDefs;
            for (int index = 0; index < childThingDefs.Count; ++index)
                num += bill.Map.resourceCounter.GetCount(childThingDefs[index]);
            return num;
        }

        public override string ProductsDescription(Bill_Production bill)
        {
            return RH_TET_DwarfDefOf.RH_TET_Dwarfs_StoneWools.label;
        }

        public override bool CanPossiblyStore(Bill_Production bill, ISlotGroup slotGroup)
        {
            foreach (ThingDef allowedThingDef in bill.ingredientFilter.AllowedThingDefs)
            {
                if (!allowedThingDef.butcherProducts.NullOrEmpty<ThingDefCountClass>())
                {
                    ThingDef thingDef = allowedThingDef.butcherProducts[0].thingDef;
                    if (!slotGroup.Settings.AllowedToAccept(thingDef))
                        return false;
                }
            }
            return true;
        }
    }
}
