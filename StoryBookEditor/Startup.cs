/*********************************
 * (c) Christopher Wang / Steamfist Innovations
 * 10/6/2016
 * Please don't steal this code or use without permission
*********************************/

using System.Linq;
using UnityEditor;
using UnityEngine;

namespace StoryBookEditor
{
    /// <summary>
    /// Class to handle to start up of the project to install required UI elements onto the screen
    /// </summary>
    [InitializeOnLoad]
    public class Startup
    {
        public const string StoryBookInstanceName = "StoryBook";
        public const string SFXInstanceName = "SFXPlayer";
        protected static object updateLock = new object();
        protected static StoryBook _bookInstance = null;
        /// <summary>
        /// Called on the start of unity
        /// Set supported resolution and register with first update event
        /// </summary>
        static Startup()
        {
            Debug.ClearDeveloperConsole();
            Screen.SetResolution(800, 600, false);
            if(_bookInstance == null)
                EditorApplication.update += Update;
        }
        /// <summary>
        /// Called on first update of Aplication
        /// Need to do this on first update to get scene object
        /// Adds element to screen
        /// </summary>
        static void Update()
        {
            EditorApplication.update -= Update;
            lock (updateLock)
            {
                if (_bookInstance == null)
                {
                    var storyBookRoot = (from e in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects()
                                         where e.name == StoryBookInstanceName
                                         select e).FirstOrDefault();
                    if (storyBookRoot == default(GameObject))
                    {
                        storyBookRoot = new GameObject();
                        storyBookRoot.transform.localScale = new Vector3(1f, 1f);
                        _bookInstance = storyBookRoot.AddComponent<StoryBook>();
                        storyBookRoot.name = StoryBookInstanceName;

                        var bgMixer = storyBookRoot.AddComponent<AudioSource>();
                        bgMixer.playOnAwake = true;
                        bgMixer.loop = true;
                        _bookInstance.BackgroundMusic = bgMixer;
                    }
                    else
                    {
                        _bookInstance = storyBookRoot.GetComponent<StoryBook>();
                        var bgMixer = storyBookRoot.GetComponent<AudioSource>();
                        if (bgMixer == null)
                            bgMixer = storyBookRoot.AddComponent<AudioSource>();
                        bgMixer.playOnAwake = true;
                        bgMixer.loop = true;
                        _bookInstance.BackgroundMusic = bgMixer;
                    }

                    var sfxObj = (from e in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects()
                                         where e.name == SFXInstanceName
                                  select e).FirstOrDefault();
                    if(sfxObj == default(GameObject))
                    {
                        sfxObj = new GameObject();
                        sfxObj.name = SFXInstanceName;
                        var sfxMixer = sfxObj.AddComponent<AudioSource>();
                        sfxMixer.loop = false;
                        _bookInstance.SFX = sfxMixer;
                    }
                    else
                    {
                        var sfxMixer = sfxObj.GetComponent<AudioSource>();
                        if(sfxMixer == null)
                            sfxMixer = sfxObj.AddComponent<AudioSource>();
                        sfxMixer.loop = false;
                        _bookInstance.SFX = sfxMixer;
                    }
                }
                else
                    Debug.LogWarning("Already init book instance");
            }
        }
    }
}
