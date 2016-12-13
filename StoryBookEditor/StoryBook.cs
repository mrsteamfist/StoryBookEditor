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
        protected const float WIDTH_PERCENTAGE = .0625f;
        protected const float HEIGHT_PERCENTAGE = .0833f;
        public const string TRANSITION_FADE = "FADING";
        public const string PARALAX_FADE = "PARALAXING";
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
        public AudioSource BackgroundMusic;
        public AudioSource SFX;
        public bool _isTransitioning = false;
        public string _transitionType = string.Empty;
        private const int drawDepth = -1000;
        public const float fadeSpeed = 0.5f;
        private float fadeAlpha = 0.0f;
        private int fadeDir = -1;
        #endregion
        #region Display requirements
        protected StoryBookModel _storyBook;
        private SpriteRenderer _spriteRenderer = null;
        private Vector3 _cameraSizeRatio;
        public Texture2D fadeOutTexture = null;
        public GameObject previousScreen = null;
        public GameObject nextScreen = null;
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
            if (fadeOutTexture == null)
            {
                fadeOutTexture = Resources.Load<Texture2D>("FadeImg");
                fadeOutTexture.wrapMode = TextureWrapMode.Clamp;
                fadeOutTexture.filterMode = FilterMode.Point;
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
                if (_cameraSizeRatio != null && _cameraSizeRatio != Vector3.zero)
                {
                    if (!string.IsNullOrEmpty(_storyBook.BackgroundMusic))
                    {
                        BackgroundMusicClip = Resources.Load<AudioClip>(_storyBook.BackgroundMusic);
                        BackgroundMusic.clip = BackgroundMusicClip;
                    }
                    DrawBook();
                }
            }
        }
        public float BeginFade(int direction = -1)
        {
            _isTransitioning = true;
            fadeDir = direction;
            if (direction == -1)
                fadeAlpha = 1f;
            else
                fadeAlpha = 0f;

            return fadeSpeed;
        }
        void OnGUI()
        {
            if (_isTransitioning)
            {
                if (previousScreen != null && nextScreen != null)
                {
                    if (!previousScreen.GetComponent<SpriteRenderer>().enabled)
                    {
                        previousScreen.transform.localPosition = Vector3.zero;
                        previousScreen.GetComponent<SpriteRenderer>().enabled=true;
                    }
                    if (!nextScreen.GetComponent<SpriteRenderer>().enabled)
                    {
                        nextScreen.transform.localPosition = new Vector3(11f,0,0);
                        nextScreen.GetComponent<SpriteRenderer>().enabled = true;
                    }
                    if(nextScreen.transform.localPosition.x < 1f)
                    {
                        _isTransitioning = false;
                        DestroyImmediate(nextScreen);
                        DestroyImmediate(previousScreen);
                        nextScreen = null;
                        previousScreen = null;
                    }
                    else
                    {
                        var tmp = nextScreen.transform.localPosition;
                        tmp.x -= Time.deltaTime * 5f;
                        nextScreen.transform.localPosition = tmp;
                        tmp = previousScreen.transform.localPosition;
                        tmp.x -= Time.deltaTime * 5f;
                        previousScreen.transform.localPosition = tmp;
                    }
                }
                else
                {
                    if (Time.deltaTime < .0001f)
                    {
                        System.Threading.Thread.SpinWait(60);
                        return;
                    }
                    fadeAlpha += fadeDir * fadeSpeed * Time.deltaTime;
                    fadeAlpha = Mathf.Clamp01(fadeAlpha);

                    GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, fadeAlpha);
                    GUI.depth = drawDepth;
                    GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), fadeOutTexture);

                    _isTransitioning = fadeAlpha != 0f && fadeAlpha != 1f;
                }
            }
        }
        /// <summary>
        /// Called on update
        /// Handles click events
        /// </summary>
        void Update()
        {
            if (transform != null && (transform.localScale.x == 0 || transform.localScale.y == 0))
                DrawBook();
            if(_isTransitioning)
            {
                
            }
            else if (Branches != null && Branches.Any() && Input.GetMouseButtonDown(0))
            {
                var clickX = (float)Math.Floor(Input.mousePosition.x / (Screen.width * WIDTH_PERCENTAGE));
                var clickY = (float)Math.Floor(Input.mousePosition.y / (Screen.height * HEIGHT_PERCENTAGE));

                var hitItem = Branches.Where(x => x.ItemLocation != null && x.ItemLocation.x <= clickX && clickX < x.ItemLocation.x + x.ItemSize.x &&
                        x.ItemLocation.y <= clickY && clickY < x.ItemLocation.y + x.ItemSize.y);
                if (hitItem.Any())
                {
                    if (!string.IsNullOrEmpty(hitItem.First().SFX) && SFX != null)
                    {
                        if (hitItem.First().SFXClip == null)
                            hitItem.First().SFXClip = Resources.Load<AudioClip>(hitItem.First().SFX);
                        if (hitItem.First().SFXClip != null)
                        {
                            SFX.PlayOneShot(hitItem.First().SFXClip, 1.0f);
                        }
                        else if (!string.IsNullOrEmpty(hitItem.First().SFX) && hitItem.First().SFXClip)
                        {
                            Debug.LogWarning("Unabled to load Sound effect for " + hitItem.First().SFX);
                        }
                    }
                    else if(SFX == null)
                    {
                        Debug.LogError("SFX broken");
                    }

                    LoadPage(hitItem.First().NextPageId, hitItem.First());
                    BeginFade(-1);
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                LoadPage(_initPage, null);
                BeginFade(-1);
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
            if(Branches.RemoveAll((b) => b.Id == id) > 0)
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
        public void AddBranchToPage(Vector2 loc, Vector2 size, Sprite sprite, AudioClip sfx, string nextPageName)
        {
            var branch = _storyBook.AddBranchToPage(loc, size, sprite, sfx, nextPageName, _currentId);
            
            if(branch != null)
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
                DrawBook();
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
            Branches.ForEach(x => x.SFXClip = string.IsNullOrEmpty(x.SFX) ? null : Resources.Load<AudioClip>(x.SFX));

            PageName = page.Name;
            #region Background Music
            if (string.IsNullOrEmpty(page.BackgroundMusic))
            {
                BackgroundMusic.Stop();
                BackgroundMusicClip = null;
                BackgroundMusic.clip = null;
            }
            else if (BackgroundMusicClip == null || (BackgroundMusicClip.name != page.BackgroundMusic))
            {
                if (BackgroundMusicClip != null && (BackgroundMusicClip.name != page.BackgroundMusic))
                {
                    BackgroundMusic.Stop();
                }

                BackgroundMusicClip = Resources.Load<AudioClip>(page.BackgroundMusic);
                //play fx
                if (BackgroundMusicClip && Application.isPlaying)
                {
                    BackgroundMusic.clip = BackgroundMusicClip;
                    BackgroundMusic.Play();
                }
            }
            else if(!BackgroundMusic.isPlaying && Application.isPlaying)
            {
                BackgroundMusic.time = 0;
                BackgroundMusic.Play();
            }
            #endregion
            if (previousScreen != null)
            {
                DestroyImmediate(previousScreen);
            }
            previousScreen = new GameObject("prevScreen");
            previousScreen.transform.localScale = gameObject.transform.localScale;
            var tmp = previousScreen.AddComponent<SpriteRenderer>();
            tmp.sprite = PageImage;
            tmp.enabled = false;
            //load bg
            if (string.IsNullOrEmpty(page.Background))
            {
                PageImage = Resources.Load<Sprite>("background");
            }
            else
            {
                PageImage = Resources.Load<Sprite>(page.Background);
            }
            if (nextScreen != null)
                DestroyImmediate(nextScreen);
            nextScreen = new GameObject("nextScreen");
            nextScreen.transform.localScale = gameObject.transform.localScale;
            tmp = nextScreen.AddComponent<SpriteRenderer>();
            tmp.sprite = PageImage;
            tmp.enabled = false;

            //determine if can back
            PageCanBack = _initPage != _currentId;
            //render ui
            DrawBook();
            
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
        /// <summary>
        /// Helper function to render the book onto the UI
        /// </summary>
        protected void DrawBook()
        {
            if (_storyBook == null)
            {
                Debug.LogError("Reached drawing step and was not able to retrieve story book");
            }

            if (_spriteRenderer == null && (_spriteRenderer = GetComponent<SpriteRenderer>()) == null)
                _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

            if (_spriteRenderer == null)
            {
                Debug.LogError("Unable to load Story Book sprite renderer, please restart unity");
                return;
            }

            _spriteRenderer.sprite = PageImage;
            if (_spriteRenderer.sprite != null)
            {
                if (_cameraSizeRatio == null)
                {
                    transform.localScale = new Vector3(1f, 1f);
                    Debug.LogWarning("Setting camera to init size");
                }
                else
                {
                    transform.localScale = new Vector3(_cameraSizeRatio.x / _spriteRenderer.sprite.bounds.size.x, _cameraSizeRatio.y / _spriteRenderer.sprite.bounds.size.y, 1);
                    if (transform.localScale == Vector3.zero)
                        Debug.LogError("Initialization error, please restart unity");
                }
            }
            else
            {
                Debug.LogWarning("Drawing scale will be off, not main image found");
            }
            #region Display Branch Info
            //clean out the old branches
            var children = new List<GameObject>();
            foreach (Transform child in transform) children.Add(child.gameObject);
            children.Where(x => x != null).ToList().ForEach(child => DestroyImmediate(child));

            if (Branches != null && Branches.Count > 0)
            {
                #region Sizing information for drawing the branch items
                var halfSizeWidth = _spriteRenderer.sprite == null ? 400f : _spriteRenderer.sprite.bounds.size.x / 2f;
                var halfSizeHeight = _spriteRenderer.sprite == null ? 300f : _spriteRenderer.sprite.bounds.size.y / 2f;
                var sectionWidth = _spriteRenderer.sprite == null ? 50f : _spriteRenderer.sprite.bounds.size.x / 16f;
                var sectionHeight = _spriteRenderer.sprite == null ? 50f : _spriteRenderer.sprite.bounds.size.y / 12f;
                var borderOffsetWidth = _spriteRenderer.sprite == null ? 25f : _spriteRenderer.sprite.bounds.size.x / 32f;
                var borderOffsetHeight = _spriteRenderer.sprite == null ? 25f : _spriteRenderer.sprite.bounds.size.y / 24f;
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
                        if (branch.ImageSprite != null)
                        {
                            //set branch location
                            branch.GameObj.transform.localPosition = new Vector3(
                                (sectionWidth * branch.ItemLocation.x - halfSizeWidth) + (borderOffsetWidth * branch.ItemSize.x),
                                (sectionHeight * branch.ItemLocation.y - halfSizeHeight) + (borderOffsetHeight * branch.ItemSize.y), 0f);
                            //get the relative size compared with the background
                            float relativeScaleWidth = 0f;
                            float relativeScaleHeight = 0f;
                            if (PageImage == null)
                            {
                                relativeScaleWidth = _cameraSizeRatio.x / branch.ImageSprite.bounds.size.x;
                                relativeScaleHeight = _cameraSizeRatio.y / branch.ImageSprite.bounds.size.y;
                            }
                            else
                            {
                                relativeScaleWidth = PageImage.bounds.size.x / branch.ImageSprite.bounds.size.x;
                                relativeScaleHeight = PageImage.bounds.size.y / branch.ImageSprite.bounds.size.y;
                            }
                            branch.GameObj.transform.localScale = new Vector3(.0625f * relativeScaleWidth * branch.ItemSize.x, .0833f * relativeScaleHeight * branch.ItemSize.y, 1);
                        }
                    }
                }
            }

            #endregion
        }
    }
}
