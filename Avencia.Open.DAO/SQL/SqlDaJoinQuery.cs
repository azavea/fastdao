namespace Avencia.Open.DAO.SQL
{
    /// <summary>
    /// A SQL query that joins two tables, can be run by the SqlDaLayer.
    /// </summary>
    public class SqlDaJoinQuery : SqlDaQuery, IDaJoinQuery
    {
        private string _leftPrefix;
        private string _rightPrefix;

        /// <summary>
        /// Populates the prefix strings.
        /// </summary>
        /// <param name="left">Prefix for columns from the left table.</param>
        /// <param name="right">Prefix for columns from the right table.</param>
        public void SetPrefixes(string left, string right)
        {
            _leftPrefix = left;
            _rightPrefix = right;
        }

        /// <summary>
        /// The prefix that should be used to get the left table's columns out of the IDataReader
        /// when accessing them by name.
        /// </summary>
        /// <returns>The prefix for columns in the left table (I.E. "left_table.")</returns>
        public string GetLeftColumnPrefix()
        {
            return _leftPrefix;
        }

        /// <summary>
        /// The prefix that should be used to get the right table's columns out of the IDataReader
        /// when accessing them by name.
        /// </summary>
        /// <returns>The prefix for columns in the right table (I.E. "right_table.")</returns>
        public string GetRightColumnPrefix()
        {
            return _rightPrefix;
        }
    }
}