namespace KWEngine3.Exceptions
{
    /// <summary>
    /// Ausnahmefehlerklasse für GameObject-Instanzen
    /// </summary>
    public class GameObjectException : Exception
    {
        /// <summary>
        /// Standardkonstruktor
        /// </summary>
        /// <param name="message">Fehlernachricht</param>
        public GameObjectException(string message)
            : base(message)
        {

        }
    }
}
