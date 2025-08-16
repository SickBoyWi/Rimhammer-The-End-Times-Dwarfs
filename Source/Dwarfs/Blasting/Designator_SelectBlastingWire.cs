using HugsLib.Utils;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class Designator_SelectBlastingWire : Designator
    {
        public Designator_SelectBlastingWire()
        {
            this.hotKey = KeyBindingDefOf.Misc10;
            this.icon = RH_TET_DwarfDefOf.Textures.CommandButton_SelectDesigWireUI;
            this.useMouseIcon = true;
            this.defaultLabel = "RH_TET_Dwarfs_BlastingWireDesigLabel".Translate();
            this.defaultDesc = "RH_TET_Dwarfs_BlastingWireDesig".Translate();
            this.soundDragSustain = SoundDefOf.Designate_DragStandard;
            this.soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            this.soundSucceeded = SoundDefOf.ThingSelected;
            this.hasDesignateAllFloatMenuOption = true;
        }

        public override void DesignateThing(Thing t)
        {
            if (!HugsLibUtility.ShiftIsHeld)
                Find.Selector.ClearSelection();
            this.CellDesignate(t.Position);
            this.TryCloseArchitectMenu();
        }

        public override string Label
        {
            get
            {
                return "RH_TET_Dwarfs_BlastingWireDesigLabel".Translate();
            }
        }

        public override string Desc
        {
            get
            {
                return "RH_TET_Dwarfs_BlastingWireDesig".Translate();
            }
        }

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

        public override AcceptanceReport CanDesignateCell(IntVec3 loc)
        {
            List<Verse.Thing> thingList = this.Map.thingGrid.ThingsListAt(loc);
            if (thingList != null)
            {
                for (int index = 0; index < thingList.Count; ++index)
                {
                    if (this.IsSelectable(thingList[index]))
                        return (AcceptanceReport)true;
                }
            }
            return (AcceptanceReport)false;
        }

        public override AcceptanceReport CanDesignateThing(Verse.Thing t)
        {
            return (AcceptanceReport)this.IsSelectable(t);
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            if (!HugsLibUtility.ShiftIsHeld)
                Find.Selector.ClearSelection();
            this.CellDesignate(c);
            this.TryCloseArchitectMenu();
        }

        public override void DesignateMultiCell(IEnumerable<IntVec3> cells)
        {
            if (!HugsLibUtility.ShiftIsHeld)
                Find.Selector.ClearSelection();
            foreach (IntVec3 cell in cells)
                this.CellDesignate(cell);
            this.TryCloseArchitectMenu();
        }

        private bool IsSelectable(Verse.Thing t)
        {
            return t.def?.building is BuildingProperties_BlastingWire;
        }

        private void CellDesignate(IntVec3 cell)
        {
            List<Verse.Thing> thingList = this.Map.thingGrid.ThingsListAt(cell);
            Selector selector = Find.Selector;
            if (thingList == null)
                return;
            for (int index = 0; index < thingList.Count; ++index)
            {
                Verse.Thing t = thingList[index];
                if (this.IsSelectable(t) && !selector.SelectedObjects.Contains((object)t))
                {
                    selector.SelectedObjects.Add((object)t);
                    SelectionDrawer.Notify_Selected((object)t);
                }
            }
        }

        private void TryCloseArchitectMenu()
        {
            if (Find.Selector.NumSelected == 0 || Find.MainTabsRoot.OpenTab != MainButtonDefOf.Architect)
                return;
            Find.MainTabsRoot.EscapeCurrentTab(true);
        }
    }
}
