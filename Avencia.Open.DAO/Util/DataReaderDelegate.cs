using System.Collections;
using System.Data;

namespace Avencia.Open.DAO.Util
{
    /// <summary>
    /// This describes a method to be executed once an IDataReader has been obtained.  This delegate
    /// will be called once and passed the IDataReader that was the result of the query.  It is
    /// up to the delegate to iterate through the results if it wants to.
    /// </summary>
    /// <param name="parameters">A hashtable containing anything at all.  This is used as
    ///                          a way of passing parameters to the delegate, or as a way
    ///                          for the delegate to return values to the function that called
    ///                          it.  This parameter may be null.</param>
    /// <param name="reader">The data reader with the results of the database query.</param>
    public delegate void DataReaderDelegate(Hashtable parameters, IDataReader reader);
}