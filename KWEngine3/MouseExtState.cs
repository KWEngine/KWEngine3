namespace KWEngine3
{
    internal class MouseExtState
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
