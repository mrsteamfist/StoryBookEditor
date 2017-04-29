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
    [CustomEditor(typeof(StoryBook))]
    [CanEditMultipleObjects]
    public class StoryBookEditor : Editor
    {
        #region UI Elements
        public static GUIContent LocationLabel = new GUIContent("Location");
        public static GUIContent SFXLabel = new GUIContent("Sound Effect");
        public static GUIContent SizeLabel = new GUIContent("Size");
        public static GUIContent SpriteLabel = new GUIContent("Image");
        public static GUIContent AnimationLabel = new GUIContent("Animation");
        public static GUIContent NextPageLabel = new GUIContent("Next Page");
        public static GUIContent NewPageLabel = new GUIContent("New Page");
        public static GUIContent BranchesLabel = new GUIContent("Branches");
        public static GUIContent SettingsLabel = new GUIContent("Settings");
        public static GUIContent TransitionLengthLabel = new GUIContent("Length of Transition");
        public static GUIContent CurrentImageLabel = new GUIContent("Slide Start Image");
        public static GUIContent NextImageLabel = new GUIContent("Slide End Image");
        public static GUIContent PreReqBranchLabel = new GUIContent("Required Variables");
        public static GUIContent PostReqBranchLabel = new GUIContent("Set Variables");
        public static GUIContent ClearBranchLabel = new GUIContent("Cleared Variables");
        public static GUILayoutOption ButtonWidth = GUILayout.Width(66);
        public static GUILayoutOption WideButtonWidth = GUILayout.Width(133);
        #endregion

        #region Book Properties
        protected SerializedProperty PageName;
        protected SerializedProperty PagesBG;
        protected SerializedProperty PagesAni;
        protected SerializedProperty PagesCanBack;
        protected SerializedProperty Branches;
        protected SerializedProperty BackgroundClip;
        #endregion

        #region Variables
        protected Sprite _nextImg;
        protected Sprite _transCurrentImg;
        protected Sprite _transNextImg;
        protected int _transLength = 1000;
        protected TransitionTypes _transType = TransitionTypes.None;
        protected AudioClip _nextSfx;
        protected Vector2 _location = Vector2.zero;
        protected Vector2 _size = new Vector2(1, 1);
        protected string _nextName = string.Empty;
        protected bool _showNewPageContent = false;
        protected bool _showBranches = false;
        protected bool _showSettings = false;
        protected string _reverseBranchVariable = string.Empty;
        protected string _preBranchVariable = string.Empty;
        protected string _postBranchVariable = string.Empty;
        #endregion

        /// <summary>
        /// Invoked on enabled
        /// </summary>
        void OnEnable()
        {
            PagesBG = serializedObject.FindProperty(StoryBook.PAGE_IMAGE_PROPERTY);
            PagesAni = serializedObject.FindProperty(StoryBook.PAGE_ANIMATION_PROPERTY);
            PageName = serializedObject.FindProperty(StoryBook.PAGE_NAME_PROPERTY);
            Branches = serializedObject.FindProperty(StoryBook.BRANCHES_PROPERTY);
            PagesCanBack = serializedObject.FindProperty(StoryBook.PAGE_CAN_BACK_PROPERTY);
            BackgroundClip = serializedObject.FindProperty(StoryBook.BACKGROUND_CLIP_PROPERTY);
        }
        /// <summary>
        /// Casting property for serialized object as Book object
        /// </summary>
        public StoryBook TargetBook
        {
            get
            {
                return serializedObject.targetObject as StoryBook;
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
            EditorGUILayout.PropertyField(PagesAni);
            EditorGUILayout.PropertyField(BackgroundClip);
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
                if (_showBranches = EditorGUILayout.Foldout(_showBranches, BranchesLabel))
                {
                    EditorGUI.indentLevel += 1;
                    
                    for (int i = 0; i < Branches.arraySize; i++)
                    {
                        var property = Branches.GetArrayElementAtIndex(i);
                        var nameProp = property.FindPropertyRelative("NextPageName");
                        EditorGUILayout.LabelField("Branch:" + nameProp.stringValue);
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("ItemLocation"), LocationLabel);
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("ItemSize"), SizeLabel);
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("ImageSprite"), SpriteLabel);
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("CurrentAnimation"), AnimationLabel);
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("NextPageName"), NextPageLabel);
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("SFXClip"), SFXLabel);
                        property.FindPropertyRelative("TransitionType").intValue = (int)(TransitionTypes)EditorGUILayout.EnumPopup("Transition Type", (TransitionTypes)property.FindPropertyRelative("TransitionType").intValue);
                        
                        if((TransitionTypes)property.FindPropertyRelative("TransitionType").intValue == TransitionTypes.Slide)
                        {
                            EditorGUILayout.PropertyField(property.FindPropertyRelative("TransitionLength"), TransitionLengthLabel);
                            EditorGUILayout.PropertyField(property.FindPropertyRelative("CurrentImageSprite"), CurrentImageLabel);
                            EditorGUILayout.PropertyField(property.FindPropertyRelative("NextImageSprite"), NextImageLabel);
                        }
                        else if((TransitionTypes)property.FindPropertyRelative("TransitionType").intValue == TransitionTypes.Fade)
                        {
                            EditorGUILayout.PropertyField(property.FindPropertyRelative("TransitionLength"), TransitionLengthLabel);
                        }
                        else
                        {
                            property.FindPropertyRelative("TransitionLength").intValue = 1000;
                            property.FindPropertyRelative("CurrentImageSprite").objectReferenceValue = null;
                            property.FindPropertyRelative("NextImageSprite").objectReferenceValue = null;
                        }
                        #region Branch Variables
                        if(property.FindPropertyRelative("IsPreVariablesOpen").boolValue = EditorGUILayout.Foldout(property.FindPropertyRelative("IsPreVariablesOpen").boolValue, PreReqBranchLabel))
                        {
                            var preReqs = property.FindPropertyRelative("PreVariables");
                            for (int j = 0; j < preReqs.arraySize; j++)
                            {
                                EditorGUILayout.BeginHorizontal();
                                var oldVal = preReqs.GetArrayElementAtIndex(j).stringValue;
                                preReqs.GetArrayElementAtIndex(j).stringValue = EditorGUILayout.TextField(preReqs.GetArrayElementAtIndex(j).stringValue);
                                if (oldVal != preReqs.GetArrayElementAtIndex(j).stringValue)
                                    TargetBook.BookUpdated();
                                if (GUILayout.Button("Delete Variable", EditorStyles.miniButtonRight, WideButtonWidth))
                                {
                                    TargetBook.Branches[i].PreVariables.RemoveAt(j);
                                    _preBranchVariable = string.Empty;
                                    TargetBook.BookUpdated();
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                            EditorGUILayout.BeginHorizontal();
                            _preBranchVariable = EditorGUILayout.TextField(_preBranchVariable);
                            if (GUILayout.Button("Add Variable", EditorStyles.miniButtonRight, WideButtonWidth))
                            {
                                TargetBook.Branches[i].PreVariables.Add(_preBranchVariable);
                                _preBranchVariable = string.Empty;
                                TargetBook.BookUpdated();
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        if (property.FindPropertyRelative("IsPostVariablesOpen").boolValue = EditorGUILayout.Foldout(property.FindPropertyRelative("IsPostVariablesOpen").boolValue, PostReqBranchLabel))
                        {
                            var preReqs = property.FindPropertyRelative("PostVariables");
                            for (int j = 0; j < preReqs.arraySize; j++)
                            {
                                EditorGUILayout.BeginHorizontal();
                                var oldVal = preReqs.GetArrayElementAtIndex(j).stringValue;
                                preReqs.GetArrayElementAtIndex(j).stringValue = EditorGUILayout.TextField(preReqs.GetArrayElementAtIndex(j).stringValue);
                                if (oldVal != preReqs.GetArrayElementAtIndex(j).stringValue)
                                    TargetBook.BookUpdated();
                                if (GUILayout.Button("Delete Variable", EditorStyles.miniButtonRight, WideButtonWidth))
                                {
                                    TargetBook.Branches[i].PostVariables.RemoveAt(j);
                                    _preBranchVariable = string.Empty;
                                    TargetBook.BookUpdated();
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                            EditorGUILayout.BeginHorizontal();
                            _postBranchVariable = EditorGUILayout.TextField(_postBranchVariable);
                            if (GUILayout.Button("Add Variable", EditorStyles.miniButtonRight, WideButtonWidth))
                            {
                                TargetBook.Branches[i].PostVariables.Add(_postBranchVariable);
                                _postBranchVariable = string.Empty;
                                TargetBook.BookUpdated();
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        if (property.FindPropertyRelative("IsReverseVariablesOpen").boolValue = EditorGUILayout.Foldout(property.FindPropertyRelative("IsReverseVariablesOpen").boolValue, ClearBranchLabel))
                        {
                            var preReqs = property.FindPropertyRelative("ReverseVariables");
                            for(int j = 0; j < preReqs.arraySize; j++ )
                            {
                                EditorGUILayout.BeginHorizontal();
                                var oldVal = preReqs.GetArrayElementAtIndex(j).stringValue;
                                preReqs.GetArrayElementAtIndex(j).stringValue = EditorGUILayout.TextField(preReqs.GetArrayElementAtIndex(j).stringValue);
                                if(oldVal != preReqs.GetArrayElementAtIndex(j).stringValue)
                                    TargetBook.BookUpdated();
                                if (GUILayout.Button("Delete Variable", EditorStyles.miniButtonRight, WideButtonWidth))
                                {
                                    TargetBook.Branches[i].ReverseVariables.RemoveAt(j);
                                    _preBranchVariable = string.Empty;
                                    TargetBook.BookUpdated();
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                            EditorGUILayout.BeginHorizontal();
                            _reverseBranchVariable = EditorGUILayout.TextField(_reverseBranchVariable);
                            if (GUILayout.Button("Add Variable", EditorStyles.miniButtonRight, WideButtonWidth))
                            {
                                TargetBook.Branches[i].ReverseVariables.Add(_reverseBranchVariable);
                                _reverseBranchVariable = string.Empty;
                                TargetBook.BookUpdated();
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        #endregion
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Go To", EditorStyles.miniButtonLeft, ButtonWidth))
                        {
                            TargetBook.LoadPage(TargetBook.Branches[i].NextPageId, null);
                        }
                        if (GUILayout.Button("Delete", EditorStyles.miniButtonRight, ButtonWidth))
                        {
                            TargetBook.DeleteBranch(TargetBook.Branches[i].Id);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            #endregion

            #region New Page Area
            EditorGUILayout.Separator();
            _showNewPageContent = EditorGUILayout.Foldout(_showNewPageContent, NewPageLabel);
            if (_showNewPageContent)
            {
                _nextName = EditorGUILayout.TextField("New Page Name", _nextName);
                _location = EditorGUILayout.Vector2Field("Item Location", _location);
                _size = EditorGUILayout.Vector2Field("Item Size", _size);
                _nextSfx = EditorGUILayout.ObjectField(new GUIContent("SFX"), _nextSfx, typeof(AudioClip), true) as AudioClip;
                _nextImg = EditorGUILayout.ObjectField(new GUIContent("Item Image"), _nextImg, typeof(Sprite), true) as Sprite;

                if (GUILayout.Button("Create"))
                {
                    TargetBook.AddBranchToPage(_location, _size, _nextImg, _nextSfx, _nextName);
                    _location = Vector2.zero;
                    _size = new Vector2(1, 1);
                    _nextImg = null;
                    _nextSfx = null;
                    _nextName = string.Empty;
                }
            }
            EditorGUILayout.Separator();
            #endregion
            var applied = serializedObject.ApplyModifiedProperties();
            if (applied ||
                 (Event.current.type == EventType.ExecuteCommand ||
                 Event.current.commandName == "UndoRedoPerformed"))
            {
                TargetBook.BookUpdated();
            }
        }
    }
}
