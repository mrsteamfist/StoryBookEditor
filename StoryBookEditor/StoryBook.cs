/*********************************
 * (c) Christopher Wang / Steamfist Innovations
 * 10/6/2016
 * Please don't steal this code or use without permission
*********************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEngine.Audio;

namespace StoryBookEditor
{
    /// <summary>
    /// Script to attach to the UI object to allow/launch UI
    /// </summary>
    [ExecuteInEditMode]
    public class StoryBook : MonoBehaviour
    {
        #region Variables
        #region HelperValues
        #endregion
        #region Property Names
        public const string PAGE_NAME_PROPERTY = "PageName";
        public const string PAGE_IMAGE_PROPERTY = "PageImage";
        public const string PAGE_CAN_BACK_PROPERTY = "PageCanBack";
        public const string BRANCHES_PROPERTY = "Branches";
        public const string BACKGROUND_CLIP_PROPERTY = "BackgroundMusicClip";
        #endregion
        #region Current Page
        protected string _currentId = string.Empty;
        public string PageName = null;
        public Sprite PageImage = null;
        public bool PageCanBack = false;
        public Stack<string> BackStack = new Stack<string>();
        protected string _initPage = string.Empty;
        public AudioClip BackgroundMusicClip;
        #endregion
        #region Current Data
        public event EventHandler<PageChangedEventArgs> PageChanged;
        public List<StoryBranchModel> Branches = new List<StoryBranchModel>();
        private IFileService _fileService;
        #endregion
        #region Display requirements
        protected StoryBookModel _storyBook;
        protected ScreenManager _screenManager;
        protected AudioManager _audioManager;
        #endregion
        #endregion

        #region Unity Methods        
        /// <summary>
        /// Called on loaded from scene
        /// Handles loading
        /// </summary>
        void OnEnable()
        {
            if (_fileService == null)
                _fileService = new FileService();

            if (_screenManager == null && (_screenManager = GetComponent<ScreenManager>()) == null)
            {
                _screenManager = gameObject.AddComponent<ScreenManager>();
                _screenManager.OnEnable();
            }
            _screenManager.ItemClickDelegate = BranchClicked;

            if (_audioManager == null && (_audioManager = GetComponent<AudioManager>()) == null)
            {
                _audioManager = gameObject.AddComponent<AudioManager>();
                _audioManager.OnEnable();
            }

            if (_storyBook == null)
            {
                if (File.Exists(FileService.PATH))
                    _storyBook = _fileService.ReadBook(FileService.PATH);
                #region Create book if it doesn't exist
                if (_storyBook == null)
                {
                    Debug.LogWarning("First time book load null, re-initing it");
                    _storyBook = new StoryBookModel();
                    _fileService.SaveBook(_storyBook, FileService.PATH);
                }
                #endregion
                #region Make new book if there isn't any
                if (_storyBook.Pages == null || !_storyBook.Pages.Any())
                {
                    var p = new StoryPageModel();
                    p.Name = "Default";
                    p.Background = "background";
                    _currentId = p.Id;
                    _storyBook.Pages.Add(p);

                    PageName = "Default";
                    PageImage = Resources.Load<Sprite>("background");
                    if (PageImage == null)
                        Debug.LogWarning("Unable to load default background sprite");
                    PageCanBack = false;
                    Branches.Clear();
                    _fileService.SaveBook(_storyBook, FileService.PATH);
                    LoadPage(_currentId, null);
                }
                #endregion
                else
                {
                    LoadPage(_storyBook.Pages.First().Id, null);
                }
            }
        }

        public void BranchClicked(string id)
        {
            var hitItem = Branches.Where(x => x.Id == id);
            if (id != null && hitItem != null && hitItem.Any())
            {
                if (!string.IsNullOrEmpty(hitItem.First().SFX))
                {
                    if (hitItem.First().SFXClip == null)
                        hitItem.First().SFXClip = Resources.Load<AudioClip>(hitItem.First().SFX);
                    _audioManager.PlaySFX(hitItem.First().SFXClip);
                }
                if(hitItem.First().TransitionType == TransitionTypes.Fade)
                {
                    _screenManager.TransitionComplete += (o, a) =>
                    {
                        LoadPage(hitItem.First().NextPageId, hitItem.First());
                    };
                    _screenManager.BeginFade();
                }
                else if(hitItem.First().TransitionType == TransitionTypes.Slide)
                {
                    _screenManager.TransitionComplete += (o, a) =>
                    {
                        LoadPage(hitItem.First().NextPageId, hitItem.First());
                    };
                    _screenManager.BeginSlide(hitItem.First().CurrentImageSprite, hitItem.First().NextImageSprite);
                }
                else
                    LoadPage(hitItem.First().NextPageId, hitItem.First());
                
            }
            else
            {
                LoadPage(_initPage, null);
            }
        }

        #endregion
        #region Public Facing Functions
        /// <summary>
        /// Delete the given branch from the UI and the book
        /// </summary>
        /// <param name="id">The ID of the branch to remove</param>
        public void DeleteBranch(string id)
        {
            if (_storyBook.Branches.RemoveAll((b) => b.Id == id) > 0)
            {
                if (_fileService != null)
                    _fileService.SaveBook(_storyBook, FileService.PATH);
            }
            if (Branches.RemoveAll((b) => b.Id == id) > 0)
            {
                BookUpdated();
            }
        }
        /// <summary>
        /// Adds a new branch to the current page and adds a page to that branch
        /// </summary>
        /// <param name="loc">Location of where the next item will be</param>
        /// <param name="size">Size of the next item</param>
        /// <param name="sprite">Sprite to show the next item</param>
        /// <param name="nextPageName">Title of that page the branch will navigate to</param>
        public void AddBranchToPage(Vector2 loc, Vector2 size, Sprite sprite, AudioClip sfx,
            TransitionTypes transition, int transitionLength, Sprite currentImage, Sprite nextImage, string nextPageName)
        {
            var branch = _storyBook.AddBranchToPage(loc, size, sprite, sfx, transition, transitionLength, currentImage, nextImage, nextPageName, _currentId);

            if (branch != null)
            {
                Branches.Add(branch);
                if (_fileService != null)
                    _fileService.SaveBook(_storyBook, FileService.PATH);
                else
                    Debug.LogWarning("Book update and no file service initialized");
                BookUpdated();
            }
            else
            {
                Debug.LogError("Failed to save branch");
            }
        }
        /// <summary>
        /// Forces the book to update after editor performs operation on it
        /// </summary>
        public void BookUpdated()
        {
            if (_fileService == null)
                return;
            //change occured, I need to update the book with the current page, branches
            if (!_storyBook.UpdatePage(_currentId, PageName, PageImage, BackgroundMusicClip, Branches.ToArray()))
                Debug.LogError("Book update failed");
            else
            {
                _fileService.SaveBook(_storyBook, FileService.PATH);
                _screenManager.UpdateDraw(PageImage, Branches);
            }
        }

        /// <summary>
        /// Public method to allow the loading of the page
        /// </summary>
        /// <param name="id">The page ID to load</param>
        public void LoadPage(string id, StoryBranchModel from)
        {
            if (id == _currentId)
                return;

            var page = (from p in _storyBook.Pages
                        where p.Id == id
                        select p).FirstOrDefault();
            if (page == null)
            {
                page = (from p in _storyBook.Pages
                        select p).FirstOrDefault();
                _currentId = page.Id;
            }
            else if (Branches.Where(b => b.NextPageId == id).Any())
            {
                BackStack.Push(_currentId);
            }

            _currentId = id;

            if (string.IsNullOrEmpty(_initPage))
                _initPage = _currentId;
            //load the branches
            Branches = (from b in _storyBook.Branches
                        where page.Branches.Contains(b.Id)
                        select b).ToList();
            Branches.ForEach(x => x.LoadResourcesFromStrings());

            PageName = page.Name;
            if (!string.IsNullOrEmpty(page.Background))
                PageImage = Resources.Load<Sprite>(page.Background);
            else
                PageImage = null;

            #region Background Music
            if (!string.IsNullOrEmpty(page.BackgroundMusic))
                BackgroundMusicClip = Resources.Load<AudioClip>(page.BackgroundMusic);
            else
                BackgroundMusicClip = null;
            _audioManager.PlayBackgroundMusic(BackgroundMusicClip);
            #endregion

            //determine if can back
            PageCanBack = _initPage != _currentId;
            //render ui
            _screenManager.UpdateDraw(PageImage, Branches);

            //Call event
            if (PageChanged != null)
            {
                PageChanged(page, new PageChangedEventArgs() { Branch = from });
            }
        }
        /// <summary>
        /// Function to the previous page
        /// </summary>
        public void LoadBack()
        {
            if (BackStack.Any())
            {
                LoadPage(BackStack.Pop(), null);
            }
            else
            {
                LoadPage(_initPage, null);
            }
        }
        #endregion
    }
}
