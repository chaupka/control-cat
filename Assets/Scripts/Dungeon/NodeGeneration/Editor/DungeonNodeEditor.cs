using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DungeonGeneration
{
    public class DungeonNodeEditor : EditorWindow
    {
        DungeonTreeView treeView;
        DungeonTree tree;
        InspectorView inspectorView;
        ToolbarMenu toolbarMenu;
        TextField treeNameField;
        TextField locationPathField;
        Button createNewTreeButton;
        VisualElement overlay;
        DungeonTreeSettings settings;

        [MenuItem("Window/Carsten/Dungeon Node Editor ...")]
        public static void OpenWindow()
        {
            var wnd = GetWindow<DungeonNodeEditor>();
            wnd.titleContent = new GUIContent("Dungeon Node Editor");
            wnd.minSize = new Vector2(800, 600);
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            if (Selection.activeObject is DungeonTree)
            {
                OpenWindow();
                return true;
            }
            return false;
        }

        List<T> LoadAssets<T>()
            where T : Object
        {
            string[] assetIds = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            var assets = new List<T>();
            foreach (var assetId in assetIds)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetId);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                assets.Add(asset);
            }
            return assets;
        }

        public void CreateGUI()
        {
            settings = DungeonTreeSettings.GetOrCreateSettings();

            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree = settings.dungeonTreeXml;
            visualTree.CloneTree(root);

            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheet = settings.dungeonTreeStyle;
            root.styleSheets.Add(styleSheet);

            // Main treeview
            treeView = root.Q<DungeonTreeView>();
            treeView.OnNodeSelected = OnNodeSelectionChanged;

            // Inspector View
            inspectorView = root.Q<InspectorView>();

            // Toolbar assets menu
            toolbarMenu = root.Q<ToolbarMenu>();
            var dungeonTrees = LoadAssets<DungeonTree>();
            dungeonTrees.ForEach(tree =>
            {
                toolbarMenu.menu.AppendAction(
                    $"{tree.name}",
                    (a) =>
                    {
                        Selection.activeObject = tree;
                    }
                );
            });
            toolbarMenu.menu.AppendSeparator();
            toolbarMenu.menu.AppendAction("New Dungeon...", (a) => CreateNewDungeon("NewDungeon"));

            // New Tree Dialog
            treeNameField = root.Q<TextField>("TreeName");
            locationPathField = root.Q<TextField>("LocationPath");
            overlay = root.Q<VisualElement>("Overlay");
            createNewTreeButton = root.Q<Button>("CreateButton");
            createNewTreeButton.clicked += () => CreateNewDungeon(treeNameField.value);

            if (tree == null)
            {
                OnSelectionChange();
            }
            else
            {
                SelectTree(tree);
            }
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
            }
        }

        private void OnSelectionChange()
        {
            EditorApplication.delayCall += () =>
            {
                DungeonTree tree = Selection.activeObject as DungeonTree;
                if (!tree)
                {
                    if (Selection.activeGameObject)
                    {
                        AbstractDungeonGenerator generator =
                            Selection.activeGameObject.GetComponent<AbstractDungeonGenerator>();
                        if (generator)
                        {
                            tree = generator.parameters.dungeon.tree;
                        }
                    }
                }

                SelectTree(tree);
            };
        }

        void SelectTree(DungeonTree newTree)
        {
            if (treeView == null)
            {
                return;
            }

            if (!newTree)
            {
                return;
            }

            this.tree = newTree;

            overlay.style.visibility = Visibility.Hidden;

            if (Application.isPlaying)
            {
                treeView.PopulateView(tree);
            }
            else
            {
                treeView.PopulateView(tree);
            }

            // blackboardProperty = treeObject.FindProperty("blackboard");

            EditorApplication.delayCall += () =>
            {
                treeView.FrameAll();
            };
        }

        void OnNodeSelectionChanged(NodeView node)
        {
            inspectorView.UpdateSelection(node);
        }

        void CreateNewDungeon(string assetName)
        {
            string path = System.IO.Path.Combine(locationPathField.value, $"{assetName}.asset");
            DungeonTree tree = CreateInstance<DungeonTree>();
            tree.name = treeNameField.ToString();
            AssetDatabase.CreateAsset(tree, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = tree;
            EditorGUIUtility.PingObject(tree);
        }
    }
}
