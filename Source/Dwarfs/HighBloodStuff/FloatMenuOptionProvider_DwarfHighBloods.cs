using RimWorld;
using RimWorld.Planet;
using System;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TheEndTimes_Dwarfs
{
    public class FloatMenuOptionProvider_DwarfHighBloods : FloatMenuOptionProvider
    {
        protected override bool Drafted
        {
            get
            {
                return true;
            }
        }

        protected override bool Undrafted
        {
            get
            {
                return true;
            }
        }

        protected override bool Multiselect
        {
            get
            {
                return false;
            }
        }

        protected override bool RequiresManipulation
        {
            get
            {
                return true;
            }
        }

        protected override FloatMenuOption GetSingleOptionFor(
          Thing clickedThing,
          FloatMenuContext context)
        {
            if (DwarfsUtil.IsDwarf(context.FirstSelectedPawn))
            {
                if (clickedThing.def.ingestible == null || !clickedThing.def.ingestible.showIngestFloatOption)
                    return (FloatMenuOption)null;
                if (!clickedThing.IngestibleNow || !context.FirstSelectedPawn.RaceProps.CanEverEat(clickedThing.def))
                    return (FloatMenuOption)null;

                if ((RH_TET_DwarfsMod.king != null && RH_TET_DwarfsMod.king.Equals(context.FirstSelectedPawn))
                    || (RH_TET_DwarfsMod.thanes != null && RH_TET_DwarfsMod.thanes.Count > 0 && RH_TET_DwarfsMod.thanes.Contains(context.FirstSelectedPawn)))
                {
                    CompDwarfHighBlood comp = context.FirstSelectedPawn.TryGetComp<CompDwarfHighBlood>();

                    if (comp == null)
                        Log.Error("Dwarf has null high blood comp.");

                    Thing t = clickedThing;
                    if ((t.def.ingestible != null && context.FirstSelectedPawn.RaceProps.CanEverEat(t) && t.IngestibleNow) && !(t.def.IsNonMedicalDrug && context.FirstSelectedPawn.IsTeetotaler()))
                    {
                        if (DwarfsUtil.InappropriateForHighBlood(t.def, context.FirstSelectedPawn, true))
                        {
                            FloatMenuOption fmoSave = null;
                            //foreach (FloatMenuOption fmo in opts)
                            //{
                            //    if (fmo.Label.Contains(t.LabelShort))
                            //    {
                            //        fmoSave = fmo;
                            //        break;
                            //    }
                            //}

                            fmoSave.Label += (": " + "RH_TET_Dwarfs_HighBloodFoodBad".Translate((NamedArgument)comp.highBloodComp.GetCurrentHighBlood().GetLabelFor(context.FirstSelectedPawn)));
                            fmoSave.Disabled = true;

                            return fmoSave;
                        }
                    }
                }
            }

            return null;
        }
    }
}