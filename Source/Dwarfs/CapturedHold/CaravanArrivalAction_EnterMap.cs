using RimWorld;
using RimWorld.Planet;
using System;
using System.Text;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class CaravanArrivalAction_EnterMap : CaravanArrivalAction_Enter
    {
        private MapParent mapParent;

        public MapParent MapParent
        {
            get
            {
                return this.mapParent;
            }
        }

        public virtual IntVec3 MapSize { get; set; }

        public override string Label
        {
            get
            {
                return "EnterMap".Translate((NamedArgument)this.mapParent.Label);
            }
        }

        public override string ReportString
        {
            get
            {
                return "CaravanEntering".Translate((NamedArgument)this.mapParent.Label);
            }
        }

        public CaravanArrivalAction_EnterMap()
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<MapParent>(ref this.mapParent, "MapParent", false);
        }

        public CaravanArrivalAction_EnterMap(MapParent mapParent)
        {
            this.mapParent = mapParent;
        }

        public CaravanArrivalAction_EnterMap(MapParent mapParent, VisitableSite site)
        {
            this.mapParent = mapParent;
        }

        public override void Arrived(Caravan caravan)
        {
            this.Enter(caravan);
        }

        public virtual FloatMenuAcceptanceReport CanVisit(
          Caravan caravan,
          MapParent mapParent)
        {
            if (mapParent == null || !mapParent.Spawned)
                return (FloatMenuAcceptanceReport)false;
            return (FloatMenuAcceptanceReport)true;
        }

        public override FloatMenuAcceptanceReport StillValid(
          Caravan caravan,
          int destinationTile)
        {
            FloatMenuAcceptanceReport acceptanceReport = (FloatMenuAcceptanceReport)true;
            if (!(bool)acceptanceReport)
                return acceptanceReport;
            if (this.mapParent != null && this.mapParent.Tile != destinationTile)
                return (FloatMenuAcceptanceReport)false;
            return this.CanVisit(caravan, this.mapParent);
        }

        public void Enter(Caravan caravan)
        {
            if (!this.mapParent.HasMap)
                LongEventHandler.QueueLongEvent((Action)(() => this.DoEnter(caravan)), "GeneratingMapForNewEncounter", false, (Action<Exception>)null);
            else
                this.DoEnter(caravan);
        }

        public virtual void DoEnter(Caravan caravan)
        {
            Pawn pawn = caravan.PawnsListForReading[0];
            int num = !this.mapParent.HasMap ? 1 : 0;
            Map orGenerateMap = this.GetOrGenerateMap(this.mapParent.Tile, this.MapSize, (WorldObjectDef)null);
            if (num != 0)
            {
                Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
                PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(orGenerateMap.mapPawns.AllPawns, "LetterRelatedPawnsSite".Translate((NamedArgument)Faction.OfPlayer.def.pawnsPlural), LetterDefOf.NeutralEvent, true, true);
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("LetterCaravanEnteredMap".Translate((NamedArgument)caravan.Label, (NamedArgument)((WorldObject)this.MapParent)).CapitalizeFirst());
                Find.LetterStack.ReceiveLetter("RH_TET_Dwarf_CaravanArrivedAt".Translate() + " " + this.Label, stringBuilder.ToString(), LetterDefOf.NeutralEvent, (string)null);
            }
            else
                Find.LetterStack.ReceiveLetter("RH_TET_Dwarf_CaravanArrivedAt".Translate() + " " + this.Label, 
                    "LetterCaravanEnteredMap".Translate((NamedArgument)caravan.Label, 
                    (NamedArgument)((WorldObject)this.MapParent)).CapitalizeFirst(), 
                    LetterDefOf.NeutralEvent, 
                    (LookTargets)((Thing)pawn), 
                    null,
                    null,
                    null,
                    null);

            Map map = orGenerateMap;
            CaravanEnterMode enterMode = CaravanEnterMode.Edge;
            CaravanEnterMapUtility.Enter(caravan, map, enterMode, CaravanDropInventoryMode.DoNotDrop, true, (Predicate<IntVec3>)null);
        }

        public virtual Map GetOrGenerateMap(
          int tile,
          IntVec3 mapSize,
          WorldObjectDef suggestedMapParentDef)
        {
            return GetOrGenerateMapUtility.GetOrGenerateMap(this.mapParent.Tile, this.MapSize, (WorldObjectDef)null);
        }
    }
}
