using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DungeonGeneration
{
    [CreateAssetMenu(menuName = "Carsten/DungeonTree")]
    public class DungeonTree : ScriptableObject
    {
        public Node startRoom;
        public List<Node> nodes = new();

        public static List<Node> GetChildren(Node parent)
        {
            var children = new List<Node>();

            if (parent is RoomNode roomNode)
            {
                return roomNode.children.Cast<Node>().ToList();
            }
            if (parent is ConnectionNode connectionNode)
            {
                children.Add(connectionNode.child);
            }

            return children;
        }

        public static void Traverse(
            Node node,
            System.Action<Node> visiter,
            List<Node> traversedNodes = default
        )
        {
            traversedNodes ??= new List<Node>();
            if (node && !traversedNodes.Contains(node))
            {
                traversedNodes.Add(node);
                visiter.Invoke(node);
                var nodes = GetChildren(node);
                nodes.ForEach((n) => Traverse(n, visiter, traversedNodes));
            }
        }

        public DungeonTree Clone()
        {
            DungeonTree tree = Instantiate(this);
            tree.startRoom = startRoom.Clone();
            tree.nodes = new List<Node>();
            Traverse(
                tree.startRoom,
                (n) =>
                {
                    tree.nodes.Add(n);
                }
            );

            return tree;
        }

        #region Editor Compatibility
#if UNITY_EDITOR

        public Node CreateNode(System.Type type)
        {
            Node node = CreateInstance(type) as Node;
            node.name = type.Name;
            node.guid = GUID.Generate().ToString();

            Undo.RecordObject(this, "Dungeon Tree (CreateNode)");
            nodes.Add(node);

            if (!Application.isPlaying)
            {
                AssetDatabase.AddObjectToAsset(node, this);
            }

            Undo.RegisterCreatedObjectUndo(node, "Dungeon Tree (CreateNode)");

            AssetDatabase.SaveAssets();
            return node;
        }

        public void DeleteNode(Node node)
        {
            Undo.RecordObject(this, "Dungeon Tree (DeleteNode)");
            nodes.Remove(node);

            //AssetDatabase.RemoveObjectFromAsset(node);
            Undo.DestroyObjectImmediate(node);

            AssetDatabase.SaveAssets();
        }

        public void AddChild(Node parent, Node child)
        {
            if (parent is RoomNode roomNode)
            {
                Undo.RecordObject(roomNode, "Dungeon Tree (AddChild)");
                roomNode.children.Add((ConnectionNode)child);
                EditorUtility.SetDirty(roomNode);
            }
            if (parent is ConnectionNode connectionNode)
            {
                Undo.RecordObject(connectionNode, "Dungeon Tree (AddChild)");
                connectionNode.child = (RoomNode)child;
                EditorUtility.SetDirty(connectionNode);
            }
        }

        public void RemoveChild(Node parent, Node child)
        {
            if (parent is RoomNode roomNode)
            {
                Undo.RecordObject(roomNode, "Dungeon Tree (RemoveChild)");
                roomNode.children.Remove((ConnectionNode)child);
                EditorUtility.SetDirty(roomNode);
            }
            if (parent is ConnectionNode connectionNode)
            {
                Undo.RecordObject(connectionNode, "Dungeon Tree (RemoveChild)");
                connectionNode.child = null;
                EditorUtility.SetDirty(connectionNode);
            }
        }
#endif
        #endregion Editor Compatibility
    }
}
