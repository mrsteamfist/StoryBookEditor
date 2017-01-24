/*********************************
 * (c) Christopher Wang / Steamfist Innovations
 * 10/6/2016
 * Please don't steal this code or use without permission
*********************************/

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace StoryBookEditor
{
    public class StoryBookModel
    {
        public List<StoryPageModel> Pages;
        public List<StoryBranchModel> Branches;
        public string BackgroundMusic;

        public StoryBookModel()
        {
            Pages = new List<StoryPageModel>();
            Branches = new List<StoryBranchModel>();
        }

        public override bool Equals(object obj)
        {
            var other = obj as StoryBookModel;

            if (obj == null || other == null)
            {
                return false;
            }
            if (Pages.Count != other.Pages.Count || Branches.Count != other.Branches.Count)
                return false;
            for (int i = 0; i < Pages.Count; i++)
            {
                if (Pages[i] != other.Pages[i])
                    return false;
            }
            for (int i = 0; i < Branches.Count; i++)
            {
                if (Branches[i] != other.Branches[i])
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return (Branches.GetHashCode() >> 10) + Pages.GetHashCode();
        }

        public bool UpdatePage(string pageId, string pageName, Sprite pageImage, AudioClip bgm, params StoryBranchModel[] branches)
        {
            var matchingPage = (from p in Pages
                                where p.Id == pageId
                                select p).FirstOrDefault();
            if (matchingPage == null)
            {
                Debug.LogError("Page updated, unable to find in story book");
                return false;
            }

            matchingPage.Name = pageName;

            if (pageImage == null && !string.IsNullOrEmpty(matchingPage.Background))
            {
                matchingPage.Background = string.Empty;
            }
            else if (pageImage != null && pageImage.name != matchingPage.Background)
            {
                matchingPage.Background = pageImage.name;
            }

            if ((string.IsNullOrEmpty(matchingPage.BackgroundMusic) && bgm != null) ||
                    (!string.IsNullOrEmpty(matchingPage.BackgroundMusic) && bgm == null) ||
                    (!string.IsNullOrEmpty(matchingPage.BackgroundMusic) && bgm != null && matchingPage.BackgroundMusic != bgm.name))
            {
                if (bgm == null)
                {
                    matchingPage.BackgroundMusic = null;
                }
                else
                {
                    matchingPage.BackgroundMusic = bgm.name;
                }
            }

            branches.ToList().ForEach(x =>
            {
                var index = Branches.ToList().IndexOf(x);
                if (index >= 0)
                {
                    Branches[index].CopyFrom(x);
                }
                else
                {
                    Debug.LogError("Unable to find branch in book");
                }
            });

            return true;
        }
 
        public StoryBranchModel AddBranchToPage(Vector2 loc, Vector2 size, Sprite sprite, AudioClip sfx,
            TransitionTypes transition, int transitionLength, Sprite currentImage, Sprite nextImage, string nextPageName, string currentId)
        {
            StoryBranchModel reply = null;
            if (string.IsNullOrEmpty(nextPageName))
            {
                nextPageName = "Next Page " + Pages.Count.ToString();
            }
            //Todo: turn this into a contructor to ensure variables being set
            reply = new StoryBranchModel()
            {
                ImageSprite = sprite,
                ItemLocation = loc,
                ItemSize = size,
                SFXClip = sfx,
                NextPageName = nextPageName,
                TransitionType = transition,
                TransitionLength = transitionLength,
                CurrentImageSprite = currentImage,
                NextImageSprite= nextImage,
            };
            reply.CopyObjsIntoStrings();

            var page = Pages.Where(x => x.Name.ToLower() == nextPageName.ToLower()).FirstOrDefault();
            if (page == null)
            {
                page = new StoryPageModel()
                {
                    Name = nextPageName
                };
                Pages.Add(page);
            }
            var currentPage = (from p in Pages
                               where p.Id == currentId
                               select p).FirstOrDefault();
            if (currentPage != null)
            {
                currentPage.Branches.Add(reply.Id);
            }
            reply.NextPageId = page.Id;
            Branches.Add(reply);

            return reply;
        }
        public string GetPageId(string pageName)
        {
            return (from p in Pages
                    where p.Name == pageName
                    select p.Id).FirstOrDefault();
        }
    }
}
