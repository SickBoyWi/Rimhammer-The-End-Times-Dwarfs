using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TheEndTimes_Dwarfs
{
    public class Building_DetonatorManual : Building, IGraphicVariantProvider, IPawnDetonateable
    {
        private const float PlungerDownTime = 0.25f;
        private Building_DetonatorManual.VisualVariant currentVariant;
        private float plungerExpireTime;
        private bool wantDetonation;

        public int GraphicVariant
        {
            get
            {
                return (int)this.currentVariant;
            }
        }

        public bool UseInteractionCell
        {
            get
            {
                return false;
            }
        }

        public bool WantsDetonation
        {
            get
            {
                return this.wantDetonation;
            }
            set
            {
                this.wantDetonation = value;
            }
        }

        public void DoDetonation()
        {
            this.wantDetonation = false;
            this.currentVariant = Building_DetonatorManual.VisualVariant.PlungerDown;
            this.plungerExpireTime = Time.realtimeSinceStartup + 0.2f;
            RH_TET_DwarfDefOf.RH_TET_Dwarfs_DetonatorLever.PlayOneShot((SoundInfo)new TargetInfo(this.Position, this.Map, false));
            CompWiredBlasterSender comp = this.GetComp<CompWiredBlasterSender>();
            if (comp == null)
                return;
            comp.SendNewSignal();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.wantDetonation, "wantDetonation", false, false);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            Command detonate;
            if (this.CanDetonateImmediately())
            {
                Command_Action commandAction = new Command_Action();
                commandAction.action = new Action(this.DoDetonation);
                commandAction.defaultLabel = "RH_TET_Dwarfs_BlastNow".Translate();
                detonate = (Command)commandAction;
            }
            else
            {
                Command_Toggle commandToggle = new Command_Toggle();
                commandToggle.toggleAction = new Action(this.DetonateToggleAction);
                commandToggle.isActive = (Func<bool>)(() => this.wantDetonation);
                commandToggle.defaultLabel = "RH_TET_Dwarfs_Blast".Translate();
                detonate = (Command)commandToggle;
            }
            detonate.icon = RH_TET_DwarfDefOf.Textures.CommandButton_DetonateManualUI;
            detonate.defaultDesc = "RH_TET_Dwarfs_BlastDesc".Translate();
            yield return (Gizmo)detonate;
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                Gizmo g = gizmo;
                yield return g;
                g = (Gizmo)null;
            }
        }

        public override void Draw()
        {
            if ((double)this.plungerExpireTime < (double)Time.realtimeSinceStartup)
            {
                this.currentVariant = Building_DetonatorManual.VisualVariant.PlungerUp;
                this.plungerExpireTime = 0.0f;
            }
            base.Draw();
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(
          Pawn selPawn)
        {
            FloatMenuOption opt = BlastingUtility.TryMakeDetonatorFloatMenuOption(selPawn, (IPawnDetonateable)this);
            if (opt != null)
                yield return opt;
            foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(selPawn))
            {
                FloatMenuOption option = floatMenuOption;
                yield return option;
                option = (FloatMenuOption)null;
            }
        }

        public bool RedButtonFeverCanInteract
        {
            get
            {
                return true;
            }
        }

        public void RedButtonFeverDoInteraction(Pawn p)
        {
            this.DoDetonation();
        }

        private void DetonateToggleAction()
        {
            this.wantDetonation = !this.wantDetonation;
        }

        private bool CanDetonateImmediately()
        {
            for (int index = 0; index < GenAdj.AdjacentCellsAround.Length; ++index)
            {
                Pawn firstPawn = (this.Position + GenAdj.AdjacentCellsAround[index]).GetFirstPawn(this.Map);
                if (firstPawn != null && firstPawn.Drafted)
                    return true;
            }
            return false;
        }

        private enum VisualVariant
        {
            PlungerUp,
            PlungerDown,
        }
    }
}
