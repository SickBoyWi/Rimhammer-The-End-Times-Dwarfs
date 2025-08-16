using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class Designator_DecorateWall : Designator
    {
        //public override int DraggableDimensions
        //{
        //    get
        //    {
        //        return 2;
        //    }
        //}

        public override bool DragDrawMeasurements
        {
            get
            {
                return true;
            }
        }

        public Designator_DecorateWall()
        {
            this.defaultLabel = (string)"RH_TET_Dwarfs_Designator_DecorateWallLabel".Translate();
            this.defaultDesc = (string)"RH_TET_Dwarfs_Designator_DecorateWallDesc".Translate();
            this.icon = ContentFinder<Texture2D>.Get("UI/Designators/RH_TET_Dwarfs_DecWall", true);
            this.useMouseIcon = true;
            this.soundDragSustain = SoundDefOf.Designate_DragStandard;
            this.soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            this.soundSucceeded = SoundDefOf.Designate_SmoothSurface;
            this.hotKey = KeyBindingDefOf.Misc1;
        }

        public override AcceptanceReport CanDesignateThing(Thing t)
        {
            if (t != null && t.def != null && t.def.defName.Contains("Smoothed") && this.CanDesignateCell(t.Position).Accepted)
                return AcceptanceReport.WasAccepted;
            return (AcceptanceReport)false;
        }

        public override void DesignateThing(Thing t)
        {
            this.DesignateSingleCell(t.Position);
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(this.Map))
                return (AcceptanceReport)false;
            if (c.Fogged(this.Map))
                return (AcceptanceReport)false;
            if (c.InNoBuildEdgeArea(this.Map))
                return (AcceptanceReport)"TooCloseToMapEdge".Translate();
            Building building = c.GetFirstBuilding(this.Map);
            if (building == null)
                return (AcceptanceReport)false;
            if (building != null && this.Map.designationManager.DesignationOn(building, RH_TET_DwarfDefOf.RH_TET_Dwarfs_DecorateWallDes) != null)
                return (AcceptanceReport)"RH_TET_Dwarfs_AlreadyBeingDecorated".Translate();
            if (building != null && building.def.defName.Contains("Smoothed") && !building.def.defName.EndsWith("_Decorated"))
            {
                if (!DebugSettings.godMode && !RH_TET_DwarfDefOf.RH_TET_Dwarf_WallsAdvanced.IsFinished)
                {
                    return (AcceptanceReport)"RH_TET_Dwarfs_ResearchIncomplete".Translate(RH_TET_DwarfDefOf.RH_TET_Dwarf_WallsAdvanced.label);
                }
                if (building.Faction == null)
                    building.SetFaction(Faction.OfPlayer);
                return AcceptanceReport.WasAccepted;
            }
            return (AcceptanceReport)"RH_TET_Dwarfs_NotDecoratable".Translate();
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            Building building = c.GetFirstBuilding(this.Map);
            this.Map.designationManager.RemoveAllDesignationsOn(building);
            this.Map.designationManager.AddDesignation(new Designation(building, RH_TET_DwarfDefOf.RH_TET_Dwarfs_DecorateWallDes));
        }

        public override void SelectedUpdate()
        {
            GenUI.RenderMouseoverBracket();
        }

        public override void RenderHighlight(List<IntVec3> dragCells)
        {
            DesignatorUtility.RenderHighlightOverSelectableCells((Designator)this, dragCells);
        }
    }
}
