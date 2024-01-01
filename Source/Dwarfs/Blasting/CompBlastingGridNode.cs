using Verse;

namespace TheEndTimes_Dwarfs
{
    public abstract class CompBlastingGridNode : ThingComp
    {
        private IntVec3 cachedPosition = IntVec3.Invalid;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!(this.parent is Building))
                return;
            this.cachedPosition = this.parent.Position;
            this.parent.Map.mapDrawer.MapMeshDirty(this.parent.Position, MapMeshFlag.Buildings);
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            if (!this.cachedPosition.IsValid)
                return;
            map.mapDrawer.MapMeshDirty(this.cachedPosition, MapMeshFlag.Buildings);
        }

        public abstract void PrintForDetonationGrid(SectionLayer layer);

        protected void PrintConnection(SectionLayer layer)
        {
            GraphicDatabase.Get<Graphic_Single>("Things/Building/Blasting/RH_TET_Dwarfs_BlastWireAtlas", ShaderDatabase.MetaOverlay).Print(layer, (Verse.Thing)this.parent, 0f);
        }

        protected void PrintEndpoint(SectionLayer layer)
        {
            GraphicDatabase.Get<Graphic_Single>("Things/Building/Blasting/RH_TET_Dwarfs_Blasting_EndPoint", ShaderDatabase.MetaOverlay).Print(layer, (Verse.Thing)this.parent, 0f);
        }
    }
}
