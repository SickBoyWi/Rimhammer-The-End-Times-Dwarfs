using RimWorld;
using System.Collections.Generic;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public struct HighBloodFoodRequirement
    {
        public FoodPreferability minQuality;
        public FoodTypeFlags allowedTypes;
        public List<ThingDef> allowedDefs;

        public bool Defined
        {
            get
            {
                return (uint)this.minQuality > 0U;
            }
        }

        public bool Acceptable(ThingDef food)
        {
            if (food.ingestible == null)
                return false;
            if (this.allowedDefs.Contains(food) || this.allowedTypes != FoodTypeFlags.None && (this.allowedTypes & food.ingestible.foodType) != FoodTypeFlags.None)
                return true;
            return food.ingestible.preferability >= this.minQuality;
        }
    }
}
