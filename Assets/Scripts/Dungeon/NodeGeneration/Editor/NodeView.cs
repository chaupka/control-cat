using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor;

namespace DungeonGeneration
{
    public class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        public Action<NodeView> OnNodeSelected;
        public Node node;
        public Port input;
        public Port output;

        public NodeView(Node node)
            : base(AssetDatabase.GetAssetPath(DungeonTreeSettings.GetOrCreateSettings().nodeXml))
        {
            this.node = node;
            this.node.name = node.GetType().Name;
            this.title = node.name.Replace("(Clone)", "").Replace("Node", "");
            this.viewDataKey = node.guid;

            style.left = node.position.x;
            style.top = node.position.y;

            CreateInputPorts();
            CreateOutputPorts();
            SetupClasses();
            SetupDataBinding();
        }

        private void SetupDataBinding()
        {
            Label descriptionLabel = this.Q<Label>("description");
            descriptionLabel.bindingPath = "description";
            descriptionLabel.Bind(new SerializedObject(node));
        }

        private void SetupClasses()
        {
            if (node is StartRoom)
            {
                AddToClassList("start");
            }
            else if (node is TrapRoom)
            {
                AddToClassList("trap");
            }
            else if (node is KeyRoom || node is LockRoom)
            {
                AddToClassList("keyLock");
            }
            else if (node is EndRoom)
            {
                AddToClassList("end");
            }
            else if (node is RoomNode)
            {
                AddToClassList("room");
            }
            else if (node is CorridorNode)
            {
                AddToClassList("corridor");
            }
            else if (node is VentNode)
            {
                AddToClassList("vent");
            }
        }

        private void CreateInputPorts()
        {
            if (node is RoomNode)
            {
                input = new NodePort(Direction.Input, Port.Capacity.Multi);
            }
            else if (node is ConnectionNode)
            {
                input = new NodePort(Direction.Input, Port.Capacity.Single);
            }

            if (input != null)
            {
                input.portName = "";
                input.style.flexDirection = FlexDirection.Column;
                inputContainer.Add(input);
            }
        }

        private void CreateOutputPorts()
        {
            if (node is StartRoom)
            {
                output = new NodePort(Direction.Output, Port.Capacity.Single);
            }
            else if (node is RoomNode)
            {
                output = new NodePort(Direction.Output, Port.Capacity.Multi);
            }
            else if (node is ConnectionNode)
            {
                output = new NodePort(Direction.Output, Port.Capacity.Single);
            }

            if (output != null)
            {
                output.portName = "";
                output.style.flexDirection = FlexDirection.ColumnReverse;
                outputContainer.Add(output);
            }
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            Undo.RecordObject(node, "Dungeon Tree (Set Position");
            node.position.x = newPos.xMin;
            node.position.y = newPos.yMin;
            EditorUtility.SetDirty(node);
        }

        public override void OnSelected()
        {
            base.OnSelected();
            OnNodeSelected?.Invoke(this);
        }

        public void SortChildren()
        {
            if (node is RoomNode roomNode)
            {
                roomNode.children.Sort(SortByHorizontalPosition);
            }
        }

        private int SortByHorizontalPosition(Node left, Node right)
        {
            return left.position.x < right.position.x ? -1 : 1;
        }
    }
}
