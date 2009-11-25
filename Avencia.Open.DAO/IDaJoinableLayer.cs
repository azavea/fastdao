using Avencia.Open.DAO.Criteria.Joins;

namespace Avencia.Open.DAO
{
    /// <summary>
    /// If the layer supports joins natively (such as SQL statements in the database)
    /// it will implement this interface.
    /// </summary>
    public interface IDaJoinableLayer
    {
        /// <summary>
        /// This returns whether or not this layer can perform the requested
        /// join natively.  This lets a layer that can have native joins determine
        /// whether this particular join is able to be done natively.
        /// </summary>
        /// <typeparam name="R">The type of object returned by the other DAO.</typeparam>
        /// <param name="crit">The criteria specifying the requested join.</param>
        /// <param name="rightConn">The connection info for the other DAO we're joining with.</param>
        /// <param name="rightMapping">Class mapping for the right table we would be querying against.</param>
        /// <returns>True if we can perform the join natively, false if we cannot.</returns>
        bool CanJoin<R>(DaoJoinCriteria crit, ConnectionDescriptor rightConn, ClassMapping rightMapping) where R : new();

        /// <summary>
        /// This is not garaunteed to succeed unless CanJoin(crit, rightDao) returns true.
        /// </summary>
        /// <param name="crit">The criteria specifying the requested join.</param>
        /// <param name="leftMapping">Class mapping for the left table we're querying against.</param>
        /// <param name="rightMapping">Class mapping for the right table we're querying against.</param>
        IDaJoinQuery CreateJoinQuery(DaoJoinCriteria crit, ClassMapping leftMapping,
                                          ClassMapping rightMapping);
    }
}