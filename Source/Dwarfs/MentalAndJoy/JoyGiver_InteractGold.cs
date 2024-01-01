using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace TheEndTimes_Dwarfs
{
    public class JoyGiver_InteractGold : JoyGiver
    {
        private static List<Thing> tmpCandidates = new List<Thing>();

        protected virtual bool CanDoDuringParty
        {
            get
            {
                return true;
            }
        }

        private Thing FindBestThing(Pawn pawn, bool inBed, IntVec3 partySpot)
        {
            JoyGiver_InteractGold.tmpCandidates.Clear();
            this.GetSearchSet(pawn, JoyGiver_InteractGold.tmpCandidates);
            if (JoyGiver_InteractGold.tmpCandidates.Count == 0)
            {
                return null;
            }

            Predicate<Thing> predicate = (Thing t) => (this.CanInteractWith(pawn, t, inBed) && t.Map.glowGrid.GameGlowAt(t.Position) > .49f);
            if (partySpot.IsValid)
            {
                Predicate<Thing> oldValidator = predicate;
                predicate = ((Thing x) => GatheringsUtility.InGatheringArea(x.Position, partySpot, pawn.Map) && oldValidator(x));
            }

            IntVec3 position = pawn.Position;
            Map map = pawn.Map;
            List<Thing> searchSet = JoyGiver_InteractGold.tmpCandidates;
            PathEndMode peMode = PathEndMode.OnCell;
            TraverseParms traverseParams = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false);
            Predicate<Thing> validator = predicate;
            Thing result = GenClosest.ClosestThing_Global_Reachable(position, map, searchSet, peMode, traverseParams, 9999f, validator, null);
            JoyGiver_InteractGold.tmpCandidates.Clear();
            
            return result;
        }

        protected override void GetSearchSet(Pawn pawn, List<Thing> outCandidates)
        {
            outCandidates.Clear();

            if (this.def.thingDefs == null)
            {
                return;
            }
            if (this.def.thingDefs.Count == 1)
            {
                outCandidates.AddRange(pawn.Map.listerThings.ThingsOfDef(this.def.thingDefs[0]));
            }
            else
            {
                for (int i = 0; i < this.def.thingDefs.Count; i++)
                {
                    outCandidates.AddRange(pawn.Map.listerThings.ThingsOfDef(this.def.thingDefs[i]));
                }
            }
        }
        
        public override Job TryGiveJob(Pawn pawn)
        {
            Thing thing = this.FindBestThing(pawn, false, IntVec3.Invalid);
            if (thing != null)
            {
                return this.TryGivePlayJob(pawn, thing);
            }
            return null;
        }

        public override Job TryGiveJobWhileInBed(Pawn pawn)
        {
            Thing thing = this.FindBestThing(pawn, true, IntVec3.Invalid);
            if (thing != null)
            {
                return this.TryGivePlayJobWhileInBed(pawn, thing);
            }
            return null;
        }
        // TODO Add or dump.
        //public override Job TryGiveJobInPartyArea(Pawn pawn, IntVec3 partySpot)
        //{
        //    if (!this.CanDoDuringParty)
        //    {
        //        return null;
        //    }
        //    Thing thing = this.FindBestThing(pawn, false, partySpot);
        //    if (thing != null)
        //    {
        //        return this.TryGivePlayJob(pawn, thing);
        //    }
        //    return null;
        //}

        protected virtual bool CanInteractWith(Pawn pawn, Thing t, bool inBed)
        {
            if (!pawn.CanReserve(t, this.def.jobDef.joyMaxParticipants, -1, null, false))
            {
                return false;
            }
            if (t.IsForbidden(pawn))
            {
                return false;
            }
            if (!t.IsSociallyProper(pawn))
            {
                return false;
            }
            if (!t.IsPoliticallyProper(pawn))
            {
                return false;
            }
            
            return true;
        }

        protected Job TryGivePlayJob(Pawn pawn, Thing t)
        {
            IntVec3 c;
            Building t2;
            
            if (!WatchThingUtility.TryFindBestWatchCell(t, pawn, this.def.desireSit, out c, out t2))
            {
                return null;
            }
            
            return new Job(this.def.jobDef, t, c, t2);
        }

        protected virtual Job TryGivePlayJobWhileInBed(Pawn pawn, Thing t_in)
        {
            Building_Bed t = pawn.CurrentBed();
            return new Job(this.def.jobDef, t_in, pawn.Position, t);
        }
    }
}