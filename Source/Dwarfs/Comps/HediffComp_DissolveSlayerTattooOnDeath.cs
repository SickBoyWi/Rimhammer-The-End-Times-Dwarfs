using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TheEndTimes_Dwarfs
{
    public class HediffComp_DissolveSlayerTattooOnDeath : HediffComp
    {
        public HediffCompProperties_DissolveSlayerTattooOnDeath Props
        {
            get
            {
                return (HediffCompProperties_DissolveSlayerTattooOnDeath)this.props;
            }
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            base.Notify_PawnDied(dinfo, culprit);
        }

        public override void Notify_PawnKilled()
        {
            List<Apparel> apparelToDestroy = new List<Apparel>();

            foreach(Apparel a in this.Pawn.apparel.WornApparel)
            {
                if (a != null && a.def != null && a.def.defName.Contains("Slayer_Tattoo"))
                    apparelToDestroy.Add(a);
            }

            foreach (Apparel a in apparelToDestroy)
                a.Destroy(DestroyMode.Vanish);

            if (!this.Pawn.Spawned)
                return;

            if (this.Props.mote != null || this.Props.fleck != null)
            {
                Vector3 drawPos = this.Pawn.DrawPos;
                for (int index = 0; index < this.Props.moteCount; ++index)
                {
                    Vector2 vector2 = Rand.InsideUnitCircle * this.Props.moteOffsetRange.RandomInRange * (float)Rand.Sign;
                    Vector3 loc = new Vector3(drawPos.x + vector2.x, drawPos.y, drawPos.z + vector2.y);
                    if (this.Props.mote != null)
                        MoteMaker.MakeStaticMote(loc, this.Pawn.Map, this.Props.mote, 1f, false, 0.0f);
                    else
                        FleckMaker.Static(loc, this.Pawn.Map, this.Props.fleck, 1f);
                }
            }
            if (this.Props.sound == null)
                return;
            this.Props.sound.PlayOneShot(SoundInfo.InMap((TargetInfo)(Thing)this.Pawn, MaintenanceType.None));
        }
    }
}
