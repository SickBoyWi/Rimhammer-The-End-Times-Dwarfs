using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class VisitableSite : MapParent
    {
        public bool RemoveAfterLeave = true;

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(
          Caravan caravan)
        {
            VisitableSite visitableSite = this;

            foreach (FloatMenuOption floatMenuOption in visitableSite.GetFloatMenuOptions(caravan))
                yield return floatMenuOption;

            if (!visitableSite.HasMap)
            {
                foreach (FloatMenuOption floatMenuOption in visitableSite.GetFloatMenuOptions(caravan, (MapParent)visitableSite))
                    yield return floatMenuOption;
            }
        }

        public virtual IEnumerable<FloatMenuOption> GetFloatMenuOptions(
          Caravan caravan,
          MapParent mapParent)
        {
            return (IEnumerable<FloatMenuOption>)null;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.RemoveAfterLeave, "RemoveAfterLeave", false, false);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            VisitableSite visitableSite = this;

            foreach (Gizmo gizmo in visitableSite.GetGizmos())
                yield return gizmo;
            if (visitableSite.HasMap)
                yield return (Gizmo)visitableSite.LeaveCommand(visitableSite.Map);
        }

        public virtual bool CanLeave()
        {
            return true;
        }

        private Command LeaveCommand(Map map)
        {
            Command_Action commandAction = new Command_Action();
            commandAction.defaultLabel = "RH_TET_Dwarf_LeaveCommand".Translate();
            commandAction.defaultDesc = "RH_TET_Dwarf_LeaveCommandDesc".Translate();
            commandAction.icon = ContentFinder<Texture2D>.Get("Map/leaving-queue", true);
            commandAction.action = (Action)(() =>
            {
                if (!this.CanLeave())
                    return;
                this.PreForceReform((MapParent)this);
            });
            if (map.mapPawns.FreeColonistsCount == 0)
                commandAction.Disable((string)null);
            return (Command)commandAction;
        }

        public virtual void PreForceReform(MapParent mapParent)
        {
            this.ForceReform(mapParent);
        }

        public virtual void ForceReform(MapParent mapParent)
        {
            if (Dialog_FormCaravan.AllSendablePawns(mapParent.Map, true).Any<Pawn>((Predicate<Pawn>)(x => x.IsColonist)))
            {
                Messages.Message("MessageYouHaveToReformCaravanNow".Translate(), (LookTargets)new GlobalTargetInfo(mapParent.Tile), MessageTypeDefOf.NeutralEvent, true);
                Current.Game.CurrentMap = mapParent.Map;
                Find.WindowStack.Add((Window)new Dialog_FormCaravan(mapParent.Map, true, (Action)(() =>
                {
                    if (!this.RemoveAfterLeave || !mapParent.HasMap)
                        return;
                    Find.WorldObjects.Remove((WorldObject)mapParent);
                }), true));
            }
            else
            {
                List<Pawn> list = new List<Pawn>();
                list.Clear();
                list.AddRange(mapParent.Map.mapPawns.AllPawns.Where<Pawn>((Func<Pawn, bool>)(x =>
                {
                    if (x.Faction != Faction.OfPlayer)
                        return x.HostFaction == Faction.OfPlayer;
                    return true;
                })));
                if (list.Any<Pawn>((Predicate<Pawn>)(x => CaravanUtility.IsOwner(x, Faction.OfPlayer))))
                    CaravanExitMapUtility.ExitMapAndCreateCaravan((IEnumerable<Pawn>)list, Faction.OfPlayer, mapParent.Tile, mapParent.Tile, -1, true);
                list.Clear();
                if (!this.RemoveAfterLeave)
                    return;
                Find.WorldObjects.Remove((WorldObject)mapParent);
            }
        }
    }
}
