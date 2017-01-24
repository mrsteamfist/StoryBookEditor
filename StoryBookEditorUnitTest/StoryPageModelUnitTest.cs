using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoryBookEditor;
using System.Collections.Generic;

namespace StoryBookEditorUnitTest
{
    [TestClass]
    public class StoryPageModelUnitTest
    {
        /// <summary>
        /// This test creates a stoyrbook object and verifies it values are initialized
        /// </summary>
        [TestMethod]
        public void CreateInstanceTest()
        {
            StoryBookModel model = new StoryBookModel();
            Assert.AreEqual(model.BackgroundMusic, null);
            CollectionAssert.AreEqual(model.Branches, new List<StoryBranchModel>());
            CollectionAssert.AreEqual(model.Pages, new List<StoryPageModel>());
        }
        [TestMethod]
        public void EqualsTest()
        {
            StoryBookModel model1 = null;
            Console.WriteLine("Verify null comparision");
            Assert.AreEqual(model1, null);

            model1 = new StoryBookModel();
            StoryBookModel model2 = new StoryBookModel();
            Console.WriteLine("Verify two initial objects test");
            Assert.AreEqual(model1, model2);

            var branch = new StoryBranchModel();
            model1.Branches.Add(branch);
            model2.Branches.Add(branch);
            Console.WriteLine("Verify with a single branch");
            Assert.AreEqual(model1, model2);

            //ToDo: verify two different matching branches
            //ToDo: verify multiple instances of same branch with different ammounts fail
            //ToDo: verify two not equal branches fail
        }
        [TestMethod]
        public void UpdatePageTest()
        {
            StoryBookModel model = new StoryBookModel();
            //verify UpdatePage sets correct values
        }
        [TestMethod]
        public void AddBranchToPageTest()
        {
            var pageName = "next name";
            //var sfxName = "sfx name";
            //var spriteName = "sprite name";
            StoryBookModel model = new StoryBookModel();
            var page = new StoryPageModel();
            model.Pages.Add(page);
            page = new StoryPageModel() { Name = pageName };
            model.Pages.Add(page);
            var branch = new StoryBranchModel();
            branch.ItemLocation = new UnityEngine.Vector2(1, 2);
            branch.ItemSize = new UnityEngine.Vector2(2, 3);
            branch.ImageSprite = new UnityEngine.Sprite();// { name = spriteName };
            branch.SFXClip = new UnityEngine.AudioClip();// { name = sfxName };

            //verify AddBranchToPage adds a valid branch to the book
            //todo: make unit test with unity
            //var addedBranch = model.AddBranchToPage(branch.ItemLocation, branch.ItemSize, branch.ImageSprite, branch.SFXClip, branch.NextPageName, page.Id);
        }
        [TestMethod]
        public void GetPageByIdTest()
        {
            string selectedPage = string.Empty;
            string selectedName = null;
            StoryBookModel model = new StoryBookModel();

            //verify getpageid returns null on empty page
            Assert.AreEqual(model.GetPageId(null), null);
            //add a few pages
            for(int i = 0; i < 100; i++)
            {
                var page = new StoryPageModel()
                {
                    Name = "Page " + i,
                };
                if(i == 15)
                {
                    selectedPage = page.Id;
                    selectedName = page.Name;
                }
                model.Pages.Add(page);
            }
            //verify GetPageId returns correct page number
            Assert.AreEqual(model.GetPageId(selectedName), selectedPage);
            //verify that getpageid return null on bad input
            Assert.AreEqual(model.GetPageId(null), null);
            //verify that getpageid return null on page that doesn't exist
            Assert.AreEqual(model.GetPageId("FAKE"), null);
        }
    }
}
