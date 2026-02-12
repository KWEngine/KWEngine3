namespace KWEngine3
{
    /// <summary>
    /// Attribut zur Anzeige eines benutzerdefinierten Labels für ein Feld oder eine Eigenschaft.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class KWDebugAttribute : Attribute
    {
        /// <summary>
        /// Das anzuzeigende Label.
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// Initialisiert eine neue Instanz des <see cref="KWDebugAttribute"/> mit einem optionalen Label.
        /// </summary>
        /// <param name="label">Das anzuzeigende Label (optional).</param>
        public KWDebugAttribute(string label = null) => Label = label;
    }
}
