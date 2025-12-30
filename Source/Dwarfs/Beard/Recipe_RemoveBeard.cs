using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class Recipe_RemoveBeard : RecipeWorker
    {
        protected virtual bool SpawnPartsWhenRemoved
        {
            get
            {
                return true;
            }
        }

        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(
          Pawn pawn,
          RecipeDef recipe)
        {
            if (DwarfsUtil.IsDwarf(pawn) && pawn.gender == Gender.Male)
            {
                BodyPartRecord part = pawn.health.hediffSet.GetBodyPartRecord(RH_TET_DwarfDefOf.RH_TET_Dwarfs_BP_JawBeard);

                if (part != null)
                {
                    yield return part;
                }
            }
        }

        public override bool IsViolationOnPawn(Pawn pawn, BodyPartRecord part, Faction billDoerFaction)
        {
            return true;
        }

        public override void ApplyOnPawn(
          Pawn pawn,
          BodyPartRecord part,
          Pawn billDoer,
          List<Thing> ingredients,
          Bill bill)
        {
            bool flag1 = MedicalRecipesUtility.IsClean(pawn, part);
            bool flag2 = this.IsViolationOnPawn(pawn, part, Faction.OfPlayer);
            if (billDoer != null)
            {
                if (this.CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                    return;
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, (object)billDoer, (object)pawn);
                pawn.health.hediffSet.GetDirectlyAddedPartFor(part)?.Notify_SurgicallyRemoved(billDoer);
                if (this.SpawnPartsWhenRemoved)
                {
                    GenSpawn.Spawn(RH_TET_DwarfDefOf.RH_TET_Dwarfs_BodyPart_Beard, billDoer.Position, billDoer.Map, WipeMode.Vanish);
                    MedicalRecipesUtility.SpawnThingsFromHediffs(pawn, part, billDoer.Position, billDoer.Map);
                }
            }
            this.DamagePart(pawn, part);
            pawn.Drawer.renderer.SetAllGraphicsDirty();
            if (flag1)
                this.ApplyThoughts(pawn, billDoer);
            if (!flag2)
                return;
            this.ReportViolation(pawn, billDoer, pawn.HomeFaction, -70, (HistoryEventDef)null);
        }

        public virtual void DamagePart(Pawn pawn, BodyPartRecord part)
        {
            pawn.TakeDamage(new DamageInfo(DamageDefOf.SurgicalCut, 99999f, 999f, -1f, (Thing)null, part, (ThingDef)null, DamageInfo.SourceCategory.ThingOrUnknown, (Thing)null, true, true, QualityCategory.Normal, true, false));
        }

        public virtual void ApplyThoughts(Pawn pawn, Pawn billDoer)
        {
            if (pawn.Dead)
                ThoughtUtility.GiveThoughtsForPawnExecuted(pawn, billDoer, PawnExecutionKind.OrganHarvesting);
            else
                ThoughtUtility.GiveThoughtsForPawnOrganHarvested(pawn, billDoer);
        }

        //public override string GetLabelWhenUsedOn(Pawn pawn, BodyPartRecord part)
        //{
        //    if (!part.def.removeRecipeLabelOverride.NullOrEmpty())
        //        return part.def.removeRecipeLabelOverride;
        //    if (pawn.RaceProps.IsMechanoid || pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(part))
        //    {
        //        Hediff hediff;
        //        return pawn.health.hediffSet.TryGetDirectlyAddedPartFor(part, out hediff) ? (string)"RemovePart".Translate((NamedArgument)hediff.Label) : RecipeDefOf.RemoveBodyPart.label;
        //    }
        //    switch (HealthUtility.PartRemovalIntent(pawn, part))
        //    {
        //        case BodyPartRemovalIntent.Harvest:
        //            return (string)"HarvestOrgan".Translate();
        //        case BodyPartRemovalIntent.Amputate:
        //            return part.depth == BodyPartDepth.Inside || part.def.socketed ? (string)"RemoveOrgan".Translate() : (string)"Amputate".Translate();
        //        default:
        //            throw new InvalidOperationException();
        //    }
        //}

        protected bool CheckSurgeryFail(
          Pawn surgeon,
          Pawn patient,
          List<Thing> ingredients,
          BodyPartRecord part,
          Bill bill)
        {
            if (this.recipe.surgeryOutcomeEffect == null)
                return false;
            SurgeryOutcome outcome = this.recipe.surgeryOutcomeEffect.GetOutcome(this.recipe, surgeon, patient, ingredients, part, bill);
            if ((outcome != null ? (outcome.failure ? 1 : 0) : 0) == 0)
                return false;
            if (this.recipe.addsHediffOnFailure != null)
                patient.health.AddHediff(this.recipe.addsHediffOnFailure, part, new DamageInfo?(), (DamageWorker.DamageResult)null);
            return true;
        }

        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            if (!(thing is Pawn p))
                return false;

            Gender? genderPrerequisite = this.recipe.genderPrerequisite;

            if (DwarfsUtil.IsDwarf(p) && p.gender == genderPrerequisite)
            {
                BodyPartRecord partBeard = p.health.hediffSet.GetBodyPartRecord(RH_TET_DwarfDefOf.RH_TET_Dwarfs_BP_JawBeard);

                if (partBeard != null)
                {
                    return true;
                }
            }

            // TODO Consider lodgers, mutants, age, etc.
            //return ((this.recipe.allowedForQuestLodgers || !p.IsQuestLodger()) && (this.recipe.minAllowedAge <= 0 || p.ageTracker.AgeBiologicalYears >= this.recipe.minAllowedAge)) && ((!this.recipe.developmentalStageFilter.HasValue || this.recipe.developmentalStageFilter.Value.Has(p.DevelopmentalStage)) && (!this.recipe.humanlikeOnly || p.RaceProps.Humanlike)) && (!ModsConfig.AnomalyActive || (this.recipe.mutantBlacklist == null || !p.IsMutant || !this.recipe.mutantBlacklist.Contains(p.mutant.Def)) && (this.recipe.mutantPrerequisite == null || p.IsMutant && this.recipe.mutantPrerequisite.Contains(p.mutant.Def)));
            return false;
        }
            
        public virtual bool CompletableEver(Pawn surgeryTarget)
        {
            return true;
        }
    }
}