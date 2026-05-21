using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class IncidentWorker_HuntedRunesmith : IncidentWorker
    {
        private static readonly IntRange RaidDelay = new IntRange(90000, 125000);
        private static readonly FloatRange RaidPointsFactorRange = new FloatRange(1.8f, 2.0f);
        private const float RelationWithColonistWeight = .02f;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            IntVec3 spawnSpot;
            if (!base.CanFireNowSub(parms) || !this.TryFindSpawnSpot((Map)parms.target, out spawnSpot))
                return false;

            Faction ofPlayer = Faction.OfPlayer;

            // Make sure this only happens to Empire.
            if (ofPlayer.def.defName.Equals("RH_TET_Dwarf_PlayerColony")
                || ofPlayer.def.defName.Equals("RH_TET_Dwarf_Slayer_PlayerColony")
                || RH_TET_DwarfsMod.AnyOneCanDwarfModActive)
            {
                Faction enemyFac;
                bool factionFound = this.TryFindEnemyFaction(out enemyFac);
                int runicPawnCount = 0;

                if (factionFound)
                {
                    // If the player has more than two faith pawns already, don't allow this event.
                    List<Pawn> playerPawns = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_Colonists;

                    foreach (Pawn p in playerPawns)
                    {
                        if (p.def != null)
                        {
                            if (p.def.defName.Contains("Runic"))
                                runicPawnCount++;
                        }

                        if (runicPawnCount > 2)
                            break;
                    }

                    if (runicPawnCount > 2)
                        return false;
                    else
                        return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            Faction enemyFac;
            IntVec3 spawnSpot;
            if (!this.TryFindSpawnSpot(map, out spawnSpot) || !this.TryFindEnemyFaction(out enemyFac))
                return false;
            int num = Rand.Int;
            IncidentParms raidParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, (IIncidentTarget)map);
            raidParms.forced = true;
            raidParms.faction = enemyFac;
            raidParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            raidParms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
            raidParms.spawnCenter = spawnSpot;
            raidParms.points = Mathf.Max(raidParms.points * IncidentWorker_HuntedRunesmith.RaidPointsFactorRange.RandomInRange, enemyFac.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat));
            raidParms.pawnGroupMakerSeed = new int?(num);
            PawnGroupMakerParms pawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, raidParms, false);
            pawnGroupMakerParms.points = IncidentWorker_Raid.AdjustedRaidPoints(pawnGroupMakerParms.points, raidParms.raidArrivalMode, raidParms.raidStrategy, pawnGroupMakerParms.faction, PawnGroupKindDefOf.Combat, raidParms.target, raidParms.raidAgeRestriction);
            IEnumerable<PawnKindDef> pawnKindsExample = PawnGroupMakerUtility.GeneratePawnKindsExample(pawnGroupMakerParms);

            List<Pawn> runesmith = new List<Pawn>();

            PawnGenerationRequest request = new PawnGenerationRequest(GetRandomPawnKindDef(), (Faction)null,
                PawnGenerationContext.NonPlayer,
                PlanetTile.Invalid,
                false, false, false,
                true, true, 1f,
                true, true, true,
                true, false, false,
                false, false, false,
                0.0f, 0.0f, null, 0.05f, (Predicate<Pawn>)null, (Predicate<Pawn>)null, (IEnumerable<TraitDef>)null, (IEnumerable<TraitDef>)null, null, null, null, null, (string)null, (string)null, (RoyalTitleDef)null, null, false, false, false, false, (List<GeneDef>)null, (List<GeneDef>)null, (XenotypeDef)null, (CustomXenotype)null, (List<XenotypeDef>)null, 0.0f, DevelopmentalStage.Adult, (Func<XenotypeDef, PawnKindDef>)null, null, null, false, false, false, -1, 0, false);

            Pawn refugee = PawnGenerator.GeneratePawn(request);

            refugee.relations.everSeenByPlayer = true;

            try
            {
                refugee.ideo.SetIdeo(Faction.OfPlayer.ideos.PrimaryIdeo);
            }
            catch
            {
                // Ignore if no ideos present.
            }
            TaggedString text = "RH_TET_Dwarfs_RefugeeChasedRunicInitial".Translate((NamedArgument)refugee.Name.ToStringFull, (NamedArgument)refugee.story.Title, (NamedArgument)enemyFac.def.pawnsPlural, (NamedArgument)enemyFac.Name, (NamedArgument)PawnUtility.PawnKindsToCommaList(pawnKindsExample, true), refugee.Named("PAWN")).AdjustedFor(refugee, "PAWN");
            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, refugee);

            runesmith.Add(refugee);

            DiaNode nodeRoot = new DiaNode(text);
            nodeRoot.options.Add(new DiaOption("RH_TET_Dwarfs_RefugeeChasedRunicInitial_Accept".Translate())
            {
                action = (Action)(() =>
                {
                    foreach (Pawn p in runesmith)
                    {
                        GenSpawn.Spawn((Thing)p, spawnSpot, map, WipeMode.Vanish);
                        p.SetFaction(Faction.OfPlayer, (Pawn)null);
                    }

                    CameraJumper.TryJump((GlobalTargetInfo)((Thing)refugee));
                    Find.Storyteller.incidentQueue.Add(new QueuedIncident(new FiringIncident(IncidentDefOf.RaidEnemy, (StorytellerComp)null, raidParms), Find.TickManager.TicksGame + IncidentWorker_HuntedRunesmith.RaidDelay.RandomInRange, 0));
                }),
                resolveTree = true
            });
            nodeRoot.options.Add(new DiaOption("RH_TET_Dwarfs_RefugeeChasedRunicInitial_Reject".Translate())
            {
                action = (Action)(() => Find.WorldPawns.PassToWorld(refugee, PawnDiscardDecideMode.Decide)),
                link = new DiaNode("RH_TET_Dwarfs_RefugeeChasedRunicInitial_Rejected".Translate((NamedArgument)refugee.LabelShort, (NamedArgument)((Thing)refugee)))
                {
                    options = {
                    new DiaOption("OK".Translate()) { resolveTree = true }
                    }
                }
            });

            string title = "RH_TET_Dwarfs_RefugeeChasedRunicTitle".Translate((NamedArgument)map.Parent.Label);
            Find.WindowStack.Add((Window)new Dialog_NodeTreeWithFactionInfo(nodeRoot, enemyFac, true, true, title));
            Find.Archive.Add((IArchivable)new ArchivedDialog(nodeRoot.text, title, enemyFac));
            return true;
        }

        private PawnKindDef GetRandomPawnKindDef()
        {
            List<PawnKindDef> STANDARD_RUNIC_PAWN_KINDS = new List<PawnKindDef>();
            STANDARD_RUNIC_PAWN_KINDS.Add(RH_TET_DwarfDefOf.RH_TET_Dwarfs_RunicStandard);

            List<PawnKindDef> GREAT_RUNIC_PAWN_KINDS = new List<PawnKindDef>();
            GREAT_RUNIC_PAWN_KINDS.Add(RH_TET_DwarfDefOf.RH_TET_Dwarfs_RunicMaster);

            if (RH_TET_DwarfsMod.random.Next(0, 10) > 3)
                return STANDARD_RUNIC_PAWN_KINDS.RandomElement();
            else
                return GREAT_RUNIC_PAWN_KINDS.RandomElement();

        }

        private bool TryFindSpawnSpot(Map map, out IntVec3 spawnSpot)
        {
            return CellFinder.TryFindRandomEdgeCellWith((Predicate<IntVec3>)(c =>
            {
                if (map.reachability.CanReachColony(c))
                    return !c.Fogged(map);
                return false;
            }), map, CellFinder.EdgeRoadChance_Neutral, out spawnSpot);
        }

        private bool TryFindEnemyFaction(out Faction enemyFac)
        {
            return Find.FactionManager.AllFactions.Where<Faction>((Func<Faction, bool>)(f =>
            {
                if (!f.def.hidden && !f.defeated)
                    return f.HostileTo(Faction.OfPlayer);
                return false;
            })).TryRandomElement<Faction>(out enemyFac);
        }
    }
}
