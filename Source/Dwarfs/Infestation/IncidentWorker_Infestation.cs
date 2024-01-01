using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class IncidentWorker_Infestation : IncidentWorker
    {
        public const float SkavenTunnelPoints = 220f;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Map target = (Map)parms.target;
            if (base.CanFireNowSub(parms) && TunnelUtility.TotalSpawnedTunnelsCount(target) < 35)
            {
                IntVec3 cell;
                return InfestationCellFinder.TryFindCell(out cell, target);
            }
            return false;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map target = (Map)parms.target;
            Thing thing = TheEndTimes_Dwarfs.InfestationUtility.SpawnTunnels(Mathf.Max(GenMath.RoundRandom(parms.points / 220f), 1), target, false, (string)null);
            this.SendStandardLetter(parms, (LookTargets)thing, (NamedArgument[])Array.Empty<NamedArgument>());
            Find.TickManager.slower.SignalForceNormalSpeedShort();
            return true;
        }
    }
}
