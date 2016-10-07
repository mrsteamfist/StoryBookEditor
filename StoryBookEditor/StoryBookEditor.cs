/*********************************
 * (c) Christopher Wang / Steamfist Innovations
 * 10/6/2016
 * Please don't steal this code or use without permission
*********************************/

using UnityEngine;
using UnityEditor;

namespace StoryBookEditor
{
    /// <summary>
    /// Editor Object for a story book UI
    /// </summary>
    [CustomEditor(typeof(BookObject))]
    [CanEditMultipleObjects]
    public class StoryBookEditor : Editor
    {
        public static GUIContent LocationLabel = new GUIContent("Location");
        public static GUIContent SizeLabel = new GUIContent("Size");
        public static GUIContent SpriteLabel = new GUIContent("Image");
        public static GUIContent NextPageLabel = new GUIContent("Next Page");
        public static GUILayoutOption ButtonWidth = GUILayout.Width(66);

        protected SerializedProperty PageName;
        protected SerializedProperty PagesBG;
        protected SerializedProperty PagesCanBack;
        protected SerializedProperty Branches;

        protected Sprite _nextImg;
        protected Vector2 _location = Vector2.zero;
        protected Vector2 _size = new Vector2(1, 1);
        protected string _nextName = string.Empty;

        /// <summary>
        /// Invoked on enabled
        /// </summary>
        void OnEnable()
        {
            PagesBG = serializedObject.FindProperty(BookObject.PAGE_IMAGE_PROPERTY);
            PageName = serializedObject.FindProperty(BookObject.PAGE_NAME_PROPERTY);
            Branches = serializedObject.FindProperty(BookObject.BRANCHES_PROPERTY);
            PagesCanBack = serializedObject.FindProperty(BookObject.PAGE_CAN_BACK_PROPERTY);
        }
        /// <summary>
        /// Casting property for serialized object as Book object
        /// </summary>
        public BookObject TargetBook
        {
            get
            {
                return serializedObject.targetObject as BookObject;
            }
        }
        /// <summary>
        /// Generates UI for creating book
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            #region Back Button
            EditorGUILayout.PropertyField(PageName);
            EditorGUILayout.PropertyField(PagesBG);
            if (PagesCanBack.boolValue)
            {
                if (GUILayout.Button("Back"))
                {
                    TargetBook.LoadBack();
                }
            }
            #endregion

            #region Current Adjacent Pages
            if (Branches.arraySize > 0)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.PropertyField(Branches);
                EditorGUI.indentLevel += 1;

                for (int i = 0; i < Branches.arraySize; i++)
                {
                    var property = Branches.GetArrayElementAtIndex(i);
                    var nameProp = property.FindPropertyRelative("NextPageName");
                    EditorGUILayout.LabelField("Branch:" + nameProp.stringValue);
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("ItemLocation"), LocationLabel);
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("ItemSize"), SizeLabel);
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("ImageSprite"), SpriteLabel);
                    EditorGUILayout.PropertyField(nameProp, NextPageLabel);
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Go To", EditorStyles.miniButtonLeft, ButtonWidth))
                    {
                        TargetBook.LoadPage(TargetBook.Branches[i].NextPageId);
                    }
                    if (GUILayout.Button("Delete", EditorStyles.miniButtonRight, ButtonWidth))
                    {
                        TargetBook.DeleteBranch(TargetBook.Branches[i].Id);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            #endregion

            #region New Page Area
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("New Page");

            _nextName = EditorGUILayout.TextField("New Page Name", _nextName);
            _location = EditorGUILayout.Vector2Field("Item Location", _location);
            _size = EditorGUILayout.Vector2Field("Item Size", _size);
            _nextImg = EditorGUILayout.ObjectField(new GUIContent("Item Image"), _nextImg, typeof(Sprite), true) as Sprite;

            if (GUILayout.Button("Create"))
            {
                TargetBook.AddBranchToPage(_location, _size, _nextImg, _nextName);
                _location = Vector2.zero;
                _size = new Vector2(1, 1);
                _nextImg = null;
                _nextName = string.Empty;
            }
            EditorGUILayout.Separator();
            #endregion

            if (serializedObject.ApplyModifiedProperties() ||
                 (Event.current.type == EventType.ExecuteCommand &&
                 Event.current.commandName == "UndoRedoPerformed"))
            {
                TargetBook.BookUpdated();
            }
        }
    }
}
