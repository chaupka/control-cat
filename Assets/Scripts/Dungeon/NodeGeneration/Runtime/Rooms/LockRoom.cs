namespace DungeonGeneration
{
    public enum KeyLock
    {
        Blue,
        Red
    }

    public class LockRoom : RoomNode
    {
        public KeyLock keyLock;
    }
}
