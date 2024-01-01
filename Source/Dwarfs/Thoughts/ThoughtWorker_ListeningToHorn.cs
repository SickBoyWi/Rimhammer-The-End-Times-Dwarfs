using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace TheEndTimes_Dwarfs
{
    public class ThoughtWorker_ListeningToHorn : ThoughtWorker_MusicalInstrumentListeningBase
    {
        protected override ThingDef InstrumentDef
        {
            get
            {
                return RH_TET_DwarfDefOf.RH_TET_Dwarfs_Horn;
            }
        }
    }
}