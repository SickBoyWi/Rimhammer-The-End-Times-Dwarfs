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

    public class CompProperties_DwarfHighBlood : CompProperties
    {
        public CompProperties_DwarfHighBlood()
        {
            compClass = typeof(CompDwarfHighBlood);
        }
    }
}
