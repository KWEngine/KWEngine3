namespace KWEngine3.Exceptions
{
    /// <summary>
    /// Ausnahmefehlerklasse der Engine (noch nicht überall verwendet)
    /// </summary>
    public class EngineException : Exception
    {
        /// <summary>
        /// Standardkonstruktor
        /// </summary>
        /// <param name="msg">Fehlermeldung</param>
        public EngineException(string msg) 
            : base(msg)
        {

        }
    }
}
