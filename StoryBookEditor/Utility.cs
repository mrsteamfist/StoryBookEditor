using UnityEngine;

namespace StoryBookEditor
{
    public delegate void OnClickDelegate(StoryBranchModel id);
    public class Utilities
    {
        public static void CopySprite(Sprite source, string sourceName, out Sprite dest, out string destName)
        {
            if (source == null && string.IsNullOrEmpty(sourceName))
            {
                dest = null;
                destName = null;
            }
            else if (source != null)
            {
                destName = source.name;
                dest = source;
            }
            else
            {
                destName = sourceName;
                dest = Resources.Load<Sprite>(sourceName);
            }
        }

        public static void CopyAudioClip(AudioClip source, string sourceName, out AudioClip dest, out string destName)
        {
            if (source == null && string.IsNullOrEmpty(sourceName))
            {
                dest = null;
                destName = null;
            }
            else if (source != null)
            {
                destName = source.name;
                dest = source;
            }
            else
            {
                destName = sourceName;
                dest = Resources.Load<AudioClip>(sourceName);
            }
        }
    }
}