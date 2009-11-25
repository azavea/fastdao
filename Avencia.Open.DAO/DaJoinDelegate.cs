using System.Collections;
using System.Data;

namespace Avencia.Open.DAO
{
    /// <summary>
    /// This describes a method to be executed by the ExecuteJoin call.  This delegate
    /// will be called once and passed the IDataReader that was the result of the join query.  It is
    /// up to the delegate to iterate through the results if it wants to.
    /// </summary>
    /// <param name="parameters">A hashtable containing anything at all.  This is used as
    ///                          a way of passing parameters to the delegate, or as a way
    ///                          for the delegate to return values to the function that called
    ///                          it through the ExecuteJoin method.
    ///                          This parameter may be null.</param>
    /// <param name="leftColumnPrefix">The prefix to use when querying the IDataReader for the
    ///                                columns of the left table by name.  Either leftColumnPrefix or
    ///                                rightColumnPrefix can be null, but both cannot be null due
    ///                                to the possibility of duplicate columns (if both tables have an
    ///                                "ID" column for example).  This may be the table name, or a
    ///                                made up string like "left" or "table1".</param>
    /// <param name="rightColumnPrefix">The prefix to use when querying the IDataReader for the
    ///                                columns of the right table by name.  Either leftColumnPrefix or
    ///                                rightColumnPrefix can be null, but both cannot be null due
    ///                                to the possibility of duplicate columns (if both tables have an
    ///                                "ID" column for example).  This may be the table name, or a
    ///                                made up string like "right" or "table1".</param>
    /// <param name="reader">The data reader with the results of the database query.</param>
    public delegate void DaJoinDelegate(Hashtable parameters, IDataReader reader,
                                        string leftColumnPrefix, string rightColumnPrefix);
}