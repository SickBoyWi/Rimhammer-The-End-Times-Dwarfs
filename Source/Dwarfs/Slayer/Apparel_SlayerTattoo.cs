using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class Apparel_SlayerTattoos : Apparel
    {
        public new bool PawnCanWear(Pawn pawn, bool ignoreGender = false)
        {
            return this.def.IsApparel && DwarfsUtil.IsSlayer(pawn) && this.def.apparel.PawnCanWear(pawn, ignoreGender);
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }
    }
}
