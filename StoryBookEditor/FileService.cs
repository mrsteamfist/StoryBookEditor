/*********************************
 * (c) Christopher Wang / Steamfist Innovations
 * 10/6/2016
 * Please don't steal this code or use without permission
*********************************/

using System.IO;
using UnityEngine;

namespace StoryBookEditor
{
    /// <summary>
    /// Service to read and write to the story file
    /// </summary>
    public class FileService : IFileService
    {
        public static string PATH = System.IO.Path.Combine(Application.dataPath, @"Resources\game.story");
        static object fileLock = new object();
        /// <summary>
        /// Operation to read the story book from path
        /// </summary>
        /// <param name="path">Path to where file lives</param>
        /// <returns>Instance of the book object or null if there is an issue</returns>
        public StoryBookModel ReadBook(string path)
        {
            lock (fileLock)
            {
                StreamReader reader = new StreamReader(path);
                var json = reader.ReadToEnd();
                reader.Close();
                return JsonUtility.FromJson<StoryBookModel>(json);
            }
        }
        /// <summary>
        /// Operation to save the book to the current path
        /// </summary>
        /// <param name="book">Book to save</param>
        /// <param name="path">Path to where file lives</param>
        /// <returns>If can save</returns>
        public bool SaveBook(StoryBookModel storyBook, string path)
        {
            if (storyBook != null)
            {
                var json = JsonUtility.ToJson(storyBook);
                var writer = new StreamWriter(path);
                writer.Write(json);
                writer.Close();
                return true;
            }

            return false;
        }
    }
}
