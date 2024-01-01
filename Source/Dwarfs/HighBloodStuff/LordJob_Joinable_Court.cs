using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace TheEndTimes_Dwarfs
{
    public class LordJob_Joinable_Court : LordJob_Joinable_Gathering
    {
        public static readonly Dictionary<ThoughtDef, float> OutcomeThoughtChances = new Dictionary<ThoughtDef, float>()
    {
      {
        RH_TET_DwarfDefOf.RH_TET_Dwarfs_AwfulHeldCourt,
        0.05f
      },
      {
        RH_TET_DwarfDefOf.RH_TET_Dwarfs_BoringHeldCourt,
        0.15f
      },
      {
        RH_TET_DwarfDefOf.RH_TET_Dwarfs_DecentHeldCourt,
        0.6f
      },
      {
        RH_TET_DwarfDefOf.RH_TET_Dwarfs_GreatHeldCourt,
        0.2f
      }
    };
        private static List<Tuple<ThoughtDef, float>> outcomeChancesTemp = new List<Tuple<ThoughtDef, float>>();
        public const float DurationHours = 5f;

        public override bool AllowStartNewGatherings
        {
            get
            {
                return false;
            }
        }

        public override bool OrganizerIsStartingPawn
        {
            get
            {
                return true;
            }
        }

        public LordJob_Joinable_Court()
        {
        }

        public LordJob_Joinable_Court(IntVec3 spot, Pawn organizer, GatheringDef gatheringDef)
          : base(spot, organizer, gatheringDef)
        {
        }

        protected override LordToil CreateGatheringToil(
          IntVec3 spot,
          Pawn organizer,
          GatheringDef gatheringDef)
        {
            return (LordToil)new LordToil_HoldCourt(spot, gatheringDef, organizer);
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil gatheringToil = this.CreateGatheringToil(this.spot, this.organizer, this.gatheringDef);
            stateGraph.AddToil(gatheringToil);
            LordToil_End lordToilEnd = new LordToil_End();
            stateGraph.AddToil((LordToil)lordToilEnd);
            float courtDuration = 8000f;
            Transition transition1 = new Transition(gatheringToil, (LordToil)lordToilEnd, false, true);
            transition1.AddTrigger((Trigger)new Trigger_TickCondition(new Func<bool>(((LordJob_Joinable_Court)this).ShouldBeCalledOff), 1));
            transition1.AddTrigger((Trigger)new Trigger_PawnKilled());
            transition1.AddTrigger((Trigger)new Trigger_PawnLost(PawnLostCondition.LeftVoluntarily, this.organizer));
            transition1.AddPreAction((TransitionAction)new TransitionAction_Custom((Action)(() => this.ApplyOutcome((float)this.lord.ticksInToil / courtDuration))));
            stateGraph.AddTransition(transition1, false);
            this.timeoutTrigger = (Trigger_TicksPassed)new Trigger_TicksPassedAfterConditionMet((int)courtDuration, (Func<bool>)(() => GatheringsUtility.InGatheringArea(this.organizer.Position, this.spot, this.organizer.Map)), 60);
            Transition transition2 = new Transition(gatheringToil, (LordToil)lordToilEnd, false, true);
            transition2.AddTrigger((Trigger)this.timeoutTrigger);
            transition2.AddPreAction((TransitionAction)new TransitionAction_Custom((Action)(() => this.ApplyOutcome(1f))));
            stateGraph.AddTransition(transition2, false);
            return stateGraph;
        }

        public override string GetReport(Pawn pawn)
        {
            if (pawn != this.organizer)
                return (string)"RH_TET_Dwarfs_LordReportObservingCourt".Translate(this.organizer.Named("ORGANIZER"));
            return (string)"RH_TET_Dwarfs_LordReportHoldingCourt".Translate();
        }

        protected virtual void ApplyOutcome(float progress)
        {
            if ((double)progress < 0.5)
            {
                Find.LetterStack.ReceiveLetter("RH_TET_Dwarfs_CourtCancelLabel".Translate(), "RH_TET_Dwarfs_CourtCancel".Translate(this.organizer.Named("ORGANIZER")).CapitalizeFirst(), LetterDefOf.NegativeEvent, (LookTargets)((Thing)this.organizer), (Faction)null, (Quest)null, (List<ThingDef>)null, (string)null);
            }
            else
            {
                ThoughtDef key = LordJob_Joinable_Court.OutcomeThoughtChances.RandomElementByWeight<KeyValuePair<ThoughtDef, float>>((Func<KeyValuePair<ThoughtDef, float>, float>)(t =>
                {
                    if (!LordJob_Joinable_Court.PositiveOutcome(t.Key))
                        return LordJob_Joinable_Court.OutcomeThoughtChances[t.Key];
                    return LordJob_Joinable_Court.OutcomeThoughtChances[t.Key] * this.organizer.GetStatValue(StatDefOf.SocialImpact, true) * progress;
                })).Key;
                foreach (Pawn ownedPawn in this.lord.ownedPawns)
                {
                    if (ownedPawn != this.organizer && this.organizer.Position.InHorDistOf(ownedPawn.Position, 18f))
                        ownedPawn.needs.mood.thoughts.memories.TryGainMemory(key, this.organizer);
                }
                TaggedString text = "RH_TET_Dwarfs_FinishedCourt".Translate(this.organizer.Named("ORGANIZER")).CapitalizeFirst() + " " + (key.defName + "Letter").Translate();
                if ((double)progress < 1.0)
                    text += "\n\n" + "RH_TET_Dwarfs_CourtInterrupted".Translate((NamedArgument)progress.ToStringPercent(), this.organizer.Named("ORGANIZER"));
                Find.LetterStack.ReceiveLetter((TaggedString)key.stages[0].LabelCap, text, LordJob_Joinable_Court.PositiveOutcome(key) ? LetterDefOf.PositiveEvent : LetterDefOf.NegativeEvent, (LookTargets)((Thing)this.organizer), (Faction)null, (Quest)null, (List<ThingDef>)null, (string)null);

                Ability ability = this.organizer.abilities.GetAbility(RH_TET_DwarfDefOf.RH_TET_Dwarfs_HoldCourtAbility);

                CompDwarfHighBlood comp = this.organizer.TryGetComp<CompDwarfHighBlood>();

                if (comp == null || comp.highBloodComp == null || comp.highBloodComp.GetCurrentHighBlood() == null)
                    return;

                HighBloodDef highBlood = comp.highBloodComp.GetCurrentHighBlood();

                if (ability == null)
                    return;

                comp.highBloodComp.lastCourtTicks = Find.TickManager.TicksGame;

                ability.StartCooldown(highBlood.courtCooldown.RandomInRange);
            }
        }

        private static bool PositiveOutcome(ThoughtDef outcome)
        {
            if (outcome != RH_TET_DwarfDefOf.RH_TET_Dwarfs_DecentHeldCourt)
                return outcome == RH_TET_DwarfDefOf.RH_TET_Dwarfs_GreatHeldCourt;
            return true;
        }

        public static IEnumerable<Tuple<ThoughtDef, float>> OutcomeChancesForPawn(
          Pawn p)
        {
            LordJob_Joinable_Court.outcomeChancesTemp.Clear();
            float num = 1f / LordJob_Joinable_Court.OutcomeThoughtChances.Sum<KeyValuePair<ThoughtDef, float>>((Func<KeyValuePair<ThoughtDef, float>, float>)(c =>
            {
                if (!LordJob_Joinable_Court.PositiveOutcome(c.Key))
                    return c.Value;
                return c.Value * p.GetStatValue(StatDefOf.SocialImpact, true);
            }));
            foreach (KeyValuePair<ThoughtDef, float> outcomeThoughtChance in LordJob_Joinable_Court.OutcomeThoughtChances)
                LordJob_Joinable_Court.outcomeChancesTemp.Add(new Tuple<ThoughtDef, float>(outcomeThoughtChance.Key, (LordJob_Joinable_Court.PositiveOutcome(outcomeThoughtChance.Key) ? outcomeThoughtChance.Value * p.GetStatValue(StatDefOf.SocialImpact, true) : outcomeThoughtChance.Value) * num));
            return (IEnumerable<Tuple<ThoughtDef, float>>)LordJob_Joinable_Court.outcomeChancesTemp;
        }
    }
}
