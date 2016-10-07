/*********************************
 * (c) Christopher Wang / Steamfist Innovations
 * 10/6/2016
 * Please don't steal this code or use without permission
*********************************/

namespace StoryBookEditor
{
    /// <summary>
    /// Interface to make a file service object
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// Operation to read the story book from path
        /// </summary>
        /// <param name="path">Path to where file lives</param>
        /// <returns>Instance of the book object or null if there is an issue</returns>
        StoryBook ReadBook(string path);
        /// <summary>
        /// Operation to save the book to the current path
        /// </summary>
        /// <param name="book">Book to save</param>
        /// <param name="path">Path to where file lives</param>
        /// <returns>If can save</returns>
        bool SaveBook(StoryBook book, string path);
    }
}
