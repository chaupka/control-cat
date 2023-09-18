using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Carsten/SpriteSheetRuleTile")]
public class SpriteSheetRuleTile : RuleTile<SpriteSheetRuleTile.Neighbor>
{
    public TileBase[] friendTiles;

    public class Neighbor : TilingRuleOutput.Neighbor
    {
        public const int ThisOrFriend = 3;
    }

    public override bool RuleMatch(int neighbor, TileBase tile)
    {
        switch (neighbor)
        {
            case Neighbor.This: return tile == this;
            case Neighbor.ThisOrFriend: return tile == this || HasFriendTile(tile);
            case Neighbor.NotThis: return tile != this;
        }
        return base.RuleMatch(neighbor, tile);
    }

    bool HasFriendTile(TileBase tile)
    {
        if (tile == null)
            return false;

        for (int i = 0; i < friendTiles.Length; i++)
        {
            if (friendTiles[i] == tile)
                return true;
        }
        return false;
    }
}