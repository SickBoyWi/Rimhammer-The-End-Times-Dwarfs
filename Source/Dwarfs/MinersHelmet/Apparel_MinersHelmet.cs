using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class Apparel_MinersHelmet : Apparel
    {
        public const int updatePeriodInTicks = 60;
        public int nextUpdateTick;
        public Thing light;
        public bool lightIsOn;
        public bool needSynchronization;
        public Apparel_MinersHelmet.LightMode lightMode;
        
        public Apparel_MinersHelmet()
            : base()
        {
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.needSynchronization = true;
            if (respawningAfterLoad)
                return;
            this.nextUpdateTick = Find.TickManager.TicksGame + Rand.Range(0, 60);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
            
            if (!this.light.DestroyedOrNull())
            {
                this.light.Destroy(DestroyMode.Vanish);
                this.light = (Thing)null;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Thing>(ref this.light, "light", false);
            Scribe_Values.Look<bool>(ref this.lightIsOn, "lightIsOn", false, false);
            Scribe_Values.Look<Apparel_MinersHelmet.LightMode>(ref this.lightMode, "lightMode", Apparel_MinersHelmet.LightMode.Automatic, false);
            Scribe_Values.Look<int>(ref this.nextUpdateTick, "nextUpdateTick", 0, false);
            Scribe_Values.Look<bool>(ref this.needSynchronization, "needSynchronization", false, false);
        }

        public override void Tick()
        {
            base.Tick();
            if (!this.lightIsOn && Find.TickManager.TicksGame < this.nextUpdateTick)
                return;
            this.nextUpdateTick = Find.TickManager.TicksGame + 60;
            if (this.needSynchronization && this.Wearer != null)
            {
                this.SynchronizeLightMode();
                this.needSynchronization = false;
            }
            this.RefreshLightState();
        }

        // Called once, when first spawned. Sets light mode appropriately when reloading.
        public void SynchronizeLightMode()
        {
            for (int index = 0; index < ((Pawn_ApparelTracker)this.Wearer.apparel).WornApparelCount; ++index)
            {
                if (((Pawn_ApparelTracker)this.Wearer.apparel).WornApparel[index] is Apparel_MinersHelmet)
                    (((Pawn_ApparelTracker)this.Wearer.apparel).WornApparel[index] as Apparel_MinersHelmet).lightMode = this.lightMode;
            }
        }

        public void RefreshLightState()
        {
            if (this.ComputeLightState())
                this.SwitchOnLight();
            else
                this.SwitchOffLight();

            if (this.Wearer == null || this.Wearer.Dead)
            {
                if (!this.light.DestroyedOrNull())
                {
                    this.light.Destroy(DestroyMode.Vanish);
                    this.light = (Thing)null;
                }
            }
        }

        // Determines on or off status. Return true if it needs to be on, false if it needs to off.
        public bool ComputeLightState()
        {
            return this.Wearer != null
                && !this.Wearer.Dead
                && (!this.Wearer.Downed && RestUtility.Awake(this.Wearer)) 
                && (this.lightMode == Apparel_MinersHelmet.LightMode.ForcedOn 
                    || this.lightMode != Apparel_MinersHelmet.LightMode.ForcedOff 
                    && this.Wearer.Map != null 
                    && (GridsUtility.Roofed(this.Wearer.Position, this.Wearer.Map) 
                        && this.Wearer.Map.glowGrid.PsychGlowAt(this.Wearer.Position) <= PsychGlow.Lit 
                        || !GridsUtility.Roofed(this.Wearer.Position, this.Wearer.Map) 
                        && this.Wearer.Map.glowGrid.PsychGlowAt(this.Wearer.Position) < PsychGlow.Overlit));
        }

        public void SwitchOnLight()
        {
            //Log.Error("LightOn:" + this.def.defName + "," + (this.Wearer != null ? this.Wearer.NameFullColored.ToString() : ""));
            IntVec3 intVec3 = this.Wearer.DrawPos.ToIntVec3();
            if (!this.light.DestroyedOrNull() && intVec3 != this.light.Position)
                this.SwitchOffLight();
            if (this.light.DestroyedOrNull() && intVec3.GetFirstThing(this.Wearer.Map, RH_TET_DwarfDefOf.RH_TET_Dwarf_MiningHelmetLight) == null)
                this.light = GenSpawn.Spawn(RH_TET_DwarfDefOf.RH_TET_Dwarf_MiningHelmetLight, intVec3, this.Wearer.Map, WipeMode.Vanish);
            this.lightIsOn = true;
        }

        public void SwitchOffLight()
        {
            if (!this.light.DestroyedOrNull())
            {
                this.light.Destroy(DestroyMode.Vanish);
                this.light = (Thing)null;
            }
            this.lightIsOn = false;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            IEnumerable<Gizmo> wornGizmos = this.GetWornGizmos();
            IEnumerable<Gizmo> gizmos = base.GetGizmos();
            return gizmos == null ? wornGizmos : gizmos.Concat<Gizmo>(wornGizmos);
        }

        public override IEnumerable<Gizmo> GetWornGizmos()
        {
            IList<Gizmo> gizmoList = (IList<Gizmo>)new List<Gizmo>();
            int num = 700000101;
            Command_Action commandAction = new Command_Action();
            switch (this.lightMode)
            {
                case Apparel_MinersHelmet.LightMode.Automatic:
                    commandAction.icon = ContentFinder<Texture2D>.Get("UI/Commands/CommandButton_LigthModeAutomatic", true);
                    commandAction.defaultLabel = "Light: automatic.";
                    break;
                case Apparel_MinersHelmet.LightMode.ForcedOn:
                    commandAction.icon = ContentFinder<Texture2D>.Get("UI/Commands/CommandButton_LightModeForcedOn", true);
                    commandAction.defaultLabel = "Light: on.";
                    break;
                case Apparel_MinersHelmet.LightMode.ForcedOff:
                    commandAction.icon = ContentFinder<Texture2D>.Get("UI/Commands/CommandButton_LightModeForcedOff", true);
                    commandAction.defaultLabel = "Light: off.";
                    break;
            }
            commandAction.defaultDesc = "Switch mode.";
            commandAction.activateSound = SoundDef.Named("Click");
            commandAction.action = new Action(this.SwitchLigthMode);
            commandAction.groupKey = num + 1;
            gizmoList.Add((Gizmo)commandAction);
            return (IEnumerable<Gizmo>)gizmoList;
        }

        public void SwitchLigthMode()
        {
            switch (this.lightMode)
            {
                case Apparel_MinersHelmet.LightMode.Automatic:
                    this.lightMode = Apparel_MinersHelmet.LightMode.ForcedOn;
                    break;
                case Apparel_MinersHelmet.LightMode.ForcedOn:
                    this.lightMode = Apparel_MinersHelmet.LightMode.ForcedOff;
                    break;
                case Apparel_MinersHelmet.LightMode.ForcedOff:
                    this.lightMode = Apparel_MinersHelmet.LightMode.Automatic;
                    break;
            }
            this.RefreshLightState();
        }

        public enum LightMode
        {
            Automatic,
            ForcedOn,
            ForcedOff,
        }
    }
}
