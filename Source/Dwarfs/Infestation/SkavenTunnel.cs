using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace TheEndTimes_Dwarfs
{
    public class SkavenTunnel : ThingWithComps, IAttackTarget, ILoadReferenceable
    {
        public static List<PawnKindDef> spawnablePawnKinds = new List<PawnKindDef>();
        public static readonly string MemoAttackedByEnemy = "HiveAttacked";
        public static readonly string MemoDeSpawned = "HiveDeSpawned";
        public static readonly string MemoBurnedBadly = "HiveBurnedBadly";
        public static readonly string MemoDestroyedNonRoofCollapse = "HiveDestroyedNonRoofCollapse";
        public const int PawnSpawnRadius = 2;
        public const float MaxSpawnedPawnsPoints = 500f;
        public const float InitialPawnsPoints = 200f;

        public CompCanBeDormant CompDormant
        {
            get
            {
                return this.GetComp<CompCanBeDormant>();
            }
        }

        Thing IAttackTarget.Thing
        {
            get
            {
                return (Thing)this;
            }
        }

        public float TargetPriorityFactor
        {
            get
            {
                return 0.4f;
            }
        }

        public LocalTargetInfo TargetCurrentlyAimingAt
        {
            get
            {
                return LocalTargetInfo.Invalid;
            }
        }

        public CompSpawnerPawn PawnSpawner
        {
            get
            {
                return this.GetComp<CompSpawnerPawn>();
            }
        }

        public bool ThreatDisabled(IAttackTargetSearcher disabledFor)
        {
            if (!this.Spawned)
                return true;
            CompCanBeDormant comp = this.GetComp<CompCanBeDormant>();
            return comp != null && !comp.Awake;
        }

        public static void ResetStaticData()
        {
            SkavenTunnel.spawnablePawnKinds.Clear();
            SkavenTunnel.spawnablePawnKinds.Add(PawnKindDefOf.Megascarab);
            SkavenTunnel.spawnablePawnKinds.Add(PawnKindDefOf.Spelopede);
            SkavenTunnel.spawnablePawnKinds.Add(PawnKindDefOf.Megaspider);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (this.Faction != null)
                return;
            this.SetFaction(Faction.OfInsects, (Pawn)null);
        }

        protected override void Tick()
        {
            base.Tick();
            if (!this.Spawned || this.CompDormant.Awake || this.Position.Fogged(this.Map))
                return;
            this.CompDormant.WakeUp();
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            Map map = this.Map;
            base.DeSpawn(mode);
            List<Lord> lords = map.lordManager.lords;
            for (int index = 0; index < lords.Count; ++index)
                lords[index].ReceiveMemo(SkavenTunnel.MemoDeSpawned);
            TunnelUtility.Notify_TunnelDespawned(this, map);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (!this.questTags.NullOrEmpty<string>())
            {
                bool flag = false;
                List<Thing> thingList = this.Map.listerThings.ThingsOfDef(this.def);
                for (int index = 0; index < thingList.Count; ++index)
                {
                    if (thingList[index] is SkavenTunnel tunnel && tunnel != this && (tunnel.CompDormant.Awake && !tunnel.questTags.NullOrEmpty<string>()) && QuestUtility.AnyMatchingTags(tunnel.questTags, this.questTags))
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                    QuestUtility.SendQuestTargetSignals(this.questTags, "AllHivesDestroyed");
            }
            base.Destroy(mode);
        }

        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            if (dinfo.Def.ExternalViolenceFor((Thing)this) && dinfo.Instigator != null && dinfo.Instigator.Faction != null)
                this.GetComp<CompSpawnerPawn>().Lord?.ReceiveMemo(SkavenTunnel.MemoAttackedByEnemy);
            if (dinfo.Def == DamageDefOf.Flame && (double)this.HitPoints < (double)this.MaxHitPoints * 0.300000011920929)
                this.GetComp<CompSpawnerPawn>().Lord?.ReceiveMemo(SkavenTunnel.MemoBurnedBadly);
            base.PostApplyDamage(dinfo, totalDamageDealt);
        }

        public override void Kill(DamageInfo? dinfo = null, Hediff exactCulprit = null)
        {
            if (this.Spawned && (!dinfo.HasValue ? 0 : (dinfo.Value.Category == DamageInfo.SourceCategory.Collapse ? 1 : 0)) == 0)
            {
                List<Lord> lords = this.Map.lordManager.lords;
                for (int index = 0; index < lords.Count; ++index)
                    lords[index].ReceiveMemo(SkavenTunnel.MemoDestroyedNonRoofCollapse);
            }
            base.Kill(dinfo, exactCulprit);
        }

        public override bool PreventPlayerSellingThingsNearby(out string reason)
        {
            if (this.PawnSpawner.spawnedPawns.Count > 0 && this.PawnSpawner.spawnedPawns.Any<Pawn>((Predicate<Pawn>)(p => !p.Downed)))
            {
                reason = this.def.label;
                return true;
            }
            reason = (string)null;
            return false;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            SkavenTunnel tunnel = this;
            foreach (Gizmo gizmo in base.GetGizmos())
                yield return gizmo;
            foreach (Gizmo questRelatedGizmo in QuestUtility.GetQuestRelatedGizmos((Thing)tunnel))
                yield return questRelatedGizmo;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.Saving)
                return;
            bool flag = false;
            Scribe_Values.Look<bool>(ref flag, "active", false, false);
            if (!flag)
                return;
            this.CompDormant.WakeUp();
        }
    }
}
