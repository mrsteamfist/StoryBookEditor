using System;
using UnityEngine;

namespace StoryBookEditor
{
    public class AnimationCompleteScript : MonoBehaviour
    {
        public event EventHandler AnimationCompleted;
        public void AnimationComplete()
        {
            var complete = AnimationCompleted;
            if (complete != null)
                complete(this, EventArgs.Empty);
        }
    }
}
