using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using HarmonyLib;

namespace TheEndTimes_Dwarfs
{

    public class CompDwarfHighBlood : ThingComp
    {
        private CompProperties_DwarfHighBlood Props => (CompProperties_DwarfHighBlood)props;
        private Pawn Pawn => (Pawn)parent;
        public DwarfPawn_HighBloodTracker highBloodComp;
        
        public CompDwarfHighBlood()
        {
            highBloodComp = new DwarfPawn_HighBloodTracker(Pawn);
        }

        public CompDwarfHighBlood(DwarfPawn_HighBloodTracker highBloodP)
        {
            if (highBloodComp != null)
                this.highBloodComp = highBloodP;
            else
                highBloodComp = new DwarfPawn_HighBloodTracker(Pawn);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look<DwarfPawn_HighBloodTracker>(ref this.highBloodComp, "highBloodComp", this.Pawn);
        }

        public override void CompTick()
        {
            base.CompTick();
            if (this.highBloodComp != null && this.highBloodComp.GetCurrentHighBlood() != null && Pawn.IsColonist && !Pawn.Dead && DwarfsUtil.IsDwarf(Pawn))
                highBloodComp.HighBloodTrackerTick();
        }

        // If there are any king/thanes, this code will set up the mod class correctly.
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            if (this.highBloodComp != null)
            {
                this.highBloodComp.SetPawn(Pawn);

                if (this.highBloodComp.GetCurrentHighBlood() != null)
                {
                    if (this.highBloodComp.GetCurrentHighBlood().Equals(RH_TET_DwarfDefOf.RH_TET_Dwarfs_HighBloodKing))
                    {
                        RH_TET_DwarfsMod.king = Pawn;
                    }
                    else if (this.highBloodComp.GetCurrentHighBlood().Equals(RH_TET_DwarfDefOf.RH_TET_Dwarfs_HighBloodThane))
                    {
                        RH_TET_DwarfsMod.thanes.Add(Pawn);
                    }
                }
            }
        }
    }
}
