using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Carsten/SpriteSheetRuleTile")]
public class SpriteSheetRuleTile : RuleTile<SpriteSheetRuleTile.Neighbor>
{
    public List<TileBase> friendTiles = new();

    public class Neighbor : TilingRuleOutput.Neighbor
    {
        public const int ThisOrFriend = 3;
    }

    public override bool RuleMatch(int neighbor, TileBase tile)
    {
        return neighbor switch
        {
            Neighbor.ThisOrFriend => tile == this || friendTiles.Contains(tile),
            _ => base.RuleMatch(neighbor, tile),
        };
    }
}
