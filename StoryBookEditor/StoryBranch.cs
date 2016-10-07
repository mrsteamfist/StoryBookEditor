/*********************************
 * (c) Christopher Wang / Steamfist Innovations
 * 10/6/2016
 * Please don't steal this code or use without permission
*********************************/

using System;
using UnityEngine;

namespace StoryBookEditor
{
    /// <summary>
    /// Story branch object
    /// </summary>
    [Serializable]
    public class StoryBranch
    {
        public string Id;
        public Vector2 ItemLocation;
        public Vector2 ItemSize;
        public string Image;
        public string NextPageId = string.Empty;
        public Sprite ImageSprite;
        public string NextPageName;

        public GameObject GameObj { get; set; }
        /// <summary>
        /// Ctor, inits ID
        /// </summary>
        public StoryBranch()
        {
            Id = Guid.NewGuid().ToString();
        }
        
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        /// <summary>
        /// Overload method to compare this object is equal to another
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            var otherBranch = obj as StoryBranch;
            if (otherBranch != null)
                return otherBranch.Id == Id;
            else if(obj is string)
            {
                string otherid = (string)obj;
                return Id == otherid;
            }
            return false;
        }
        /// <summary>
        /// Overload operator to compare if 2 branches are equal
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator ==(StoryBranch lhs, StoryBranch rhs)
        {
            if ((object)lhs == null && (object)rhs == null)
                return true;
            else if ((object)lhs == null || (object)rhs == null)
                return false;
            else
                return lhs.Equals(rhs);
        }
        /// <summary>
        /// Overload operator to compare if 2 branches are not equal
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator !=(StoryBranch lhs, StoryBranch rhs)
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
