using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace DungeonGeneration
{
    // Create a new type of Settings Asset.
    class DungeonTreeSettings : ScriptableObject
    {
        public VisualTreeAsset dungeonTreeXml;
        public StyleSheet dungeonTreeStyle;
        public VisualTreeAsset nodeXml;
        public TextAsset scriptTemplateRoomNode;
        public TextAsset scriptTemplateConnectionNode;
        public string newNodeBasePath = "Assets/";

        static DungeonTreeSettings FindSettings()
        {
            var guids = AssetDatabase.FindAssets("t:DungeonTreeSettings");
            if (guids.Length > 1)
            {
                Debug.LogWarning($"Found multiple settings files, using the first.");
            }

            switch (guids.Length)
            {
                case 0:
                    return null;
                default:
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    return AssetDatabase.LoadAssetAtPath<DungeonTreeSettings>(path);
            }
        }

        internal static DungeonTreeSettings GetOrCreateSettings()
        {
            var settings = FindSettings();
            if (settings == null)
            {
                settings = CreateInstance<DungeonTreeSettings>();
                AssetDatabase.CreateAsset(
                    settings,
                    "Assets/Scripts/DungeonGeneration/DungeonTreeSettings.asset"
                );
                AssetDatabase.SaveAssets();
            }
            return settings;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
    }

    // Register a SettingsProvider using UIElements for the drawing framework:
    static class MyCustomSettingsUIElementsRegister
    {
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Settings window for the Project scope.
            var provider = new SettingsProvider(
                "Project/MyCustomUIElementsDungeonTreeSettings",
                SettingsScope.Project
            )
            {
                label = "DungeonTree",
                // activateHandler is called when the user clicks on the Settings item in the Settings window.
                activateHandler = (searchContext, rootElement) =>
                {
                    var settings = DungeonTreeSettings.GetSerializedSettings();

                    // rootElement is a VisualElement. If you add any children to it, the OnGUI function
                    // isn't called because the SettingsProvider uses the UIElements drawing framework.
                    var title = new Label() { text = "Dungeon Tree Settings" };
                    title.AddToClassList("title");
                    rootElement.Add(title);

                    var properties = new VisualElement()
                    {
                        style = { flexDirection = FlexDirection.Column }
                    };
                    properties.AddToClassList("property-list");
                    rootElement.Add(properties);

                    properties.Add(new InspectorElement(settings));

                    rootElement.Bind(settings);
                },
            };

            return provider;
        }
    }
}
