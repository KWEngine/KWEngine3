namespace KWEngine3.Helper
{
    /// <summary>
    /// Basisklasse für Explosions und Partikeleffekte
    /// </summary>
    public abstract class TimeBasedObject
    {
        internal bool _done = false;

        internal abstract void Act();
    }
}
