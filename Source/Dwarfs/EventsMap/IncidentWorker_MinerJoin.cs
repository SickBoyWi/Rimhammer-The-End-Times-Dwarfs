using RimWorld;
using System;
using Verse;
using System.Linq;

namespace TheEndTimes_Dwarfs
{
    public class IncidentWorker_MinerJoin : IncidentWorker
    {
        private const float RelationWithColonistWeight = .01f;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Faction ofPlayer = Faction.OfPlayer;

            // This only happens to dwarfs, unless any one can dwarfs mod active.
            if (!base.CanFireNowSub(parms)
                    || !(RH_TET_DwarfsMod.AnyOneCanDwarfModActive || (ofPlayer.def.defName.Equals("RH_TET_Dwarf_PlayerColony") || ofPlayer.def.defName.Equals("RH_TET_Dwarf_Slayer_PlayerColony"))))
            {
                return false;
            }
            Map map = (Map)parms.target;
            IntVec3 intVec;

            return InfestationCellFinder.TryFindCell(out intVec, map);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            IntVec3 loc;
            if (!InfestationCellFinder.TryFindCell(out loc, map))
            {
                return false;
            }
            Gender? gender = null;
            if (this.def.pawnFixedGender != Gender.None)
            {
                gender = new Gender?(this.def.pawnFixedGender);
            }
            PawnKindDef pawnKind = this.def.pawnKind;

            Faction ofPlayer = Faction.OfPlayer;
            bool pawnMustBeCapableOfViolence = this.def.pawnMustBeCapableOfViolence;
            Gender? fixedGender = gender;
            PawnGenerationRequest request = new PawnGenerationRequest(pawnKind, ofPlayer, PawnGenerationContext.NonPlayer, -1, 
                true, false, false, 
                false, pawnMustBeCapableOfViolence, 0.05f, 
                true, true, true, 
                false, false, false, 
                false, false,false,
                0.0f, 0.0f, null, 
                0.0f, null, null, 
                null, null, null, 
                null, null, fixedGender);

            WorkTypeDef mining = DefDatabase<WorkTypeDef>.AllDefsListForReading.Where(w => w.defName == "Mining").First();
            Pawn pawn = null;
            int tryMax = 30;
            for (int tryNum = 1; tryNum <= tryMax; tryNum++) 
            {
                pawn = PawnGenerator.GeneratePawn(request);

                try
                {
                    pawn.ideo.SetIdeo(Faction.OfPlayer.ideos.PrimaryIdeo);
                }
                catch
                {
                    // Ignore if no ideos present.
                }

                if ((pawn.story.DisabledWorkTagsBackstoryAndTraits & WorkTags.Mining) == WorkTags.Mining)
                {
                    pawn.Destroy(mode: DestroyMode.Vanish);
                    pawn = null;
                    if (tryNum == tryMax)
                    {
                        Log.Message("Rimhammer The End Times - Dwarfs: Can Not generate requested pawn. They are always incapable of mining, and this incident requires a miner.");
                    }
                }

                if (pawn != null)
                {
                    break;
                }
            }

            // Couldn't make the pawn, get out of here.
            if (pawn == null)
                return false;

            //  Force Minor Passion
            pawn.skills.skills.Where(w => w.def.defName == "Mining").First().passion = Passion.Minor;
            
            //  Force Mining to skill level 7, childhood backstory skillGains + adulthood backstory skillGains as defined in RH_TET_Dwarfs_Backstory.xml, 28000 is required for level 7
            SkillRecord pawnMining = pawn.skills.GetSkill(SkillDefOf.Mining);
            
            // Make malnourished.
            pawn.health.AddHediff(HediffMaker.MakeHediff(HediffDefOf.Malnutrition, pawn, null), null, null, null);

            //  Spawn
            GenSpawn.Spawn(pawn, loc, map, WipeMode.Vanish);

            TaggedString text = this.def.letterText.Formatted(pawn.Named("PAWN")).AdjustedFor(pawn, "PAWN");
            TaggedString label = this.def.letterLabel.Formatted(pawn.Named("PAWN")).AdjustedFor(pawn, "PAWN");
            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref label, pawn);
            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.PositiveEvent, pawn, null, null);
            return true;
        }

        private bool TryFindEntryCell(Map map, out IntVec3 cell)
        {
            return CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => map.reachability.CanReachColony(c) && !c.Fogged(map), map, CellFinder.EdgeRoadChance_Neutral, out cell);
        }
    }
}