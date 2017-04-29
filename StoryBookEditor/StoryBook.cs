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
        public const string PAGE_ANIMATION_PROPERTY = "PageAnimation";
        public const string PAGE_CAN_BACK_PROPERTY = "PageCanBack";
        public const string BRANCHES_PROPERTY = "Branches";
        public const string BACKGROUND_CLIP_PROPERTY = "BackgroundMusicClip";
        #endregion
        #region Current Page
        protected string _currentId = string.Empty;
        public string PageName = null;
        public Sprite PageImage = null;
        public AnimationClip PageAnimation = null;
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
        public StoryBookModel StoryBookData { get { return _storyBook; } }
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
                if (FileService.DoesFileExist())
                    _storyBook = _fileService.ReadBook(FileService.GetFileName());
                #region Create book if it doesn't exist
                if (_storyBook == null)
                {
                    Debug.LogWarning("First time book load null, re-initing it");
                    _storyBook = new StoryBookModel();
                    _fileService.SaveBook(_storyBook, FileService.GetFileName());
                }
                #endregion

                _screenManager.StoryBookInstance = _storyBook;

                _screenManager.TransitionComplete += (o, a) =>
                {
                    _currentId = null;
                    LoadPage(a.Item.NextPageId, a.Item);
                };

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
                    PageAnimation = null;

                    PageCanBack = false;
                    Branches.Clear();
                    _fileService.SaveBook(_storyBook, FileService.GetFileName());
                    LoadPage(_currentId, null);
                }
                #endregion
                else
                {
                    LoadPage(_storyBook.Pages.First().Id, null);
                }
            }
        }

        public void BranchClicked(StoryBranchModel hitItem)
        {
            if (hitItem != null)
            {
                //Set and clear variables
                _storyBook.SetVariables(hitItem);
                _storyBook.ClearVariables(hitItem);

                //Play SFX
                if (!string.IsNullOrEmpty(hitItem.SFX))
                {
                    if (hitItem.SFXClip == null)
                        hitItem.SFXClip = Resources.Load<AudioClip>(hitItem.SFX);
                    _audioManager.PlaySFX(hitItem.SFXClip);
                }
                //Perform Fades
                if (hitItem.TransitionType == TransitionTypes.Fade)
                {
                    _screenManager.BeginFade();
                }
                else if (hitItem.TransitionType == TransitionTypes.Slide)
                {
                    _screenManager.BeginSlide(hitItem.CurrentImageSprite, hitItem.NextImageSprite);
                }
                else
                {
                    _currentId = null;
                    LoadPage(hitItem.NextPageId, hitItem);
                }
                
            }
            else
            {
                _currentId = null;
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
                    _fileService.SaveBook(_storyBook, FileService.GetFileName());
            }
            if (Branches.RemoveAll((b) => b.Id == id) > 0)
            {
                BookUpdated();
            }
        }
        public void DeletePage(string id)
        {
            var loadPage = id;

            var page = _storyBook.Pages.Where(x => x.Id == id).FirstOrDefault();
            if(page != null)
            {
                _storyBook.Branches.RemoveAll(x => page.Branches.Contains(x.Id));
                _storyBook.Branches.RemoveAll(x => x.NextPageId == page.Id);

                if(id == _currentId)
                {
                    if(!_storyBook.Pages.Any())
                        _storyBook.Pages.Add(new StoryPageModel());
                    loadPage = _storyBook.Pages.First().Id;
                }

                if (_storyBook.Pages.RemoveAll((b) => b.Id == id) > 0)
                {
                    if (_fileService != null)
                        _fileService.SaveBook(_storyBook, FileService.GetFileName());
                }

                LoadPage(loadPage, null);
            }            
        }
        /// <summary>
        /// Adds a new branch to the current page and adds a page to that branch
        /// </summary>
        /// <param name="loc">Location of where the next item will be</param>
        /// <param name="size">Size of the next item</param>
        /// <param name="sprite">Sprite to show the next item</param>
        /// <param name="nextPageName">Title of that page the branch will navigate to</param>
        public void AddBranchToPage(Vector2 loc, Vector2 size, Sprite sprite, AudioClip sfx, string nextPageName)
        {
            var branch = _storyBook.AddBranchToPage(loc, size, sprite, sfx, nextPageName, _currentId);

            if (branch != null)
            {
                Branches.Add(branch);
                if (_fileService != null)
                    _fileService.SaveBook(_storyBook, FileService.GetFileName());
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
            if (!_storyBook.UpdatePage(_currentId, PageName, PageImage, PageAnimation, BackgroundMusicClip, Branches.ToArray()))
                Debug.LogError("Book update failed");
            else
            {
                _fileService.SaveBook(_storyBook, FileService.GetFileName());
                _screenManager.UpdateDraw(PageImage, PageAnimation, Branches);
            }
        }

        /// <summary>
        /// Public method to allow the loading of the page
        /// </summary>
        /// <param name="id">The page ID to load</param>
        public void LoadPage(string id, StoryBranchModel from)
        {
            var page = (from p in _storyBook.Pages
                        where p.Id == id
                        select p).FirstOrDefault();

            if (id != _currentId)
            {
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
                if (!string.IsNullOrEmpty(page.Animation))
                    PageAnimation = Resources.Load<AnimationClip>(page.Animation);
                else
                    PageAnimation = null;

                #region Background Music
                if (!string.IsNullOrEmpty(page.BackgroundMusic))
                    BackgroundMusicClip = Resources.Load<AudioClip>(page.BackgroundMusic);
                else
                    BackgroundMusicClip = null;
                _audioManager.PlayBackgroundMusic(BackgroundMusicClip);
                #endregion

                //determine if can back
                PageCanBack = _initPage != _currentId;
            }

            if (string.IsNullOrEmpty(_initPage))
                _initPage = _currentId;
            
            //render ui
            _screenManager.UpdateDraw(PageImage, PageAnimation, Branches);

            //Call event
            if (id != _currentId && PageChanged != null)
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
