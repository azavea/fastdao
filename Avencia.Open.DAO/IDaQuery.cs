namespace Avencia.Open.DAO
{
    /// <summary>
    /// This is an interface that defines a query that can be run by an IDaLayer.
    /// </summary>
    public interface IDaQuery
    {
        /// <summary>
        /// Clears the contents of the query, allowing the object to be reused.
        /// </summary>
        void Clear();
    }
}