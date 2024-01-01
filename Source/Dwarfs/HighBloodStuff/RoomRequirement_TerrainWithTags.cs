using RimWorld;
using System.Collections.Generic;
using Verse;

namespace TheEndTimes_Dwarfs
{
    public class RoomRequirement_FloorsWithTags : RoomRequirement
    {
        public List<string> tags;

        public override bool Met(Room r, Pawn p)
        {
            foreach (IntVec3 cell in r.Cells) // for each cell in the room
            {
                List<string> tagsLoc = cell.GetTerrain(r.Map).tags; // get terrain tags for the cell
                if (tagsLoc.NullOrEmpty<string>())
                    return false;
                bool flag = false;
                foreach (string str in tagsLoc) // for each tag on cell
                {
                    if (this.tags.Contains(str)) // see if tag on cell matches tag on req
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                    return false;
            }
            return true;
        }

        public override bool SameOrSubsetOf(RoomRequirement other)
        {
            if (!base.SameOrSubsetOf(other))
                return false;
            RoomRequirement_FloorsWithTags requirementTerrainWithTags = (RoomRequirement_FloorsWithTags)other;
            foreach (string tag in this.tags)
            {
                if (!requirementTerrainWithTags.tags.Contains(tag))
                    return false;
            }
            return true;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            RoomRequirement_FloorsWithTags requirementTerrainWithTags = this;
            foreach (string str in base.ConfigErrors())
                yield return str;
            if (string.IsNullOrEmpty(requirementTerrainWithTags.labelKey))
                yield return "does not define a label key";
            if (requirementTerrainWithTags.tags.NullOrEmpty<string>())
                yield return "tags are null or empty";
        }
    }
}
