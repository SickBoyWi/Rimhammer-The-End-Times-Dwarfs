using UnityEngine;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class Graphic_Variants : Graphic_Collection
    {
        public override Material MatSingle
        {
            get
            {
                return this.GetDefaultMat();
            }
        }

        public override Material MatAt(Rot4 rot, Verse.Thing thing = null)
        {
            return this.MatSingleFor(thing);
        }

        public override Material MatSingleFor(Verse.Thing thing)
        {
            IGraphicVariantProvider graphicVariantProvider = thing as IGraphicVariantProvider;
            if (graphicVariantProvider == null)
                return this.GetDefaultMat();
            int graphicVariant = graphicVariantProvider.GraphicVariant;
            if (graphicVariant >= 0 && graphicVariant <= this.subGraphics.Length)
                return this.subGraphics[graphicVariant].MatSingleFor(thing);
            Log.Error(string.Format("No material with index {0} available, as requested by {1}", (object)graphicVariant, (object)thing.GetType()));
            return this.GetDefaultMat();
        }

        private Material GetDefaultMat()
        {
            if ((uint)this.subGraphics.Length > 0U)
                return this.subGraphics[0].MatSingle;
            return base.MatSingle;
        }

        public override Graphic GetColoredVersion(
          Shader newShader,
          Color newColor,
          Color newColorTwo)
        {
            return GraphicDatabase.Get<Graphic_Single>(this.path, newShader, this.drawSize, newColor, newColorTwo, this.data, (string)null);
        }
    }
}
