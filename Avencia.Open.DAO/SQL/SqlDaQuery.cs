using System.Collections.Generic;
using System.Text;

namespace Avencia.Open.DAO.SQL
{
    /// <summary>
    /// A "normal" SQL query, can be run by the SqlDaLayer.
    /// </summary>
    public class SqlDaQuery : IDaQuery
    {
        /// <summary>
        /// The SQL statement to run, hopefully parameterized.
        /// </summary>
        public readonly StringBuilder Sql = new StringBuilder();
        /// <summary>
        /// Any parameters for the SQL statement.
        /// </summary>
        public readonly List<object> Params = new List<object>();

        /// <summary>
        /// Clears the contents of the query, allowing the object to be reused.
        /// </summary>
        public virtual void Clear()
        {
            Params.Clear();
            Sql.Remove(0, Sql.Length);
        }

        ///<summary>
        ///Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        ///</summary>
        ///
        ///<returns>
        ///A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        public override string ToString()
        {
            return SqlUtilities.SqlParamsToString(Sql.ToString(), Params);
        }
    }
}