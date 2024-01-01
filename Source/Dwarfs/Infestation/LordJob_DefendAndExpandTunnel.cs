using RimWorld;
using Verse;
using Verse.AI.Group;

namespace TheEndTimes_Dwarfs
{
    public class LordJob_DefendAndExpandTunnel : LordJob
    {
        private bool aggressive;

        public override bool CanBlockHostileVisitors
        {
            get
            {
                return false;
            }
        }

        public override bool AddFleeToil
        {
            get
            {
                return false;
            }
        }

        public LordJob_DefendAndExpandTunnel()
        {
        }

        public LordJob_DefendAndExpandTunnel(SpawnedPawnParams parms)
        {
            this.aggressive = parms.aggressive;
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil_DefendAndExpandTunnel defendAndExpandTunnel = new LordToil_DefendAndExpandTunnel();
            defendAndExpandTunnel.distToTunnelToAttack = 10f;
            stateGraph.StartingToil = (LordToil)defendAndExpandTunnel;
            LordToil_DefendTunnelAggressively tunnelAggressively = new LordToil_DefendTunnelAggressively();
            tunnelAggressively.distToTunnelToAttack = 40f;
            stateGraph.AddToil((LordToil)tunnelAggressively);
            LordToil_AssaultColony toilAssaultColony = new LordToil_AssaultColony(false);
            stateGraph.AddToil((LordToil)toilAssaultColony);
            Transition transition1 = new Transition((LordToil)defendAndExpandTunnel, this.aggressive ? (LordToil)toilAssaultColony : (LordToil)tunnelAggressively, false, true);
            transition1.AddTrigger((Trigger)new Trigger_PawnHarmed(0.5f, true, (Faction)null));
            transition1.AddTrigger((Trigger)new Trigger_PawnLostViolently(false));
            transition1.AddTrigger((Trigger)new Trigger_Memo(SkavenTunnel.MemoAttackedByEnemy));
            transition1.AddTrigger((Trigger)new Trigger_Memo(SkavenTunnel.MemoBurnedBadly));
            transition1.AddTrigger((Trigger)new Trigger_Memo(SkavenTunnel.MemoDestroyedNonRoofCollapse));
            transition1.AddTrigger((Trigger)new Trigger_Memo(HediffGiver_Heat.MemoPawnBurnedByAir));
            transition1.AddPostAction((TransitionAction)new TransitionAction_EndAllJobs());
            stateGraph.AddTransition(transition1, false);
            Transition transition2 = new Transition((LordToil)defendAndExpandTunnel, (LordToil)toilAssaultColony, false, true);
            transition2.AddTrigger((Trigger)new Trigger_PawnHarmed(0.5f, false, this.Map.ParentFaction));
            transition2.AddPostAction((TransitionAction)new TransitionAction_EndAllJobs());
            stateGraph.AddTransition(transition2, false);
            Transition transition3 = new Transition((LordToil)tunnelAggressively, (LordToil)toilAssaultColony, false, true);
            transition3.AddTrigger((Trigger)new Trigger_PawnHarmed(0.5f, false, this.Map.ParentFaction));
            transition3.AddPostAction((TransitionAction)new TransitionAction_EndAllJobs());
            stateGraph.AddTransition(transition3, false);
            Transition transition4 = new Transition((LordToil)defendAndExpandTunnel, (LordToil)defendAndExpandTunnel, true, true);
            transition4.AddTrigger((Trigger)new Trigger_Memo(SkavenTunnel.MemoDeSpawned));
            stateGraph.AddTransition(transition4, false);
            Transition transition5 = new Transition((LordToil)tunnelAggressively, (LordToil)tunnelAggressively, true, true);
            transition5.AddTrigger((Trigger)new Trigger_Memo(SkavenTunnel.MemoDeSpawned));
            stateGraph.AddTransition(transition5, false);
            Transition transition6 = new Transition((LordToil)toilAssaultColony, (LordToil)defendAndExpandTunnel, false, true);
            transition6.AddSource((LordToil)tunnelAggressively);
            transition6.AddTrigger((Trigger)new Trigger_TicksPassedWithoutHarmOrMemos(1200, new string[5]
            {
                SkavenTunnel.MemoAttackedByEnemy,
                SkavenTunnel.MemoBurnedBadly,
                SkavenTunnel.MemoDestroyedNonRoofCollapse,
                SkavenTunnel.MemoDeSpawned,
                HediffGiver_Heat.MemoPawnBurnedByAir
            }));
            transition6.AddPostAction((TransitionAction)new TransitionAction_EndAttackBuildingJobs());
            stateGraph.AddTransition(transition6, false);
            return stateGraph;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look<bool>(ref this.aggressive, "skavenAggressive", false, false);
        }
    }
}
