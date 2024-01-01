using HugsLib.Utils;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class Building_BlastingWire : Building
    {
        private const float FreezeTemperature = -1f;
        private const float WetWeatherThreshold = 0.5f;
        private const float TicksPerDay = 60000f;
        private const float RareTicksPerDay = 240f;
        private const float MaxWetness = 1f;
        private float wetness;
        private bool wantDrying;

        private BuildingProperties_BlastingWire CustomProps
        {
            get
            {
                if (!(this.def.building is BuildingProperties_BlastingWire))
                    throw new Exception("Building_BlastingWire requires BuildingProperties_BlastingWire");
                return (BuildingProperties_BlastingWire)this.def.building;
            }
        }

        public bool WantDrying
        {
            get
            {
                return this.wantDrying;
            }
        }

        public int DryOffJobDuration
        {
            get
            {
                return this.CustomProps.dryOffJobDurationTicks;
            }
        }

        private float Wetness
        {
            get
            {
                return this.wetness;
            }
            set
            {
                this.wetness = Mathf.Min(value, Mathf.Max(1f, 0.0f));
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            CompWiredBlasterTransmitter comp = this.GetComp<CompWiredBlasterTransmitter>();
            if (comp == null)
                return;
            comp.signalPassageTest = new CompWiredBlasterTransmitter.AllowSignalPassage(this.SignalPassageTest);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.wetness, "wetness", 0.0f, false);
            Scribe_Values.Look<bool>(ref this.wantDrying, "wantDrying", false, false);
        }

        public override void TickRare()
        {
            base.TickRare();
            Room room = this.Position.GetRoom(this.Map);
            float num = room == null ? 0.0f : room.Temperature;
            bool flag = (double)num < -1.0;
            if ((double)this.Map.weatherManager.RainRate > 0.5)
            {
                if (!flag && !this.IsCovered())
                    this.Wetness = 1f;
            }
            else if ((double)this.Wetness > 0.0 && (double)num > 0.0)
                this.Wetness -= (float)(1.0 / ((double)this.CustomProps.daysToDryByself * 240.0) * ((double)num / (double)this.CustomProps.baseDryingTemp));
            if (!this.wantDrying || (double)this.Wetness != 0.0)
                return;
            this.wantDrying = false;
            this.UpdateDesignation();
        }

        public override string GetInspectString()
        {
            return ((double)this.wetness > 0.0 ? "RH_TET_Dwarfs_BlastingWireWet".Translate((NamedArgument)Mathf.Round(this.wetness * 100f)) : "RH_TET_Dwarfs_BlastingWireDry".Translate()) + ", " + (this.IsCovered() ? "RH_TET_Dwarfs_BlastingWireCovered".Translate() : "RH_TET_Dwarfs_BlastingWireExposed".Translate());
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if ((double)this.wetness > 0.0)
            {
                Command_Toggle commandToggle = new Command_Toggle();
                commandToggle.toggleAction = new Action(this.DryGizmoAction);
                commandToggle.isActive = (Func<bool>)(() => this.wantDrying);
                commandToggle.icon = RH_TET_DwarfDefOf.Textures.CommandButton_DryOutUI;
                commandToggle.defaultLabel = "RH_TET_Dwarfs_BlastingWireDryOff".Translate();
                commandToggle.defaultDesc = "RH_TET_Dwarfs_BlastingWireDryOffDesc".Translate();
                commandToggle.hotKey = KeyBindingDefOf.Misc1;
                yield return (Gizmo)commandToggle;
            }
            foreach (Gizmo gizmo1 in base.GetGizmos())
            {
                Gizmo gizmo = gizmo1;
                yield return gizmo;
                gizmo = (Gizmo)null;
            }
        }

        public void DryOff()
        {
            this.Wetness = 0.0f;
            this.wantDrying = false;
            this.UpdateDesignation();
        }

        private void DryGizmoAction()
        {
            this.wantDrying = !this.wantDrying;
            this.UpdateDesignation();
        }

        private bool SignalPassageTest()
        {
            float num = (double)this.wetness > 0.0 ? this.CustomProps.failChanceWhenFullyWet * this.wetness : 0.0f;
            bool flag = (double)num <= 0.0 || (double)Rand.Range(0.0f, 1f) > (double)num;
            if (!flag)
                this.DoFailure();
            return flag;
        }

        private bool IsCovered()
        {
            return this.Position.Roofed(this.Map) || this.Map.edificeGrid[this.Map.cellIndices.CellToIndex(this.Position)] != null;
        }

        private void DoFailure()
        {
            if (this.CustomProps.failEffecter != null)
                this.CustomProps.failEffecter.Spawn().Trigger(new TargetInfo(this.Position, this.Map, false), (TargetInfo)((Verse.Thing)null));
            Map map = this.Map;
            this.Destroy(DestroyMode.KillFinalize);
            List<IntVec3> list = ((IEnumerable<IntVec3>)GenAdj.CardinalDirections).ToList<IntVec3>();
            list.Shuffle<IntVec3>();
            list.Add(IntVec3.Zero);
            list.Reverse();
            Fire createdFire = (Fire)null;
            foreach (IntVec3 intVec3 in list)
            {
                IntVec3 c = intVec3 + this.Position;
                FireUtility.TryStartFireIn(c, map, Rand.Range(0.4f, 0.6f));
                createdFire = map.thingGrid.ThingAt<Fire>(c);
                if (createdFire != null)
                    break;
            }
            Alert_BlastingWireFailure.Instance.ReportFailure(createdFire);
        }

        private void UpdateDesignation()
        {
            HugsLibUtility.ToggleDesignation((Verse.Thing)this, RH_TET_DwarfDefOf.RH_TET_Dwarfs_BlastingWireDryOut, this.wantDrying);
        }
    }
}
