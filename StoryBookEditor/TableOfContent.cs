using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace StoryBookEditor
{
    public class TableOfContent : EditorWindow
    {
        protected static readonly int TAB_OFFSET = 8;
        public static readonly GUILayoutOption LabelMaxWidth = GUILayout.MaxWidth(250);

        [MenuItem("Story Book Editor/Table of Content")]
        public static void ShowWindow()
        {
            GetWindow(typeof(TableOfContent));
        }
        private void OnGUI()
        {
            GUILayout.Label("Table of Contents", EditorStyles.boldLabel);
            var roots = Startup.BookInstance.StoryBookData.Pages.Where(x => !Startup.BookInstance.StoryBookData.Branches.Where(y => y.NextPageId == x.Id).Any());
            if (roots.Any())
            {
                var root = roots.First();
                GUILayout.Label(string.Format("Root: {0}", root.Name), EditorStyles.boldLabel);
                printChildren(root, TAB_OFFSET, new List<string>() { root.Name });
            }

            if (roots.Count() > 1)
            {
                GUILayout.Label("Unreachable Pages:", EditorStyles.boldLabel);
                int i = 0;
                foreach (var root in roots)
                {
                    if (i > 0)
                    {
                        GUILayout.Label(root.Name);
                    }
                    i++;
                }
            }
        }

        protected void printChildren(StoryPageModel parent, int offset, List<string> prevIds)
        {
            var childPages = parent.Branches.Select(x => GetBranchById(x)).Where(x => x != null).Select(x => GetPageById(x.NextPageId)).Where(x => x != null);
            var style = GUIStyle.none;
            style.contentOffset = new Vector2(offset, 0f);
            foreach (var page in childPages)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(page.Name, style, LabelMaxWidth);
                if(GUILayout.Button("Delete", EditorStyles.miniButtonRight))
                {

                }
                EditorGUILayout.EndHorizontal();
                if (!prevIds.Contains(page.Id))
                {
                    prevIds.Add(page.Id);
                    printChildren(page, offset + TAB_OFFSET, prevIds);
                    prevIds.Remove(page.Id);
                }
                else
                {
                    GUILayout.Label("...", style);
                }
            }
            EditorGUI.indentLevel -= 1;
        }

        public static StoryPageModel GetPageById(string id)
        {
            return Startup.BookInstance.StoryBookData.Pages.Where(x => x.Id == id).FirstOrDefault();
        }

        public static StoryBranchModel GetBranchById(string id)
        {
            return Startup.BookInstance.StoryBookData.Branches.Where(x => x.Id == id).FirstOrDefault();
        }
    }
}
