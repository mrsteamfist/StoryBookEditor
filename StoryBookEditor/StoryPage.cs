/*********************************
 * (c) Christopher Wang / Steamfist Innovations
 * 10/6/2016
 * Please don't steal this code or use without permission
*********************************/

using System;
using System.Collections.Generic;

namespace StoryBookEditor
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class StoryPage
    {
        public List<string> Branches;
        public string Name;
        public string Background;
        public string Id;
        /// <summary>
        /// 
        /// </summary>
        public StoryPage()
        {
            Id = Guid.NewGuid().ToString();
            Branches = new List<string>();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if(obj == null)
                return false;

            var other = obj as StoryPage;
            if (other == null)
                return false;

            return Id == other.Id && Name == other.Name && Background == other.Background && Branches.Count == other.Branches.Count;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator == (StoryPage lhs, StoryPage rhs)
        {
            if ((object)lhs == null && (object)rhs == null)
                return true;
            else if ((object)lhs == null || (object)rhs == null)
                return false;
            else
                return lhs.Equals(rhs);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator !=(StoryPage lhs, StoryPage rhs)
        {
            if ((object)lhs == null && (object)rhs == null)
                return false;
            else if ((object)lhs == null || (object)rhs == null)
                return true;
            else
                return !lhs.Equals(rhs);
        }
    }
}
