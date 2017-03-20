/*********************************
 * (c) Christopher Wang / Steamfist Innovations
 * 10/6/2016
 * Please don't steal this code or use without permission
*********************************/

using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
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
        protected static object updateLock = new object();
        protected static StoryBook _bookInstance = null;
#if TARGET_SCENE
        private static string _currentScene = null;
#endif
        /// <summary>
        /// Called on the start of unity
        /// Set supported resolution and register with first update event
        /// </summary>
        static Startup()
        {
            Debug.ClearDeveloperConsole();
            if (_bookInstance == null)
                EditorApplication.update += Update;
        }

        public static StoryBook BookInstance { get { return _bookInstance; } }

        /// <summary>
        /// Called on first update of Aplication
        /// Need to do this on first update to get scene object
        /// Adds element to screen
        /// </summary>
        static void Update()
        {
#if !TARGET_SCENE
            EditorApplication.update -= Update;
#endif
            lock (updateLock)
            {
#if TARGET_SCENE
                if (_currentScene != UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
                {
                    _currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                    if (FileService.DoesFileExist())
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
                        }
                        else
                        {
                            _bookInstance = storyBookRoot.GetComponent<StoryBook>();
                        }
                    }
                }
#else
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
                    }
                    else
                    {
                        _bookInstance = storyBookRoot.GetComponent<StoryBook>();
                    }
                }
#endif
            }
        }
    }
}
