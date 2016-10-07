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

namespace StoryBookEditor
{
    /// <summary>
    /// Script to attach to the UI object to allow/launch UI
    /// </summary>
    [ExecuteInEditMode]
    public class BookObject : MonoBehaviour
    {
        #region Variables
        #region HelperValues
        protected const float WIDTH_PERCENTAGE = .0625f;
        protected const float HEIGHT_PERCENTAGE = .0833f;
        #endregion
        #region Property Names
        public const string PAGE_NAME_PROPERTY = "PageName";
        public const string PAGE_IMAGE_PROPERTY = "PageImage";
        public const string PAGE_CAN_BACK_PROPERTY = "PageCanBack";
        public const string BRANCHES_PROPERTY = "Branches";
        #endregion
        #region Current Page
        protected string _currentId = string.Empty;
        public string PageName = null;
        public Sprite PageImage = null;
        public bool PageCanBack = false;
        public Stack<string> BackStack = new Stack<string>();
        protected string _initPage = string.Empty;
        #endregion
        #region Current Data
        public List<StoryBranch> Branches = new List<StoryBranch>();
        private IFileService _fileService;
        #endregion
        #region Display requirements
        protected StoryBook _storyBook;
        private SpriteRenderer _spriteRenderer = null;
        private Vector3 _cameraSizeRatio;
        #endregion
        #endregion

        #region Unity Methods
        /// <summary>
        /// Called when first created
        /// Inits core data
        /// Handles loading
        /// </summary>
        void Start()
        {
            if (_spriteRenderer == null && (_spriteRenderer = GetComponent<SpriteRenderer>()) == null)
                _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

            if (Camera.main != null)
            {
                float worldScreenHeight = Camera.main.orthographicSize * 2f;
                float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;
                _cameraSizeRatio = new Vector3(worldScreenWidth, worldScreenHeight, 1);
            }

            //clear up startup junk
            var children = new List<GameObject>();
            foreach (Transform child in transform) children.Add(child.gameObject);
            children.ForEach(child => DestroyImmediate(child));
        }
        /// <summary>
        /// Called on loaded from scene
        /// Handles loading
        /// </summary>
        void Awake()
        {
            OnEnable();
        }
        /// <summary>
        /// Called on loaded from scene
        /// Handles loading
        /// </summary>
        void OnEnable()
        {
            if (_fileService == null)
                _fileService = new FileService();
            if (_storyBook == null)
            {
                if (File.Exists(FileService.PATH))
                    _storyBook = _fileService.ReadBook(FileService.PATH);
                #region Create book if it doesn't exist
                if (_storyBook == null)
                {
                    Debug.LogWarning("First time book load null, re-initing it");
                    _storyBook = new StoryBook();
                    _fileService.SaveBook(_storyBook, FileService.PATH);
                }
                #endregion
                #region Make new book if there isn't any
                if (_storyBook.Pages == null || !_storyBook.Pages.Any())
                {
                    var p = new StoryPage();
                    p.Name = "Default";
                    p.Background = "background";
                    _currentId = p.Id;
                    _storyBook.Pages.Add(p);

                    PageName = "Default";
                    PageImage = Resources.Load<Sprite>("background");
                    PageCanBack = false;
                    Branches.Clear();
                }
                #endregion
                else
                {
                    LoadPage(_storyBook.Pages.First().Id);
                }
                DrawBook();
            }
        }
        /// <summary>
        /// Called on update
        /// Handles click events
        /// </summary>
        void Update()
        {
            if (Branches != null && Branches.Any() && Input.GetMouseButtonDown(0))
            {
                var clickX = (float)Math.Floor(Input.mousePosition.x / (Screen.width * WIDTH_PERCENTAGE));
                var clickY = (float)Math.Floor(Input.mousePosition.y / (Screen.height * HEIGHT_PERCENTAGE));

                var hitItem = Branches.Where(x => x.ItemLocation.x <= clickX && clickX <= x.ItemLocation.x + x.ItemSize.x &&
                        x.ItemLocation.y <= clickY && clickY <= x.ItemLocation.y + x.ItemSize.y);
                if (hitItem.Any())
                {
                    LoadPage(hitItem.First().NextPageId);
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                LoadPage(_initPage);
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
            Branches.Where(x => x.Id == id && x.GameObj != null).ToList().ForEach(x => DestroyImmediate(x.GameObj));
            var branch = Branches.Where(x => x.Id == id).FirstOrDefault();
            if (branch != null)
                _storyBook.Pages.RemoveAll(x => branch.NextPageId == x.Id);
            Branches.RemoveAll((b) => b.Id == id);
            
            _storyBook.Branches.RemoveAll((b) => b.Id == id);
            if (_fileService != null)
                _fileService.SaveBook(_storyBook, FileService.PATH);
        }
        /// <summary>
        /// Adds a new branch to the current page and adds a page to that branch
        /// </summary>
        /// <param name="loc">Location of where the next item will be</param>
        /// <param name="size">Size of the next item</param>
        /// <param name="sprite">Sprite to show the next item</param>
        /// <param name="nextPageName">Title of that page the branch will navigate to</param>
        public void AddBranchToPage(Vector2 loc, Vector2 size, Sprite sprite, string nextPageName)
        {
            if (string.IsNullOrEmpty(nextPageName))
            {
                nextPageName = "Next Page " + _storyBook.Pages.Count.ToString();
            }
            var branch = new StoryBranch()
            {
                ImageSprite = sprite,
                ItemLocation = loc,
                ItemSize = size,
                NextPageName = nextPageName,
            };

            if (sprite != null)
            {
                branch.Image = sprite.name;
            }
            else
            {
                branch.Image = string.Empty;
            }
            var page = new StoryPage()
            {
                Name = nextPageName
            };
            var currentPage = (from p in _storyBook.Pages
                               where p.Id == _currentId
                               select p).FirstOrDefault();
            if (currentPage != null)
            {
                currentPage.Branches.Add(branch.Id);
            }
            branch.NextPageId = page.Id;
            _storyBook.Pages.Add(page);
            _storyBook.Branches.Add(branch);
            Branches.Add(branch);
            if (_fileService != null)
                _fileService.SaveBook(_storyBook, FileService.PATH);
            else
                Debug.LogWarning("Book update and no file service initialized");
        }
        /// <summary>
        /// Forces the book to update after editor performs operation on it
        /// </summary>
        public void BookUpdated()
        {
            if (_fileService == null)
                return;
            bool uiChange = false;
            //change occured, I need to update the book with the current page, branches
            #region Look for changes
            var matchingPage = (from p in _storyBook.Pages
                                where p.Id == _currentId
                                select p).FirstOrDefault();
            if (matchingPage != null)
            {
                #region Name
                if (matchingPage.Name != PageName)
                {
                    matchingPage.Name = PageName;
                }
                #endregion
                #region Image
                if (PageImage == null && !string.IsNullOrEmpty(matchingPage.Background))
                {
                    matchingPage.Background = string.Empty;
                    uiChange = true;
                }
                else if (PageImage != null && PageImage.name != matchingPage.Background)
                {
                    matchingPage.Background = PageImage.name;
                    uiChange = true;
                }
                #endregion
                #region Branches
                //get current matching branches
                //update each branch
                Branches.ToList().ForEach(x =>
                {
                    var index = _storyBook.Branches.ToList().IndexOf(x);
                    if (index >= 0)
                    {
                        if (_storyBook.Branches[index].Image != x.ImageSprite.name)
                        {
                            _storyBook.Branches[index].Image = x.ImageSprite.name;
                        }
                        if (_storyBook.Branches[index].ItemLocation != x.ItemLocation)
                        {
                            _storyBook.Branches[index].ItemLocation = x.ItemLocation;
                            uiChange = true;
                        }
                        if (_storyBook.Branches[index].ItemSize != x.ItemSize)
                        {
                            _storyBook.Branches[index].Image = x.Image;
                            uiChange = true;
                        }
                    }
                    else
                    {
                        Debug.LogError("Unable to find branch in book");
                    }
                });
                #endregion
            }
            else
            {
                Debug.LogError("Page updated, unable to find in story book");
            }
            #endregion
            _fileService.SaveBook(_storyBook, FileService.PATH);
            if (uiChange)
                DrawBook();
        }
        /// <summary>
        /// Public method to allow the loading of the page
        /// </summary>
        /// <param name="id">The page ID to load</param>
        public void LoadPage(string id)
        {
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

            PageName = page.Name;
            //load bg
            if (string.IsNullOrEmpty(page.Background))
            {
                PageImage = Resources.Load<Sprite>("background");
            }
            else
            {
                PageImage = Resources.Load<Sprite>(page.Background);
            }
            //determine if can back
            PageCanBack = _initPage != _currentId;
            //render ui
            DrawBook();
        }
        /// <summary>
        /// Function to the previous page
        /// </summary>
        public void LoadBack()
        {
            if (BackStack.Any())
            {
                LoadPage(BackStack.Pop());
            }
            else
            {
                LoadPage(_initPage);
            }
        }
        #endregion
        /// <summary>
        /// Helper function to render the book onto the UI
        /// </summary>
        protected void DrawBook()
        {
            if (_storyBook == null || _spriteRenderer == null)
                return;

            _spriteRenderer.sprite = PageImage;
            if (_spriteRenderer.sprite != null)
            {
                transform.localScale = new Vector3(_cameraSizeRatio.x / _spriteRenderer.sprite.bounds.size.x, _cameraSizeRatio.y / _spriteRenderer.sprite.bounds.size.y, 1);
            }
            #region Display Branch Info
            //clean out the old branches
            var children = new List<GameObject>();
            foreach (Transform child in transform) children.Add(child.gameObject);
            children.ForEach(child => DestroyImmediate(child));
            if (Branches != null && Branches.Count > 0)
            {
                #region Sizing information for drawing the branch items
                var halfSizeWidth = _spriteRenderer.sprite.bounds.size.x / 2f;
                var halfSizeHeight = _spriteRenderer.sprite.bounds.size.y / 2f;
                var sectionWidth = _spriteRenderer.sprite.bounds.size.x / 16f;
                var sectionHeight = _spriteRenderer.sprite.bounds.size.y / 12f;
                var borderOffsetWidth = _spriteRenderer.sprite.bounds.size.x / 32;
                var borderOffsetHeight = _spriteRenderer.sprite.bounds.size.y / 24;
                #endregion
                for (int i = 0; i < Branches.Count; i++)
                {
                    var branch = Branches[i];
                    if (!string.IsNullOrEmpty(branch.Image))
                    {
                        branch.ImageSprite = Resources.Load<Sprite>(branch.Image);
                        branch.GameObj = new GameObject();
                        branch.GameObj.name = "Branch " + branch.NextPageId;
                        branch.GameObj.transform.SetParent(transform);
                        var sR = branch.GameObj.AddComponent<SpriteRenderer>();
                        sR.sprite = branch.ImageSprite;
                        //set branch location
                        branch.GameObj.transform.localPosition = new Vector3(
                            (sectionWidth * branch.ItemLocation.x - halfSizeWidth) + (borderOffsetWidth * branch.ItemSize.x),
                            (sectionHeight * branch.ItemLocation.y - halfSizeHeight) + (borderOffsetHeight * branch.ItemSize.y), 0f);
                        //get the relative size compared with the background
                        var relativeScaleWidth = PageImage.bounds.size.x / branch.ImageSprite.bounds.size.x;
                        var relativeScaleHeight = PageImage.bounds.size.y / branch.ImageSprite.bounds.size.y;
                        branch.GameObj.transform.localScale = new Vector3(.0625f * relativeScaleWidth * branch.ItemSize.x, .0833f * relativeScaleHeight * branch.ItemSize.y, 1);
                    }
                }
            }
            #endregion
        }
    }
}
