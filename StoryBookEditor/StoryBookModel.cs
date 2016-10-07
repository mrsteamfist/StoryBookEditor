/*********************************
 * (c) Christopher Wang / Steamfist Innovations
 * 10/6/2016
 * Please don't steal this code or use without permission
*********************************/

using System.Collections.Generic;

namespace StoryBookEditor
{
    public class StoryBookModel
    {
        public List<StoryPageModel> Pages;
        public List<StoryBranchModel> Branches;

        public StoryBookModel()
        {
            Pages = new List<StoryPageModel>();
            Branches = new List<StoryBranchModel>();
        }

        public override bool Equals(object obj)
        {
            var other = obj as StoryBookModel;

            if (obj == null || other == null)
            {
                return false;
            }
            if (Pages.Count != other.Pages.Count || Branches.Count != other.Branches.Count)
                return false;
            for (int i = 0; i < Pages.Count; i++)
            {
                if (Pages[i] != other.Pages[i])
                    return false;
            }
            for (int i = 0; i < Branches.Count; i++)
            {
                if (Branches[i] != other.Branches[i])
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return (Branches.GetHashCode() >> 10) + Pages.GetHashCode();
        }
    }
}
