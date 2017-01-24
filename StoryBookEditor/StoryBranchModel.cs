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
    public class StoryBranchModel
    {
        public string Id;
        public Vector2 ItemLocation;
        public Vector2 ItemSize;
        public string Image;
        public string NextPageId = string.Empty;
        public string NextPageName;
        public string SFX;
        public TransitionTypes TransitionType = TransitionTypes.None;
        public int TransitionLength = 1000;
        public string CurrentImage;
        public Sprite CurrentImageSprite;
        public string NextImage;
        public Sprite NextImageSprite;

        public Sprite ImageSprite;
        public AudioClip SFXClip;

        public GameObject GameObj { get; set; }
        /// <summary>
        /// Ctor, inits ID
        /// </summary>
        public StoryBranchModel()
        {
            Id = Guid.NewGuid().ToString();
        }
        
        public void CopyObjsIntoStrings()
        {
            CurrentImage = CurrentImageSprite == null ? null : CurrentImageSprite.name;
            NextImage = NextImageSprite == null ? null : NextImageSprite.name;
            SFX = SFXClip == null ? null : SFXClip.name;
            Image = ImageSprite == null ? null : ImageSprite.name;
        }

        public void LoadResourcesFromStrings()
        {
            CurrentImageSprite = string.IsNullOrEmpty(CurrentImage) ? null : Resources.Load<Sprite>(CurrentImage);
            NextImageSprite = string.IsNullOrEmpty(NextImage) ? null : Resources.Load<Sprite>(NextImage);
            SFXClip = string.IsNullOrEmpty(SFX) ? null : Resources.Load<AudioClip>(SFX);
            ImageSprite = string.IsNullOrEmpty(Image) ? null : Resources.Load<Sprite>(Image);
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
            var otherBranch = obj as StoryBranchModel;
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
        public static bool operator ==(StoryBranchModel lhs, StoryBranchModel rhs)
        {
            if ((object)lhs == null && (object)rhs == null)
                return true;
            else if ((object)lhs == null || (object)rhs == null)
                return false;
            else
                return lhs.Equals(rhs);
        }
        public StoryBranchModel CopyFrom(StoryBranchModel other)
        {
            if (other == null)
                return null;

            Id = other.Id;
            ItemLocation = other.ItemLocation;
            ItemSize = other.ItemSize;
            Utilities.CopySprite(other.ImageSprite, other.Image, out ImageSprite, out Image);
            NextPageId = other.NextPageId;
            NextPageName = other.NextPageName;
            TransitionType = other.TransitionType;
            TransitionLength = other.TransitionLength;
            Utilities.CopySprite(other.CurrentImageSprite, other.CurrentImage, out CurrentImageSprite, out CurrentImage);
            Utilities.CopySprite(other.NextImageSprite, other.NextImage, out NextImageSprite, out NextImage);
            Utilities.CopyAudioClip(other.SFXClip, other.SFX, out SFXClip, out SFX);
            GameObj = other.GameObj;
            
            return this;
    }

        /// <summary>
        /// Overload operator to compare if 2 branches are not equal
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator !=(StoryBranchModel lhs, StoryBranchModel rhs)
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
