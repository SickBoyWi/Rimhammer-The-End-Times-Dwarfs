using RimWorld;
using System;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class IncidentWorker_WildDwarfWandersIn : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }

            Faction ofPlayer = Faction.OfPlayerSilentFail;
            if (!(RH_TET_DwarfsMod.AnyOneCanDwarfModActive || ofPlayer.def.defName.Equals("RH_TET_Dwarf_PlayerColony") || ofPlayer.def.defName.Equals("RH_TET_Dwarf_Slayer_PlayerColony")))
            {
                return false;
            }

            Faction faction;
            if (!this.TryFindFormerFaction(out faction))
            {
                return false;
            }
            Map map = (Map)parms.target;
            IntVec3 intVec;
            return !map.GameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout) && map.mapTemperature.SeasonAcceptableFor(ThingDefOf.Human) && this.TryFindEntryCell(map, out intVec);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            IntVec3 loc;
            if (!this.TryFindEntryCell(map, out loc))
            {
                return false;
            }
            Faction faction;
            if (!this.TryFindFormerFaction(out faction))
            {
                return false;
            }

            Faction ofPlayer = Faction.OfPlayer;
            PawnKindDef wildToUse = PawnKindDef.Named("RH_TET_Dwarfs_WildDwarf");
            int rando = RH_TET_DwarfsMod.random.Next(0,4);
            
            if (ofPlayer.def.defName.Equals("RH_TET_Dwarf_PlayerColony") && rando == 0)
            {
                wildToUse = PawnKindDef.Named("RH_TET_Dwarfs_WildSlayer");
            }
            else if (ofPlayer.def.defName.Equals("RH_TET_Dwarf_Slayer_PlayerColony") && rando != 0)
            {
                wildToUse = PawnKindDef.Named("RH_TET_Dwarfs_WildSlayer");
            }

            Pawn pawn = PawnGenerator.GeneratePawn(wildToUse, faction);
            string labelShortSave = pawn.LabelShort;

            pawn.SetFaction(null, null);
            pawn.ChangeKind(PawnKindDefOf.WildMan);

            GenSpawn.Spawn(pawn, loc, map, WipeMode.Vanish);
            TaggedString label = this.def.letterLabel.Formatted(labelShortSave, pawn.Named("PAWN"));
            TaggedString text = this.def.letterText.Formatted(labelShortSave, pawn.Named("PAWN")).AdjustedFor(pawn, "PAWN").CapitalizeFirst();
            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref label, pawn);
            Find.LetterStack.ReceiveLetter(label, text, this.def.letterDef, pawn, null, null);
            return true;
        }

        private bool TryFindEntryCell(Map map, out IntVec3 cell)
        {
            return CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => map.reachability.CanReachColony(c), map, CellFinder.EdgeRoadChance_Ignore, out cell);
        }

        private bool TryFindFormerFaction(out Faction formerFaction)
        {
            Faction ofPlayer = Faction.OfPlayer;

            foreach (Faction current in Find.FactionManager.AllFactionsInViewOrder)
            {
                if (current.def.defName.Equals("RH_TET_Dwarf_KarakMountain"))
                {
                    formerFaction = current;
                    return true;
                }
            }

            formerFaction = null;
            return false;
        }
    }
}
