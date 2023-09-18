using UnityEngine.UIElements;
using UnityEditor;

namespace DungeonGeneration
{
    public class InspectorView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<InspectorView, VisualElement.UxmlTraits> { }

        Editor editor;

        public InspectorView() { }

        internal void UpdateSelection(NodeView nodeView)
        {
            Clear();

            UnityEngine.Object.DestroyImmediate(editor);

            editor = Editor.CreateEditor(nodeView.node);
            IMGUIContainer container =
                new(() =>
                {
                    if (editor && editor.target)
                    {
                        editor.OnInspectorGUI();
                    }
                });
            Add(container);
        }
    }
}