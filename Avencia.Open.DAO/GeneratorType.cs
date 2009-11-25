namespace Avencia.Open.DAO
{
    /// <summary>
    /// Indicates how IDs are generated for objects.
    /// </summary>
    public enum GeneratorType
    {
        /// <summary>
        /// There is no automatic ID generation, ID fields must be set correctly
        /// before the object is saved.
        /// </summary>
        NONE,
        /// <summary>
        /// The database does it for us, via triggers, autonumbers, etc.
        /// </summary>
        AUTO,
        /// <summary>
        /// The DAO class needs to access a sequence in the DB to get the next
        /// available ID (currently supported only on Oracle).
        /// </summary>
        SEQUENCE
    }
}
