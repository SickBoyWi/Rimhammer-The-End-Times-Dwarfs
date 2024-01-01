using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TheEndTimes_Dwarfs
{
    public static class RH_TET_Dwarfs_GameVictoryUtility
    {
        private static void ShowCredits(string victoryText)
        {
            Screen_Credits screen_Credits = new Screen_Credits(victoryText);
            screen_Credits.wonGame = true;
            Find.WindowStack.Add(screen_Credits);
            Find.MusicManagerPlay.ForceSilenceFor(999f);
            ScreenFader.StartFade(Color.clear, 3f);
        }

        private static void InitiateVictory()
        {
            RH_TET_DwarfDefOf.RH_TET_Dwarf_Victory.PlayOneShotOnCamera(null);
            ScreenFader.StartFade(Color.white, 5.0f);
        }

        public static void AncientHoldRecaptured()
        {
            InitiateVictory();
            string victoryText = "RH_TET_AncientDwarvenHoldReclaimed".Translate();
            ShowCredits(victoryText);
        }
    }
}
