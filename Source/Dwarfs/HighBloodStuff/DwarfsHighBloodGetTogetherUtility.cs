using RimWorld;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public static class DwarfsHighBloodGetTogetherUtility
    {
        public static bool AcceptableGameConditionsToStartGathering(Map map)
        {
            return GatheringsUtility.AcceptableGameConditionsToContinueGathering(map) 
                && GenLocalDate.HourInteger(map) >= 6 
                && (GenLocalDate.HourInteger(map) <= 22 && !GatheringsUtility.AnyLordJobPreventsNewGatherings(map));
        }
    }
}