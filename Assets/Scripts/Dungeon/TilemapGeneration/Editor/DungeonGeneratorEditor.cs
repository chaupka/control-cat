using UnityEngine;
using UnityEditor;
using System.Linq;

namespace DungeonGeneration
{
    [CustomEditor(typeof(AbstractDungeonGenerator), true)]
    public class DungeonGeneratorEditor : Editor
    {
        protected AbstractDungeonGenerator generator;

        protected virtual void Awake()
        {
            generator = (AbstractDungeonGenerator)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Generate dungeon"))
            {
                generator.GenerateDungeon();
            }
            if (GUILayout.Button("Generate dungeon with seed"))
            {
                generator.GenerateDungeon(generator.seed);
            }
        }

        private void OnSceneGUI()
        {
            var rooms = generator.tree.nodes.OfType<RoomNode>();
            foreach (var room in rooms)
            {
                var rect = new Rect((Vector3)room.bounds.min, (Vector3)room.bounds.size);
                Handles.DrawSolidRectangleWithOutline(rect, Color.clear, Color.blue);
                Handles.Label(
                    room.bounds.min,
                    room.name + ", " + room.bounds.min + ", " + room.bounds.size
                );
            }
        }
    }
}
