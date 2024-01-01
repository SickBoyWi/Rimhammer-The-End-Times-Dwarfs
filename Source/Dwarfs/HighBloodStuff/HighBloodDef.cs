using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class HighBloodDef : Def
    {
        public float commonality = 1f;
        public float recruitmentResistanceFactor = 1f;
        public List<AbilityDef> grantedAbilities = new List<AbilityDef>();
        public int seniority;
        [MustTranslate]
        public string labelFemale;
        public WorkTags disabledWorkTags;
        public QualityCategory requiredMinimumApparelQuality;
        public ExpectationDef minExpectation;
        [NoTranslate]
        public List<string> tags;
        public ThoughtDef awardThought;
        public List<RoomRequirement> highBloodRoomRequirements;
        public List<RoomRequirement> bedroomRequirements;
        public float recruitmentDifficultyOffset;
        public float recruitmentResistanceOffset;
        public HighBloodFoodRequirement foodRequirement;
        public IntRange courtCooldown;
        [Unsaved(false)]
        private List<ThingDef> satisfyingMealsCached;
        [Unsaved(false)]
        private List<ThingDef> satisfyingMealsNoDrugsCached;

        // These aren't used yet in the XML.
        public List<HighBloodDef.ApparelRequirement> requiredApparel;
        public List<JoyKindDef> disabledJoyKinds;

        public IEnumerable<WorkTypeDef> DisabledWorkTypes
        {
            get
            {
                List<WorkTypeDef> list = DefDatabase<WorkTypeDef>.AllDefsListForReading;
                for (int i = 0; i < list.Count; ++i)
                {
                    if ((this.disabledWorkTags & list[i].workTags) != WorkTags.None)
                        yield return list[i];
                }
            }
        }

        public float MinThroneRoomQuality
        {
            get
            {
                if (this.highBloodRoomRequirements.NullOrEmpty<RoomRequirement>())
                    return 0.0f;
                RoomRequirement_Quality requiredQuality = this.highBloodRoomRequirements.OfType<RoomRequirement_Quality>().FirstOrDefault<RoomRequirement_Quality>();
                if (requiredQuality == null)
                    return 0.0f;
                return (float)requiredQuality.impressiveness;
            }
        }

        public string GetLabelFor(Pawn p)
        {
            if (p == null)
                return this.GetLabelForBothGenders();
            return this.GetLabelFor(p.gender);
        }

        public string GetLabelFor(Gender g)
        {
            if (g != Gender.Female)
                return this.label;
            if (string.IsNullOrEmpty(this.labelFemale))
                return this.label;
            return this.labelFemale;
        }

        public string GetLabelForBothGenders()
        {
            if (!string.IsNullOrEmpty(this.labelFemale))
                return this.label + " / " + this.labelFemale;
            return this.label;
        }

        public string GetLabelCapForBothGenders()
        {
            if (!string.IsNullOrEmpty(this.labelFemale))
                return (string)(this.LabelCap + " / " + this.labelFemale.CapitalizeFirst());
            return (string)this.LabelCap;
        }

        public string GetLabelCapFor(Pawn p)
        {
            return this.GetLabelFor(p).CapitalizeFirst((Def)this);
        }

        public IEnumerable<RoomRequirement> GetBedroomRequirements(Pawn p)
        {
            if (p.story.traits.HasTrait(TraitDefOf.Ascetic))
                return (IEnumerable<RoomRequirement>)null;
            return (IEnumerable<RoomRequirement>)this.bedroomRequirements;
        }

        public bool JoyKindDisabled(JoyKindDef joyKind)
        {
            if (this.disabledJoyKinds == null)
                return false;
            return this.disabledJoyKinds.Contains(joyKind);
        }

        private bool HasSameRoomRequirement(RoomRequirement otherReq, List<RoomRequirement> list)
        {
            if (list == null)
                return false;
            foreach (RoomRequirement roomRequirement in list)
            {
                if (roomRequirement.SameOrSubsetOf(otherReq))
                    return true;
            }
            return false;
        }

        public bool HasSameThroneroomRequirement(RoomRequirement otherReq)
        {
            return this.HasSameRoomRequirement(otherReq, this.highBloodRoomRequirements);
        }

        public bool HasSameBedroomRequirement(RoomRequirement otherReq)
        {
            return this.HasSameRoomRequirement(otherReq, this.bedroomRequirements);
        }

        public IEnumerable<ThingDef> SatisfyingMeals(bool includeDrugs = true)
        {
            if (includeDrugs)
            {
                if (this.satisfyingMealsCached == null)
                    this.satisfyingMealsCached = DefDatabase<ThingDef>.AllDefsListForReading.Where<ThingDef>((Func<ThingDef, bool>)(t => this.foodRequirement.Acceptable(t))).OrderByDescending<ThingDef, FoodPreferability>((Func<ThingDef, FoodPreferability>)(t => t.ingestible.preferability)).ToList<ThingDef>();
            }
            else if (this.satisfyingMealsNoDrugsCached == null)
                this.satisfyingMealsNoDrugsCached = DefDatabase<ThingDef>.AllDefsListForReading.Where<ThingDef>((Func<ThingDef, bool>)(t =>
                {
                    if (this.foodRequirement.Acceptable(t))
                        return !t.IsDrug;
                    return false;
                })).OrderByDescending<ThingDef, FoodPreferability>((Func<ThingDef, FoodPreferability>)(t => t.ingestible.preferability)).ToList<ThingDef>();
            if (!includeDrugs)
                return (IEnumerable<ThingDef>)this.satisfyingMealsNoDrugsCached;
            return (IEnumerable<ThingDef>)this.satisfyingMealsCached;
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(
          StatRequest req)
        {
            if (this.requiredMinimumApparelQuality > QualityCategory.Awful)
            {
                TaggedString taggedString = "RH_TET_Dwarfs_HighBloodTooltipRequiredApparelQuality".Translate();
                string str = this.requiredMinimumApparelQuality.GetLabel().CapitalizeFirst();
                yield return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, (string)taggedString, str, str, 99998, (string)null, (IEnumerable<Dialog_InfoCard.Hyperlink>)null, false);
            }
            if (!this.requiredApparel.NullOrEmpty<HighBloodDef.ApparelRequirement>())
            {
                TaggedString taggedString1 = "RH_TET_Dwarfs_HighBloodTooltipRequiredApparel".Translate();
                TaggedString taggedString2 = "Male".Translate().CapitalizeFirst() + ":\n" + this.RequiredApparelListForGender(Gender.Male).ToLineList("  - ", false) + "\n\n" + "Female".Translate().CapitalizeFirst() + ":\n" + this.RequiredApparelListForGender(Gender.Female).ToLineList("  - ", false);
                yield return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, (string)taggedString1, "", (string)("RH_TET_Dwarfs_HighBloodRequiredApparelStatDescription".Translate() + ":\n\n" + taggedString2), 99998, (string)null, (IEnumerable<Dialog_InfoCard.Hyperlink>)null, false);
            }
            if (!this.bedroomRequirements.NullOrEmpty<RoomRequirement>())
            {
                TaggedString taggedString = "RH_TET_Dwarfs_HighBloodTooltipBedroomRequirements".Translate();
                string valueString = this.bedroomRequirements.Select<RoomRequirement, string>((Func<RoomRequirement, string>)(r => r.Label((Room)null))).ToCommaList(false).CapitalizeFirst();
                string lineList = this.bedroomRequirements.Select<RoomRequirement, string>((Func<RoomRequirement, string>)(r => r.LabelCap((Room)null))).ToLineList("  - ", false);
                yield return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, (string)taggedString, valueString, lineList, 99997, (string)null, (IEnumerable<Dialog_InfoCard.Hyperlink>)null, false);
            }
            if (!this.highBloodRoomRequirements.NullOrEmpty<RoomRequirement>())
            {
                TaggedString taggedString = "RH_TET_Dwarfs_HighBloodTooltipRoomRequirements".Translate();
                string valueString = this.highBloodRoomRequirements.Select<RoomRequirement, string>((Func<RoomRequirement, string>)(r => r.Label((Room)null))).ToCommaList(false).CapitalizeFirst();
                string lineList = ((IList<string>)this.highBloodRoomRequirements.Select<RoomRequirement, string>((Func<RoomRequirement, string>)(r => r.LabelCap((Room)null))).ToArray<string>()).ToLineList("  - ");
                yield return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, (string)taggedString, valueString, lineList, 99997, (string)null, (IEnumerable<Dialog_InfoCard.Hyperlink>)null, false);
            }
            IEnumerable<string> strings = this.disabledWorkTags.GetAllSelectedItems<WorkTags>().Where<WorkTags>((Func<WorkTags, bool>)(t => (uint)t > 0U)).Select<WorkTags, string>((Func<WorkTags, string>)(w => w.LabelTranslated()));
            if (strings.Any<string>())
            {
                TaggedString taggedString = "RH_TET_Dwarfs_DisabledWorkTypes".Translate();
                string valueString = strings.ToCommaList(false).CapitalizeFirst();
                string lineList = strings.ToLineList(" -  ", true);
                yield return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, (string)taggedString, valueString, lineList, 99994, (string)null, (IEnumerable<Dialog_InfoCard.Hyperlink>)null, false);
            }
            
            if (this.foodRequirement.Defined && this.SatisfyingMeals(true).Any<ThingDef>())
                yield return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, (string)"RH_TET_Dwarfs_HighBloodRequiredMeals".Translate(), this.SatisfyingMeals(true).Select<ThingDef, string>((Func<ThingDef, string>)(m => m.label)).ToCommaList(false).CapitalizeFirst(), (string)"RH_TET_Dwarfs_HighBloodRequiredMealsDesc".Translate(), 99995, (string)null, (IEnumerable<Dialog_InfoCard.Hyperlink>)null, false);
        }

        private IEnumerable<string> RequiredApparelListForGender(Gender g)
        {
            foreach (TaggedString taggedString in this.requiredApparel.SelectMany<HighBloodDef.ApparelRequirement, ThingDef>((Func<HighBloodDef.ApparelRequirement, IEnumerable<ThingDef>>)(r => r.AllRequiredApparel(g))).Select<ThingDef, TaggedString>((Func<ThingDef, TaggedString>)(a => a.LabelCap)))
                yield return (string)taggedString;
            yield return (string)"RH_TET_Dwarfs_HighBloodRequirementAnyPowerArmor".Translate();
        }

        public override IEnumerable<string> ConfigErrors()
        {
            HighBloodDef highBloodDef = this;
            foreach (string str in base.ConfigErrors())
                yield return str;
            if (highBloodDef.awardThought != null && !typeof(Thought_MemoryHighBlood).IsAssignableFrom(highBloodDef.awardThought.thoughtClass))
                yield return string.Format("High blood {0} has awardThought with thoughtClass {1} which is not deriving from Thought_MemoryHighBlood!", (object)highBloodDef.defName, (object)highBloodDef.awardThought.thoughtClass.FullName);
            if (!highBloodDef.highBloodRoomRequirements.NullOrEmpty<RoomRequirement>())
            {
                foreach (RoomRequirement throneRoomRequirement in highBloodDef.highBloodRoomRequirements)
                {
                    RoomRequirement req = throneRoomRequirement;
                    foreach (object configError in req.ConfigErrors())
                        yield return string.Format("Room requirement {0}: {1}", (object)req.GetType().Name, configError);
                    req = (RoomRequirement)null;
                }
            }
        }

        public class ApparelRequirement
        {
            public List<BodyPartGroupDef> bodyPartGroupsMatchAny;
            public List<string> requiredTags;
            public List<string> allowedTags;

            public IEnumerable<ThingDef> AllAllowedApparelForPawn(
              Pawn p,
              bool ignoreGender = false,
              bool includeWorn = false)
            {
                HighBloodDef.ApparelRequirement apparelRequirement = this;
                foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefsListForReading)
                {
                    ThingDef apparel = thingDef;

                    Predicate<String> predicate = (String s) => (!apparelRequirement.requiredTags.Contains(s) ? apparelRequirement.allowedTags.Contains(s) : true);
                    Predicate<BodyPartGroupDef> predicate2 = (BodyPartGroupDef b) => (apparelRequirement.bodyPartGroupsMatchAny.Contains(b));
                    if (apparel.IsApparel && apparel.apparel.tags != null && (ignoreGender || apparel.apparel.CorrectGenderForWearing(p.gender)) && (apparel.apparel.tags.Any<string>(predicate) && apparel.apparel.bodyPartGroups.Any<BodyPartGroupDef>(new Predicate<BodyPartGroupDef>(predicate2)) && (includeWorn || !p.apparel.WornApparel.Any<Apparel>((Predicate<Apparel>)(w => w.def == apparel)))))
                        yield return apparel;
                }
            }

            public IEnumerable<ThingDef> AllRequiredApparelForPawn(
              Pawn p,
              bool ignoreGender = false,
              bool includeWorn = false)
            {
                HighBloodDef.ApparelRequirement apparelRequirement = this;
                foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefsListForReading)
                {
                    ThingDef apparel = thingDef;
                    Predicate<String> predicate = (String s) => (apparelRequirement.requiredTags.Contains(s));
                    Predicate<BodyPartGroupDef> predicate2 = (BodyPartGroupDef b) => (apparelRequirement.bodyPartGroupsMatchAny.Contains(b));
                    if (apparel.IsApparel && apparel.apparel.tags != null && (ignoreGender || apparel.apparel.CorrectGenderForWearing(p.gender)) && (apparel.apparel.tags.Any<string>(new Predicate<string>(predicate)) && apparel.apparel.bodyPartGroups.Any<BodyPartGroupDef>(new Predicate<BodyPartGroupDef>(predicate2)) && (includeWorn || !p.apparel.WornApparel.Any<Apparel>((Predicate<Apparel>)(w => w.def == apparel)))))
                        yield return apparel;
                }
            }

            public IEnumerable<ThingDef> AllRequiredApparel(Gender gender = Gender.None)
            {
                HighBloodDef.ApparelRequirement apparelRequirement = this;
                foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefsListForReading)
                {
                    Predicate<String> predicate = (String s) => (apparelRequirement.requiredTags.Contains(s));
                    Predicate<BodyPartGroupDef> predicate2 = (BodyPartGroupDef b) => (apparelRequirement.bodyPartGroupsMatchAny.Contains(b));
                    if (thingDef.IsApparel && thingDef.apparel.tags != null && (thingDef.apparel.tags.Any<string>(new Predicate<string>(predicate)) && thingDef.apparel.bodyPartGroups.Any<BodyPartGroupDef>(new Predicate<BodyPartGroupDef>(predicate2))) && (gender == Gender.None || thingDef.apparel.CorrectGenderForWearing(gender)))
                        yield return thingDef;
                }
            }

            public bool ApparelMeetsRequirement(ThingDef thingDef, bool allowUnmatched = true)
            {
                bool flag = false;
                for (int index = 0; index < this.bodyPartGroupsMatchAny.Count; ++index)
                {
                    if (thingDef.apparel.bodyPartGroups.Contains(this.bodyPartGroupsMatchAny[index]))
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                    return allowUnmatched;
                for (int index = 0; index < this.requiredTags.Count; ++index)
                {
                    if (thingDef.apparel.tags.Contains(this.requiredTags[index]))
                        return true;
                }
                if (this.allowedTags != null)
                {
                    for (int index = 0; index < this.allowedTags.Count; ++index)
                    {
                        if (thingDef.apparel.tags.Contains(this.allowedTags[index]))
                            return true;
                    }
                }
                return false;
            }

            public bool IsMet(Pawn p)
            {
                foreach (Apparel apparel in p.apparel.WornApparel)
                {
                    bool flag = false;
                    for (int index = 0; index < this.bodyPartGroupsMatchAny.Count; ++index)
                    {
                        if (apparel.def.apparel.bodyPartGroups.Contains(this.bodyPartGroupsMatchAny[index]))
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (flag)
                    {
                        for (int index = 0; index < this.requiredTags.Count; ++index)
                        {
                            if (apparel.def.apparel.tags.Contains(this.requiredTags[index]))
                                return true;
                        }
                        if (this.allowedTags != null)
                        {
                            for (int index = 0; index < this.allowedTags.Count; ++index)
                            {
                                if (apparel.def.apparel.tags.Contains(this.allowedTags[index]))
                                    return true;
                            }
                        }
                    }
                }
                return false;
            }

            public ThingDef RandomRequiredApparelForPawnInGeneration(
              Pawn p,
              Func<ThingDef, bool> validator)
            {
                ThingDef result = (ThingDef)null;
                if (!DefDatabase<ThingDef>.AllDefsListForReading.Where<ThingDef>((Func<ThingDef, bool>)(a =>
                {
                    if (!a.IsApparel || a.apparel.tags == null || !a.apparel.bodyPartGroups.Any<BodyPartGroupDef>((Predicate<BodyPartGroupDef>)(b => this.bodyPartGroupsMatchAny.Contains(b))) || (!a.apparel.tags.Any<string>((Predicate<string>)(t => this.requiredTags.Contains(t))) || !a.apparel.CorrectGenderForWearing(p.gender)))
                        return false;
                    if (validator != null)
                        return validator(a);
                    return true;
                })).TryRandomElementByWeight<ThingDef>((Func<ThingDef, float>)(a => a.generateCommonality), out result))
                    return (ThingDef)null;
                return result;
            }

            public override string ToString()
            {
                if (this.allowedTags == null)
                    return string.Format("({0}) -> {1}", (object)string.Join(",", this.bodyPartGroupsMatchAny.Select<BodyPartGroupDef, string>((Func<BodyPartGroupDef, string>)(a => a.defName)).ToArray<string>()), (object)string.Join(",", this.requiredTags.ToArray()));
                return string.Format("({0}) -> {1}|{2}", (object)string.Join(",", this.bodyPartGroupsMatchAny.Select<BodyPartGroupDef, string>((Func<BodyPartGroupDef, string>)(a => a.defName)).ToArray<string>()), (object)string.Join(",", this.requiredTags.ToArray()), (object)string.Join(",", this.allowedTags.ToArray()));
            }
        }
    }
}
