// Copyright (c) 2004-2010 Azavea, Inc.
// 
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;
using Azavea.Open.DAO.Criteria;
using Azavea.Open.DAO.Criteria.Grouping;
using Azavea.Open.DAO.Util;

namespace Azavea.Open.DAO
{
    /// <summary>
    /// Defines an interface that FastDAO can use to run data access functions that 
    /// are specific to a particular data source (e.g. sql-based database).
    /// 
    /// This class, and all classes that extend it, should be thread-safe.
    /// 
    /// NOTE on transactions: The methods on the interface accept transactions,
    /// but if your data source does not support transactions (or you have not
    /// yet implemented support), you may ignore that parameter as long as your
    /// equivilent IConnectionDescriptor is not an ITransactionalConnectionDescriptor.
    /// </summary>
    public interface IDaLayer
    {
        #region Methods Defining What Is Supported On This Layer

        /// <summary>
        /// If true, methods that return numbers of records affected will be
        /// returning accurate numbers.  If false, they will return
        /// UNKNOWN_NUM_ROWS.
        /// </summary>
        bool SupportsNumRecords();

        #endregion

        #region Methods For Modifying Behavior

        /// <summary>
        /// A method to add a coercion delegate for a type, without exposing the dictionary.
        /// </summary>
        /// <param name="t">The type to coerce.</param>
        /// <param name="coercionDelegate">How to coerce it.</param>
        void AddCoercibleType(Type t, TypeCoercionDelegate coercionDelegate);

        #endregion

        #region Delete
        /// <summary>
        /// Deletes a data object record using the mapping and criteria for what's deleted.
        /// </summary>
        /// <param name="transaction">The transaction to do this as part of. May be null.</param>
        /// <param name="mapping">The mapping of the table from which to delete.</param>
        /// <param name="crit">Criteria for deletion.  NOTE: Only the expressions are observed,
        ///                    other things (like "order" or start / limit) are ignored.
        ///                    WARNING: A null or empty (no expression) criteria will 
        ///                    delete ALL records!</param>
        /// <returns>The number of records affected.</returns>
        int Delete(ITransaction transaction, ClassMapping mapping, DaoCriteria crit);

        /// <summary>
        /// Deletes all contents of the table.  Faster for large tables than DeleteAll,
        /// but requires greater permissions.  For layers that do not support this, the
        /// behavior should be the same as calling Delete(null, mapping).
        /// </summary>
        void Truncate(ClassMapping mapping);

        #endregion

        #region Insert
        /// <summary>
        /// Inserts a data object record using the "table" and a list of column/value pairs.
        /// </summary>
        /// <param name="transaction">The transaction to do this as part of. May be null.</param>
        /// <param name="mapping">The mapping of the table or other data container we're dealing with.</param>
        /// <param name="propValues">A dictionary of "column"/value pairs for the object to insert.</param>
        /// <returns>The number of records affected.</returns>
        int Insert(ITransaction transaction, ClassMapping mapping, IDictionary<string, object> propValues);

        /// <summary>
        /// Inserts a list of data object records of the same type.
        /// </summary>
        /// <param name="transaction">The transaction to do this as part of. May be null.</param>
        /// <param name="mapping">The mapping of the table or other data container we're dealing with.</param>
        /// <param name="propValueDictionaries">A list of dictionaries of column/value pairs.  
        ///                                     Each item in the list should represent the dictionary of column/value pairs for 
        ///                                     each respective object being inserted.</param>
        void InsertBatch(ITransaction transaction, ClassMapping mapping, List<IDictionary<string, object>> propValueDictionaries);
        #endregion

        #region Update
        /// <summary>
        /// Updates a data object record using the "table" and a list of column/value pairs.
        /// </summary>
        /// <param name="transaction">The transaction to do this as part of. May be null.</param>
        /// <param name="mapping">The mapping of the table or other data container we're dealing with.</param>
        /// <param name="crit">All records matching this criteria will be updated per the dictionary of
        ///                    values.</param>
        /// <param name="propValues">A dictionary of column/value pairs for all non-ID columns to be updated.</param>
        /// <returns>The number of records affected.</returns>
        int Update(ITransaction transaction, ClassMapping mapping, DaoCriteria crit, IDictionary<string, object> propValues);

        /// <summary>
        /// Updates a list of data object records of the same type.
        /// </summary>
        /// <param name="transaction">The transaction to do this as part of. May be null.</param>
        /// <param name="mapping">The mapping of the table or other data container we're dealing with.</param>
        /// <param name="criteriaList">A list of DaoCriteria.
        ///                            Each item in the list should represent the criteria for 
        ///                            rows that will be updated per the accompanying dictionary.</param>
        /// <param name="propValueDictionaries">A list of dictionaries of column/value pairs.
        ///                                   Each item in the list should represent the dictionary of non-ID column/value pairs for 
        ///                                   each respective object being updated.</param>
        void UpdateBatch(ITransaction transaction, ClassMapping mapping, List<DaoCriteria> criteriaList,
                                         List<IDictionary<string, object>> propValueDictionaries);
        #endregion

        #region Querying
        /// <summary>
        /// Builds the query based on a serializable criteria.  The Query object is particular to
        /// the implementation, but may contain things like the parameters parsed out, or whatever
        /// makes sense to this FastDaoLayer.  You can think of this method as a method to convert
        /// from the generic DaoCriteria into the specific details necessary for querying.
        /// </summary>
        /// <param name="mapping">The mapping of the table for which to build the query string.</param>
        /// <param name="crit">The criteria to use to find the desired objects.</param>
        /// <returns>A query that can be run by ExecureQuery.</returns>
        IDaQuery CreateQuery(ClassMapping mapping, DaoCriteria crit);

        /// <summary>
        /// Executes a query and invokes a method with a DataReader of results.
        /// </summary>
        /// <param name="transaction">The transaction to do this as part of. May be null.</param>
        /// <param name="mapping">Class mapping for the table we're querying against.  Optional,
        ///                       but not all columns may be properly typed if it is null.</param>
        /// <param name="query">The query to execute, should have come from CreateQuery.</param>
        /// <param name="invokeMe">The method to invoke with the IDataReader results.</param>
        /// <param name="parameters">A hashtable containing any values that need to be persisted through invoked method.
        ///                          The list of objects from the query will be placed here.</param>
        void ExecuteQuery(ITransaction transaction, ClassMapping mapping, IDaQuery query, DataReaderDelegate invokeMe, Hashtable parameters);

        /// <summary>
        /// Should be called when you're done with the query.  Allows us to cache the
        /// objects for reuse.
        /// </summary>
        /// <param name="query">Query you're done using.</param>
        void DisposeOfQuery(IDaQuery query);

        /// <summary>
        /// Gets a count of records for the given criteria.
        /// </summary>
        /// <param name="transaction">The transaction to do this as part of. May be null.</param>
        /// <param name="mapping">The mapping of the table for which to build the query string.</param>
        /// <param name="crit">The criteria to use for "where" comparisons.</param>
        /// <returns>The number of results found that matched the criteria.</returns>
        int GetCount(ITransaction transaction, ClassMapping mapping, DaoCriteria crit);

        /// <summary>
        /// Gets a count of records for the given criteria,
        /// aggregated by the given grouping expressions.  This matches "GROUP BY" behavior
        /// in SQL.
        /// </summary>
        /// <param name="transaction">The transaction to do this as part of. May be null.</param>
        /// <param name="mapping">The mapping of the table for which to build the query string.</param>
        /// <param name="crit">The criteria to use for "where" comparisons.</param>
        /// <param name="groupExpressions">The fields/expressions to aggregate on when counting.</param>
        /// <returns>The number of objects that match the criteria, plus the values of those objects
        ///          for the fields that were aggregated on.</returns>
        List<GroupCountResult> GetCount(ITransaction transaction, ClassMapping mapping, DaoCriteria crit, ICollection<AbstractGroupExpression> groupExpressions);
        #endregion

        #region Utility Methods

        /// <summary>
        /// Attempts to convert the value into the given type.  While broadly
        /// similar to Convert.ChangeType, that method doesn't support enums and this one does.
        /// Calling that from within this method makes it take nearly twice as long, so this method
        /// does its own type checking.
        /// </summary>
        /// <param name="desiredType">Type we need the value to be.</param>
        /// <param name="input">Input value, may or may not already be the right type.</param>
        /// <returns>An object of type desiredType whose value is equal to the input.</returns>
        object CoerceType(Type desiredType, object input);

        /// <summary>
        /// Finds the last generated id number for a column.
        /// </summary>
        /// <param name="transaction">The transaction to do this as part of. May be null.</param>
        /// <param name="mapping">The class mapping for the table being queried.</param>
        /// <param name="idCol">The ID column for which to find the last-generated ID.</param>
        object GetLastAutoGeneratedId(ITransaction transaction, ClassMapping mapping, string idCol);

        /// <summary>
        /// Gets the next id number from a sequence in the data source.
        /// </summary>
        /// <param name="transaction">The transaction to do this as part of. May be null.</param>
        /// <param name="sequenceName">The name of the sequence.</param>
        /// <returns>The next number from the sequence.</returns>
        int GetNextSequenceValue(ITransaction transaction, string sequenceName);

        #endregion
    }

    /// <summary>
    /// Delegate for type coercion methods.
    /// </summary>
    /// <param name="input">The input value coming out of the reader.</param>
    /// <returns>The input value coerced into an object of the type required.</returns>
    public delegate object TypeCoercionDelegate(object input);
}