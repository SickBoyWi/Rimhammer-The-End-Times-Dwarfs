using System;
using System.Runtime.InteropServices;
using RimWorld;
using Verse;
using System.Collections.Generic;
using TheEndTimes_Magic;

namespace TheEndTimes_Dwarfs
{
    public class Building_InfestationLure : Building
    {
        private CompFlickable flickComp;
        private CompSteamPipe pipeComp;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.flickComp = this.GetComp<CompFlickable>();
            this.pipeComp = this.GetComp<CompSteamPipe>();
        }
    }
}