using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace StoryBookEditor
{
    //public 

    public class SpriteHolder : MonoBehaviour
    {
        public string AnimationState;
        public string SpriteEntity;
        public bool Loop;

        protected GameObject _holder;

        protected Animator Animator { get; set; }
        protected Animation Animation { get; set; }
        protected SpriteRenderer Sprite { get; set; }

        public SpriteHolder()
        {
            _holder = new GameObject();
            _holder.transform.parent = transform;
        }

        public bool LoadAnimation()
        {
            if (SpriteEntity != null)
            {
                if (AnimationState != null)
                {
                    var animation = Resources.Load<AnimationClip>(AnimationState);
                    if (animation != null)
                    {
                        Animation = _holder.AddComponent<Animation>();
                        animation.legacy = true;
                        Animation.clip = animation;
                        if(animation.isLooping)
                        {
                            Animation.playAutomatically = true;
                        }
                    }
                }
                if (AnimationState != null)
                {
                    var animator = Resources.Load<RuntimeAnimatorController>(SpriteEntity);
                    if(animator != null)
                    {
                        Animator = _holder.AddComponent<Animator>();
                        Animator.runtimeAnimatorController = animator;
                    }
                }
                var sprite = Resources.Load<Sprite>(SpriteEntity);
                if (sprite != null)
                {
                    Sprite = _holder.AddComponent<SpriteRenderer>();
                    Sprite.sprite = sprite;
                }
                else
                    return false;

                return true;
            }
            return false;
        }

        public IEnumerator PlayAnimation()
        {
            Animation.Play(AnimationState);
            if(!Animation.clip.isLooping)
            {
                do
                {
                    yield return null;
                } while (Animation.IsPlaying(AnimationState));
            }
        }
    }
}
