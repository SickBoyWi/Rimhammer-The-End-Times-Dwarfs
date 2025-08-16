using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;

namespace TheEndTimes_Dwarfs
{
    public class Building_Horn : Building
    {
        private Pawn currentMusician;
        private Sustainer soundWhilePlaying;

        public static bool IsAffectedByInstrument(
          ThingDef def,
          IntVec3 position,
          IntVec3 pawnPos,
          Map map)
        {
            if ((double)position.DistanceTo(pawnPos) < (double)def.building.instrumentRange)
                return position.GetRoom(map) == pawnPos.GetRoom(map);
            return false;
        }

        public bool IsBeingPlayed
        {
            get
            {
                return this.currentMusician != null;
            }
        }

        public FloatRange AffectedRange
        {
            get
            {
                if (this.soundWhilePlaying == null || this.soundWhilePlaying.def.subSounds.NullOrEmpty<SubSoundDef>())
                    return FloatRange.Zero;
                return this.soundWhilePlaying.def.subSounds.First<SubSoundDef>().distRange;
            }
        }

        public void StartPlaying(Pawn player)
        {
            this.currentMusician = player;
        }

        protected override void Tick()
        {
            base.Tick();
            if (this.currentMusician != null)
            {
                if (this.def.soundPlayInstrument != null && this.soundWhilePlaying == null)
                    this.soundWhilePlaying = this.def.soundPlayInstrument.TrySpawnSustainer(SoundInfo.InMap(new TargetInfo(this.Position, this.Map, false), MaintenanceType.PerTick));
            }
            else
                this.soundWhilePlaying = (Sustainer)null;
            if (this.soundWhilePlaying == null)
                return;
            this.soundWhilePlaying.Maintain();
        }

        public void StopPlaying()
        {
            this.currentMusician = (Pawn)null;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Pawn>(ref this.currentMusician, "currentHornPlayer", false);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                this.StopPlaying();
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            Building_Horn instrument = this;

            foreach (Gizmo gizmo in base.GetGizmos())
                yield return gizmo;
            if (Prefs.DevMode)
            {
                Command_Action commandAction = new Command_Action();
                commandAction.defaultLabel = "Debug: Toggle is playing";
                commandAction.action = new Action(instrument.DevGetMusician);
                yield return (Gizmo)commandAction;
            }
        }

        public void DevGetMusician()
        {
            this.currentMusician = this.currentMusician == null ? PawnsFinder.AllMaps_FreeColonists.FirstOrDefault<Pawn>() : (Pawn)null;
        }
    }
}
