namespace Utility
{
    public static class LayerTag
    {
        public static readonly int terrainLayer = 1 << 3;
        public static readonly int playerLayer = 1 << 6;
        public static readonly int enemyLayer = 1 << 7;
        public static readonly int cheeseLayer = 1 << 8;
        public static readonly string playerTag = "Player";
        public static readonly string ventTag = "Vent";
    }
}
