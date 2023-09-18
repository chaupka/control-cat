using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System;
using System.Linq;

namespace DungeonGeneration
{
    public class DungeonTreeView : GraphView
    {
        public Action<NodeView> OnNodeSelected;

        public new class UxmlFactory : UxmlFactory<DungeonTreeView, UxmlTraits> { }

        DungeonTree tree;
        DungeonTreeSettings settings;

        public struct ScriptTemplate
        {
            public TextAsset templateFile;
            public string defaultFileName;
            public string subFolder;
        }

        public ScriptTemplate[] scriptFileAssets =
        {
            new()
            {
                templateFile = DungeonTreeSettings.GetOrCreateSettings().scriptTemplateRoomNode,
                defaultFileName = "NewRoomNode.cs",
                subFolder = "Rooms"
            },
            new()
            {
                templateFile = DungeonTreeSettings
                    .GetOrCreateSettings()
                    .scriptTemplateConnectionNode,
                defaultFileName = "NewConnectionNode.cs",
                subFolder = "Connections"
            }
        };

        public DungeonTreeView()
        {
            settings = DungeonTreeSettings.GetOrCreateSettings();

            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var styleSheet = settings.dungeonTreeStyle;
            styleSheets.Add(styleSheet);

            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnUndoRedo()
        {
            PopulateView(tree);
            AssetDatabase.SaveAssets();
        }

        public NodeView FindNodeView(Node node)
        {
            return GetNodeByGuid(node.guid) as NodeView;
        }

        internal void PopulateView(DungeonTree tree)
        {
            this.tree = tree;

            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged;

            if (tree.startRoom == null)
            {
                tree.startRoom = tree.CreateNode(typeof(StartRoom)) as StartRoom;
                EditorUtility.SetDirty(tree);
                AssetDatabase.SaveAssets();
            }

            // Creates node view
            tree.nodes.ForEach(n => CreateNodeView(n));

            // Create edges
            tree.nodes.ForEach(n =>
            {
                var children = DungeonTree.GetChildren(n);
                children.ForEach(c =>
                {
                    NodeView parentView = FindNodeView(n);
                    NodeView childView = FindNodeView(c);

                    Edge edge = parentView.output.ConnectTo(childView.input);
                    AddElement(edge);
                });
            });
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports
                .ToList()
                .Where(
                    endPort =>
                        endPort.direction != startPort.direction && endPort.node != startPort.node
                )
                .ToList();
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null)
            {
                graphViewChange.elementsToRemove.ForEach(elem =>
                {
                    if (elem is NodeView nodeView)
                    {
                        tree.DeleteNode(nodeView.node);
                    }

                    if (elem is Edge edge)
                    {
                        NodeView parentView = edge.output.node as NodeView;
                        NodeView childView = edge.input.node as NodeView;
                        tree.RemoveChild(parentView.node, childView.node);
                    }
                });
            }

            graphViewChange.edgesToCreate?.ForEach(edge =>
            {
                NodeView parentView = edge.output.node as NodeView;
                NodeView childView = edge.input.node as NodeView;
                tree.AddChild(parentView.node, childView.node);
            });

            nodes.ForEach(
                (n) =>
                {
                    NodeView view = n as NodeView;
                    view.SortChildren();
                }
            );

            return graphViewChange;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            //base.BuildContextualMenu(evt);

            // New script functions
            evt.menu.AppendAction(
                $"Create Script.../New Room Node",
                (a) => CreateNewScript(scriptFileAssets[0])
            );
            evt.menu.AppendAction(
                $"Create Script.../New Connection Node",
                (a) => CreateNewScript(scriptFileAssets[1])
            );
            evt.menu.AppendSeparator();

            Vector2 nodePosition = this.ChangeCoordinatesTo(
                contentViewContainer,
                evt.localMousePosition
            );
            {
                var roomTypes = TypeCache
                    .GetTypesDerivedFrom<RoomNode>()
                    .Where(t => !t.Equals(typeof(StartRoom)));
                foreach (var type in roomTypes)
                {
                    evt.menu.AppendAction(
                        $"[Room]/{type.Name}",
                        (a) => CreateNode(type, nodePosition)
                    );
                }
                var connectionTypes = TypeCache.GetTypesDerivedFrom<ConnectionNode>();
                foreach (var type in connectionTypes)
                {
                    evt.menu.AppendAction(
                        $"[Connection]/{type.Name}",
                        (a) => CreateNode(type, nodePosition)
                    );
                }
            }
        }

        void SelectFolder(string path)
        {
            // https://forum.unity.com/threads/selecting-a-folder-in-the-project-via-button-in-editor-window.355357/
            // Check the path has no '/' at the end, if it does remove it,
            // Obviously in this example it doesn't but it might
            // if your getting the path some other way.

            if (path[^1] == '/')
                path = path[..^1];

            // Load object
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(
                path,
                typeof(UnityEngine.Object)
            );

            // Select the object in the project folder
            Selection.activeObject = obj;

            // Also flash the folder yellow to highlight it
            EditorGUIUtility.PingObject(obj);
        }

        void CreateNewScript(ScriptTemplate template)
        {
            SelectFolder($"{settings.newNodeBasePath}/{template.subFolder}");
            var templatePath = AssetDatabase.GetAssetPath(template.templateFile);
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(
                templatePath,
                template.defaultFileName
            );
        }

        void CreateNode(Type type, Vector2 position)
        {
            Node node = tree.CreateNode(type);
            node.position = position;
            CreateNodeView(node);
        }

        void CreateNodeView(Node node)
        {
            var nodeView = new NodeView(node) { OnNodeSelected = OnNodeSelected };
            AddElement(nodeView);
        }
    }
}
