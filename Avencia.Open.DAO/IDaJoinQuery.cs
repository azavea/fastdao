namespace Avencia.Open.DAO
{
    /// <summary>
    /// This is an interface that defines a join query that can be run by an IDaJoinableLayer.
    /// Since the layer defines what (if any) table aliases are used, it needs a way to
    /// communicate what (if anything) the columns in the data reader will be prefixed with.
    /// </summary>
    public interface IDaJoinQuery : IDaQuery
    {
        /// <summary>
        /// The prefix that should be used to get the left table's columns out of the IDataReader
        /// when accessing them by name.
        /// </summary>
        /// <returns>The prefix for columns in the left table (I.E. "left_table.")</returns>
        string GetLeftColumnPrefix();
        /// <summary>
        /// The prefix that should be used to get the right table's columns out of the IDataReader
        /// when accessing them by name.
        /// </summary>
        /// <returns>The prefix for columns in the right table (I.E. "right_table.")</returns>
        string GetRightColumnPrefix();
    }
}