using UnityEngine;

namespace StoryBookEditor 
{
    public class AudioManager : MonoBehaviour
    {
        protected AudioSource BackgroundMusic;
        protected AudioSource SFX;

        public const string SFXInstanceName = "SFXPlayer";

        public void OnEnable()
        {
            BackgroundMusic = GetComponent<AudioSource>();
            if (BackgroundMusic == null)
            {
                BackgroundMusic = gameObject.AddComponent<AudioSource>();
                BackgroundMusic.playOnAwake = true;
                BackgroundMusic.loop = true;
            }

            var sfxGameObject = GameObject.Find("SFXPlayer");
            if(sfxGameObject == null)
            {
                sfxGameObject = new GameObject();
                sfxGameObject.name = SFXInstanceName;
                SFX = sfxGameObject.AddComponent<AudioSource>();
            }
            else
            {
                SFX = sfxGameObject.GetComponent<AudioSource>();
                if (SFX == null)
                    SFX = sfxGameObject.AddComponent<AudioSource>();
            }
            SFX.loop = false;
        }

        public void PlayBackgroundMusic(AudioClip bgMusic)
        {
            if(BackgroundMusic == null)
            {
                OnEnable();
                if (BackgroundMusic == null)
                {
                    Debug.LogError("Background Music Player Failed to Load, please restart unity");
                    return;
                }
            }
            if (bgMusic == null)
            {
                BackgroundMusic.Stop();
                BackgroundMusic.clip = null;
            }
            else if (BackgroundMusic.clip == null || (BackgroundMusic.clip.name != bgMusic.name))
            {
                if (BackgroundMusic.clip != null && (BackgroundMusic.clip.name != bgMusic.name))
                {
                    BackgroundMusic.Stop();
                }

                //play bg music from scratch
                if (Application.isPlaying)
                {
                    BackgroundMusic.clip = bgMusic;
                    BackgroundMusic.Play();
                }
            }
            else if (!BackgroundMusic.isPlaying && Application.isPlaying)
            {
                //restart the music cuz it stopped
                BackgroundMusic.time = 0;
                BackgroundMusic.Play();
            }
            //else don't do anything
        }

        public void PlaySFX(AudioClip sfx)
        {
            SFX.PlayOneShot(sfx, 1.0f);
        }
    }
}
