using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(SpriteSheetRuleTile))]
public class SpriteSheetRuleTileEditor : RuleTileEditor
{
    SpriteSheetRuleTile ruleTile;

    private void Awake()
    {
        ruleTile = (SpriteSheetRuleTile)target;
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Exchange Sprites"))
        {
            ExchangeSprites();
        }
        base.OnInspectorGUI();
    }

    private void ExchangeSprites()
    {
        if (ruleTile.m_DefaultSprite != null)
        {
            string path = AssetDatabase.GetAssetPath(ruleTile.m_DefaultSprite);
            var replaceSpriteList = LoadSpritesAtPath(path);
            foreach (var rule in ruleTile.m_TilingRules)
            {
                for (int i = 0; i < rule.m_Sprites.Length; i++)
                {
                    var sprite = rule.m_Sprites[i];
                    var spriteNumber = sprite.name[(sprite.name.LastIndexOf("_") + 1)..];
                    rule.m_Sprites[i] = replaceSpriteList.Find(s => s.name.EndsWith("_" + spriteNumber));
                }
            }
        }
    }

    private List<Sprite> LoadSpritesAtPath(string path)
    {
        return new List<Object>(AssetDatabase.LoadAllAssetsAtPath(path))
                    .FindAll(FilterSprites()).Cast<Sprite>().ToList();
    }

    private System.Predicate<Object> FilterSprites()
    {
        return loaded =>
        {
            try
            {
                return (Sprite)loaded;
            }
            catch
            {
                return false;
            }
        };
    }
}