using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class CapturedHoldComp : WorldObjectComp
    {
        public CapturedHoldSite Parent;

        public CapturedHoldComp()
        {
            this.Parent = (CapturedHoldSite)this.parent;
        }

        public override void CompTick()
        {
            base.CompTick();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            // TODO could make some more info here, but doesn't really matter I think.
            //stringBuilder.Append("CompInspectStringExtra MESSAGE TEST.");
            return stringBuilder.ToString();
        }
    }
}
