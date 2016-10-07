/*********************************
 * (c) Christopher Wang / Steamfist Innovations
 * 10/6/2016
 * Please don't steal this code or use without permission
*********************************/

using System.Collections.Generic;

namespace StoryBookEditor
{
    public class StoryBook
    {
        public List<StoryPage> Pages;
        public List<StoryBranch> Branches;

        public StoryBook()
        {
            Pages = new List<StoryPage>();
            Branches = new List<StoryBranch>();
        }

        public override bool Equals(object obj)
        {
            var other = obj as StoryBook;

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
