using RimWorld;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class WallLightGlower : ThingWithComps
    {
        private CompFlickable compFlick;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.compFlick = (CompFlickable)this.GetComp<CompFlickable>();
        }

        public override void Destroy(DestroyMode mode = 0)
        {
        }

        public void ToggleGlower(bool onOrNot)
        {
            if (this.compFlick == null)
                return;
            this.compFlick.SwitchIsOn = onOrNot;
        }

        public WallLightGlower()
            : base()
        {
        }
    }
}
