using UnityEngine;
using UnityEditor;

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
    }
}
