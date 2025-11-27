namespace KWEngine3
{
    internal class KeyboardExtState
    {
        public float Time { get; set; }
        public ulong Frame { get; set; }
        public bool OldWorld { get; set; }

        public void SwitchToOldWorld()
        {
            OldWorld = true;
        }
    }
}
