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
using System.ComponentModel;
using System.Text;
using Azavea.Open.Common;
using Azavea.Open.DAO.Criteria;
using Azavea.Open.DAO.Criteria.Grouping;
using Azavea.Open.DAO.Exceptions;
using Azavea.Open.DAO.Util;
using log4net;

namespace Azavea.Open.DAO
{
    /// <summary>
    /// Defines an interface that FastDAO can use to run data access functions that 
    /// are specific to a particular data source (e.g. sql-based database).
    /// 
    /// This class, and all classes that extend it, should be thread-safe.
    /// </summary>
    public abstract class AbstractDaLayer : IDaLayer
    {
        /// <summary>
        /// log4net logger that any child class may use for logging any appropriate messages.
        /// </summary>
        protected static ILog _log = LogManager.GetLogger(
            new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().DeclaringType.Namespace);

        /// <summary>
        /// The connection descriptor that is being used by this FastDaoLayer.
        /// </summary>
        protected readonly IConnectionDescriptor _connDesc;

        #region Properties Defining What Is Supported On This Layer

        private readonly bool _supportsNumRecords;

        /// <summary>
        /// If true, methods that return numbers of records affected will be
        /// returning accurate numbers.  If false, they will return
        /// UNKNOWN_NUM_ROWS.
        /// </summary>
        public virtual bool SupportsNumRecords()
        {
            return _supportsNumRecords;
        }

        #endregion

        /// <summary>
        /// A mapping of additional types that can be handled by this DAO to methods for handling them.  
        /// It is initially null, but can be created and aded to by a subclass to easily allow for
        /// handling arbitrary data types.
        /// 
        /// Use must be synchronized!
        /// </summary>
        protected Dictionary<Type, TypeCoercionDelegate> _coerceableTypes;

        #region Constructors
        /// <summary>
        /// Instantiates the data access layer with the connection descriptor for the DB.
        /// </summary>
        /// <param name="connDesc">The connection descriptor that is being used by this FastDaoLayer.</param>
        /// <param name="supportsNumRecords">If true, methods that return numbers of records affected will be
        ///                                 returning accurate numbers.  If false, they will probably return
        ///                                 FastDAO.UNKNOWN_NUM_ROWS.</param>
        protected AbstractDaLayer(IConnectionDescriptor connDesc, bool supportsNumRecords)
        {
            _connDesc = connDesc;
            _supportsNumRecords = supportsNumRecords;
        }
        #endregion

        #region Methods For Modifying Behavior
        /// <summary>
        /// A method to add a coercion delegate for a type, without exposing the dictionary.
        /// </summary>
        /// <param name="t">The type to coerce.</param>
        /// <param name="coercionDelegate">How to coerce it.</param>
        public void AddCoercibleType(Type t, TypeCoercionDelegate coercionDelegate)
        {
            lock (this)
            {
                if (_coerceableTypes == null)
                {
                    _coerceableTypes = new Dictionary<Type, TypeCoercionDelegate>();
                }
                _coerceableTypes.Add(t, coercionDelegate);
            }
        }
        #endregion

        #region Delete
        /// <summary>
        /// Deletes a data object record using the mapping and criteria for what's deleted.
        /// </summary>
        /// <param name="transaction">The transaction to do this as part of.</param>
        /// <param name="mapping">The mapping of the table from which to delete.</param>
        /// <param name="crit">Criteria for deletion.  NOTE: Only the expressions are observed,
        ///                    other things (like "order" or start / limit) are ignored.
        ///                    WARNING: A null or empty (no expression) criteria will 
        ///                    delete ALL records!</param>
        /// <returns>The number of records affected.</returns>
        public abstract int Delete(ITransaction transaction, ClassMapping mapping, DaoCriteria crit);

        /// <summary>
        /// Deletes all contents of the table.  Faster for large tables than DeleteAll,
        /// but requires greater permissions.  For layers that do not support this, the
        /// behavior should be the same as calling Delete(null, mapping).
        /// </summary>
        public abstract void Truncate(ClassMapping mapping);

        #endregion

        #region Insert
        /// <summary>
        /// Inserts a data object record using the "table" and a list of column/value pairs.
        /// </summary>
        /// <param name="transaction">The transaction to do this as part of.</param>
        /// <param name="mapping">The mapping of the table or other data container we're dealing with.</param>
        /// <param name="propValues">A dictionary of "column"/value pairs for the object to insert.</param>
        /// <returns>The number of records affected.</returns>
        public abstract int Insert(ITransaction transaction, ClassMapping mapping, IDictionary<string, object> propValues);

        /// <summary>
        /// Inserts a list of data object records of the same type.  The default implementation
        /// merely calls Insert for each one, however some datasources may have more efficient
        /// ways of inserting multiple records that the appropriate DaLayer will take advantage of.
        /// </summary>
        /// <param name="transaction">The transaction to do this as part of.</param>
        /// <param name="mapping">The mapping of the table or other data container we're dealing with.</param>
        /// <param name="propValueDictionaries">A list of dictionaries of column/value pairs.  
        ///                                     Each item in the list should represent the dictionary of column/value pairs for 
        ///                                     each respective object being inserted.</param>
        public virtual void InsertBatch(ITransaction transaction, ClassMapping mapping, List<IDictionary<string, object>> propValueDictionaries)
        {
            foreach(Dictionary<string, object> propValues in propValueDictionaries)
            {
                Insert(transaction, mapping, propValues);
            }
        }
        #endregion

        #region Update
        /// <summary>
        /// Updates a data object record using the "table" and a list of column/value pairs.
        /// </summary>
        /// <param name="transaction">The transaction to do this as part of.</param>
        /// <param name="mapping">The mapping of the table or other data container we're dealing with.</param>
        /// <param name="crit">All records matching this criteria will be updated per the dictionary of
        ///                    values.</param>
        /// <param name="propValues">A dictionary of column/value pairs for all non-ID columns to be updated.</param>
        /// <returns>The number of records affected.</returns>
        public abstract int Update(ITransaction transaction, ClassMapping mapping, DaoCriteria crit, IDictionary<string, object> propValues);

        /// <summary>
        /// Updates a list of data object records of the same type.  The default implementation
        /// merely calls Update for each one, however some datasources may have more efficient
        /// ways of inserting multiple records that the appropriate DaLayer will take advantage of.
        /// </summary>
        /// <param name="transaction">The transaction to do this as part of.</param>
        /// <param name="mapping">The mapping of the table or other data container we're dealing with.</param>
        /// <param name="criteriaList">A list of DaoCriteria.
        ///                            Each item in the list should represent the criteria for 
        ///                            rows that will be updated per the accompanying dictionary.</param>
        /// <param name="propValueDictionaries">A list of dictionaries of column/value pairs.
        ///                                   Each item in the list should represent the dictionary of non-ID column/value pairs for 
        ///                                   each respective object being updated.</param>
        public virtual void UpdateBatch(ITransaction transaction, ClassMapping mapping, List<DaoCriteria> criteriaList,
                                         List<IDictionary<string, object>> propValueDictionaries)
        {
            for (int i = 0; i < criteriaList.Count; i++)
            {
                Update(transaction, mapping, criteriaList[i], propValueDictionaries[i]);
            }
        }
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
        public abstract IDaQuery CreateQuery(ClassMapping mapping, DaoCriteria crit);

        /// <summary>
        /// Executes a query and invokes a method with a DataReader of results.
        /// </summary>
        /// <param name="transaction">The transaction to do this as part of.</param>
        /// <param name="mapping">Class mapping for the table we're querying against.  Optional,
        ///                       but not all columns may be properly typed if it is null.</param>
        /// <param name="query">The query to execute, should have come from CreateQuery.</param>
        /// <param name="invokeMe">The method to invoke with the IDataReader results.</param>
        /// <param name="parameters">A hashtable containing any values that need to be persisted through invoked method.
        ///                          The list of objects from the query will be placed here.</param>
        public abstract void ExecuteQuery(ITransaction transaction, ClassMapping mapping, IDaQuery query, DataReaderDelegate invokeMe, Hashtable parameters);

        /// <summary>
        /// Should be called when you're done with the query.  Allows us to cache the
        /// objects for reuse.
        /// </summary>
        /// <param name="query">Query you're done using.</param>
        public abstract void DisposeOfQuery(IDaQuery query);

        /// <summary>
        /// Gets a count of records for the given criteria.
        /// </summary>
        /// <param name="transaction">The transaction to do this as part of.</param>
        /// <param name="mapping">The mapping of the table for which to build the query string.</param>
        /// <param name="crit">The criteria to use for "where" comparisons.</param>
        /// <returns>The number of results found that matched the criteria.</returns>
        public abstract int GetCount(ITransaction transaction, ClassMapping mapping, DaoCriteria crit);

        /// <summary>
        /// Gets a count of records for the given criteria,
        /// aggregated by the given grouping expressions.  This matches "GROUP BY" behavior
        /// in SQL.
        /// </summary>
        /// <param name="transaction">The transaction to do this as part of.</param>
        /// <param name="mapping">The mapping of the table for which to build the query string.</param>
        /// <param name="crit">The criteria to use for "where" comparisons.</param>
        /// <param name="groupExpressions">The fields/expressions to aggregate on when counting.</param>
        /// <returns>The number of objects that match the criteria, plus the values of those objects
        ///          for the fields that were aggregated on.</returns>
        public abstract List<GroupCountResult> GetCount(ITransaction transaction, ClassMapping mapping, DaoCriteria crit,
                                                        ICollection<AbstractGroupExpression> groupExpressions);

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
        public virtual object CoerceType(Type desiredType, object input)
        {
            try
            {
                // For speed, put the most common checks at the top.
                if (desiredType.IsInstanceOfType(input))
                {
                    return input;
                }
                if (desiredType.Equals(typeof (string)))
                {
                    return Convert.ToString(input);
                }
                if (desiredType.Equals(typeof (int)))
                {
                    return Convert.ToInt32(input);
                }
                if (desiredType.Equals(typeof (long)))
                {
                    return Convert.ToInt64(input);
                }
                if (desiredType.Equals(typeof (double)))
                {
                    return Convert.ToDouble(input);
                }
                if (desiredType.Equals(typeof (DateTime)))
                {
                    return Convert.ToDateTime(input);
                }
                if (desiredType.IsEnum)
                {
                    return (input is int) ? input : Enum.Parse(desiredType, input.ToString());
                }
                if (desiredType.Equals(typeof (bool)))
                {
                    return Convert.ToBoolean(input);
                }
                if (desiredType.Equals(typeof (short)))
                {
                    return Convert.ToInt16(input);
                }
                if (desiredType.Equals(typeof (byte)))
                {
                    return Convert.ToByte(input);
                }
                if (desiredType.Equals(typeof (char)))
                {
                    return Convert.ToChar(input);
                }
                if (desiredType.Equals(typeof (float)))
                {
                    return (float) Convert.ToDouble(input);
                }
                if (desiredType.Equals(typeof (DateTime?)))
                {
                    if (input == null)
                    {
                        return null;
                    }
                    return Convert.ToDateTime(input);
                }
                if (desiredType.Equals(typeof(int?)))
                {
                    if (input == null)
                    {
                        return null;
                    }
                    return Convert.ToInt32(input);
                }
                if (desiredType.Equals(typeof(long?)))
                {
                    if (input == null)
                    {
                        return null;
                    }
                    return Convert.ToInt64(input);
                }
                if (desiredType.Equals(typeof(double?)))
                {
                    if (input == null)
                    {
                        return null;
                    }
                    return Convert.ToDouble(input);
                }
                if (desiredType.Equals(typeof (float?)))
                {
                    if (input == null)
                    {
                        return null;
                    }
                    return (float) Convert.ToDouble(input);
                }
                if (desiredType.Equals(typeof (bool?)))
                {
                    if (input == null)
                    {
                        return null;
                    }
                    return Convert.ToBoolean(input);
                }
                if (desiredType.Equals(typeof (byte[])))
                {
                    // Cast to byte array here so we'll throw if the type is incompatible.
                    // Technically we don't have to, since this returns object, but we want
                    // this to be the method that throws the type cast exception.
                    byte[] retVal = (byte[]) input;
                    return retVal;
                }
                // Nullables are generics, so nullable enums are more work to check for.
                if (desiredType.IsGenericType &&
                    desiredType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                {
                    // Technically this first check will work for any nullable type, not just enums.
                    if (input == null)
                    {
                        return null;
                    }
                    // Note that we're only handling nullables, which have 1 generic param
                    // which is why the [0] is correct.
                    Type genericType = desiredType.GetGenericArguments()[0];
                    if (genericType.IsEnum)
                    {
                        return (input is int)
                            // Unlike normal enums, integers cannot simply be set on a nullable enum.
                            // So we have to call ToObject to convert it to an enum value first.
                            ? Enum.ToObject(genericType, input)
                            // Since it is a nullable enum, we're allowing blank or all-whitespace
                            // strings to count as nulls.
                            : (StringHelper.IsNonBlank(input.ToString())
                                ? Enum.Parse(genericType, input.ToString())
                                : null);
                    }
                }

                // For misc object types, we'll just check if it is already the correct type.
                if (desiredType.IsInstanceOfType(input))
                {
                    return input;
                }

                // If it's mapped as an AsciiString, put the value in as a byte array of
                // ascii characters.  This supports old-style (non-unicode) varchars.
                if (desiredType.Equals(typeof (AsciiString)))
                {
                    return Encoding.ASCII.GetBytes(input.ToString());
                }

                // Have subclasses be able to add their own coerced types.
                lock (this)
                {
                    if (_coerceableTypes != null)
                    {
                        foreach (Type t in _coerceableTypes.Keys)
                        {
                            if (t.IsAssignableFrom(desiredType))
                            {
                                return _coerceableTypes[t].DynamicInvoke(input);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new DaoTypeCoercionException(desiredType, input, e);
            }
            // To add support for lists, you'd need to add:
            //if (desiredType.IsSubclassOf(typeof(IList)))
            //{
            //    // input is some sort of list ID, so go run a subquery or something...
            //}
            // You'd also need to check if the input was a list, and do something special rather
            // than just try to convert it to an int or whatever.
            // Custom class types would be similar... Hmm, and you'd have to add support to the
            // class mapping to handle nested classes.  OK it's a tad more complicated, but it
            // is doable if necessary.
            // Oh yeah and then you have to handle transactions correctly, so if a second query fails
            // you remember to roll back the first one... And there are probably a billion other details...
            throw new DaoUnsupportedTypeCoercionException(desiredType, input);
        }

        /// <summary>
        /// Finds the last generated id number for a column.
        /// </summary>
        /// <param name="transaction">The transaction to do this as part of.</param>
        /// <param name="mapping">The class mapping for the table being queried.</param>
        /// <param name="idCol">The ID column for which to find the last-generated ID.</param>
        public abstract object GetLastAutoGeneratedId(ITransaction transaction, ClassMapping mapping, string idCol);

        /// <summary>
        /// Gets the next id number from a sequence in the data source.
        /// </summary>
        /// <param name="transaction">The transaction to do this as part of.</param>
        /// <param name="sequenceName">The name of the sequence.</param>
        /// <returns>The next number from the sequence.</returns>
        public abstract int GetNextSequenceValue(ITransaction transaction, string sequenceName);
        #endregion
    }
}