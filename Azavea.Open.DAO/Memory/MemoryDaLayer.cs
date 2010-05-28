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
using Azavea.Open.Common.Collections;
using Azavea.Open.DAO.Criteria;
using Azavea.Open.DAO.Criteria.Grouping;
using Azavea.Open.DAO.Unqueryable;
using Azavea.Open.DAO.Util;

namespace Azavea.Open.DAO.Memory
{
    /// <summary>
    /// Data access layer implementation that simply stores objects in memory.
    /// </summary>
    public class MemoryDaLayer : UnqueryableDaLayer
    {
        private readonly IDictionary<string, IDictionary<string, MemoryObject>> _datastore =
            new CheckedDictionary<string, IDictionary<string, MemoryObject>>();

        private readonly IDictionary<string, int> _sequences =
            new CheckedDictionary<string, int>();
        private readonly IDictionary<string, int> _lastAutogenIds =
            new CheckedDictionary<string, int>();

        /// <summary>
        /// Create a new memory store.
        /// </summary>
        /// <param name="connDesc">Connection descriptor to use with this layer.</param>
        public MemoryDaLayer(IConnectionDescriptor connDesc) : base(connDesc, true)
        {
        }

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
        public override int Delete(ITransaction transaction, ClassMapping mapping, DaoCriteria crit)
        {
            // Find all the records that don't match, and just keep those.
            DaoCriteria inverseCrit = new DaoCriteria();
            foreach (IExpression expr in crit.Expressions)
            {
                inverseCrit.Expressions.Add(expr.Invert());
            }
            IDictionary<string, MemoryObject> table = GetTable(mapping);
            int retVal;
            lock (table)
            {
                int oldCount = table.Count;
                MemoryDataReader reader = new MemoryDataReader(this, mapping, inverseCrit,
                                                               table.Values.GetEnumerator());
                IDictionary<string, MemoryObject> tempTable = new Dictionary<string, MemoryObject>();
                while (reader.Read())
                {
                    MemoryObject keeper = reader.GetCurrentObject();
                    tempTable[keeper.GetKey()] = keeper;
                }
                table.Clear();
                foreach (KeyValuePair<string, MemoryObject> kvp in tempTable)
                {
                    table.Add(kvp);
                }
                retVal = oldCount - table.Count;
            }
            return retVal;
        }

        /// <summary>
        /// Deletes all contents of the table.  Faster for large tables than DeleteAll,
        /// but requires greater permissions.  For layers that do not support this, the
        /// behavior should be the same as calling Delete(null, mapping).
        /// </summary>
        public override void Truncate(ClassMapping mapping)
        {
            IDictionary<string, MemoryObject> table = GetTable(mapping);
            lock (table)
            {
                table.Clear();
            }
        }

        /// <summary>
        /// Inserts a data object record using the "table" and a list of column/value pairs.
        /// </summary>
        /// <param name="transaction">The transaction to do this as part of.</param>
        /// <param name="mapping">The mapping of the table or other data container we're dealing with.</param>
        /// <param name="propValues">A dictionary of "column"/value pairs for the object to insert.</param>
        /// <returns>The number of records affected.</returns>
        public override int Insert(ITransaction transaction, ClassMapping mapping, IDictionary<string, object> propValues)
        {
            IDictionary<string, MemoryObject> table = GetTable(mapping);
            foreach (string colName in mapping.IdDataColsByObjAttrs.Values)
            {
                propValues[colName] = GetNextAutoGeneratedId(mapping, colName);
            }
            MemoryObject obj = new MemoryObject(mapping, propValues);
            lock (table)
            {
                table[obj.GetKey()] = obj;
            }
            return 1;
        }

        /// <summary>
        /// Inserts a list of data object records of the same type.
        /// </summary>
        /// <param name="transaction">The transaction to do this as part of.</param>
        /// <param name="mapping">The mapping of the table or other data container we're dealing with.</param>
        /// <param name="propValueDictionaries">A list of dictionaries of column/value pairs.  
        ///                                     Each item in the list should represent the dictionary of column/value pairs for 
        ///                                     each respective object being inserted.</param>
        public override void InsertBatch(ITransaction transaction, ClassMapping mapping, List<IDictionary<string, object>> propValueDictionaries)
        {
            foreach (IDictionary<string, object> setOfValues in propValueDictionaries)
            {
                Insert(transaction, mapping, setOfValues);
            }
        }

        /// <summary>
        /// Updates a data object record using the "table" and a list of column/value pairs.
        /// </summary>
        /// <param name="transaction">The transaction to do this as part of.</param>
        /// <param name="mapping">The mapping of the table or other data container we're dealing with.</param>
        /// <param name="crit">All records matching this criteria will be updated per the dictionary of
        ///                    values.</param>
        /// <param name="propValues">A dictionary of column/value pairs for all non-ID columns to be updated.</param>
        /// <returns>The number of records affected.</returns>
        public override int Update(ITransaction transaction, ClassMapping mapping, DaoCriteria crit, IDictionary<string, object> propValues)
        {
            // Since the update might change one of the id fields, we can't just overwrite the
            // table with new values (since that might be an add).  Instead create a temp table,
            // put in everything that doesn't match, then put in everything that does after
            // modifying it.
            DaoCriteria inverseCrit = new DaoCriteria();
            foreach (IExpression expr in crit.Expressions)
            {
                inverseCrit.Expressions.Add(expr.Invert());
            }
            IDictionary<string, MemoryObject> table = GetTable(mapping);
            int retVal = 0;
            lock (table)
            {
                IDictionary<string, MemoryObject> tempTable = new Dictionary<string, MemoryObject>();

                // First the unchanged records.
                MemoryDataReader reader = new MemoryDataReader(this, mapping, inverseCrit,
                                                               table.Values.GetEnumerator());
                while (reader.Read())
                {
                    MemoryObject unchanged = reader.GetCurrentObject();
                    tempTable[unchanged.GetKey()] = unchanged;
                }
                // Now the changed ones.
                reader = new MemoryDataReader(this, mapping, crit, table.Values.GetEnumerator());
                while (reader.Read())
                {
                    MemoryObject changed = reader.GetCurrentObject();
                    // Set the changed values.
                    foreach (KeyValuePair<string, object> kvp in propValues)
                    {
                        changed.ColValues[kvp.Key] = kvp.Value;
                    }
                    tempTable[changed.GetKey()] = changed;
                    retVal++;
                }
                // Now replace the real table contents with the temp ones.
                table.Clear();
                foreach (KeyValuePair<string, MemoryObject> kvp in tempTable)
                {
                    table.Add(kvp);
                }
            }
            return retVal;
        }

        /// <summary>
        /// Updates a list of data object records of the same type.
        /// </summary>
        /// <param name="transaction">The transaction to do this as part of.</param>
        /// <param name="mapping">The mapping of the table or other data container we're dealing with.</param>
        /// <param name="criteriaList">A list of DaoCriteria.
        ///                            Each item in the list should represent the criteria for 
        ///                            rows that will be updated per the accompanying dictionary.</param>
        /// <param name="propValueDictionaries">A list of dictionaries of column/value pairs.
        ///                                   Each item in the list should represent the dictionary of non-ID column/value pairs for 
        ///                                   each respective object being updated.</param>
        public override void UpdateBatch(ITransaction transaction, ClassMapping mapping, List<DaoCriteria> criteriaList, List<IDictionary<string, object>> propValueDictionaries)
        {
            for (int x = 0; x < criteriaList.Count; x++)
            {
                Update(transaction, mapping, criteriaList[x], propValueDictionaries[x]);
            }
        }

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
        public override void ExecuteQuery(ITransaction transaction, ClassMapping mapping, IDaQuery query, DataReaderDelegate invokeMe, Hashtable parameters)
        {
            // Make a copy of the table and iterate over that, that way reading doesn't block writing (or
            // more reading).
            IDictionary<string, MemoryObject> tempTable;
            IDictionary<string, MemoryObject> table = GetTable(mapping);
            lock (table)
            {
                tempTable = new CheckedDictionary<string, MemoryObject>(table);
            }
            MemoryDataReader reader = new MemoryDataReader(this, mapping, ((UnqueryableQuery)query).Criteria,
                                                           tempTable.Values.GetEnumerator());
            try
            {
                invokeMe.Invoke(parameters, reader);
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Gets a count of records for the given criteria.
        /// </summary>
        /// <param name="transaction">The transaction to do this as part of.</param>
        /// <param name="mapping">The mapping of the table for which to build the query string.</param>
        /// <param name="crit">The criteria to use for "where" comparisons.</param>
        /// <returns>The number of results found that matched the criteria.</returns>
        public override int GetCount(ITransaction transaction, ClassMapping mapping, DaoCriteria crit)
        {
            IDictionary<string, MemoryObject> table = GetTable(mapping);
            int retVal = 0;
            lock (table)
            {
                MemoryDataReader reader = new MemoryDataReader(this, mapping, crit,
                                                               table.Values.GetEnumerator());
                while (reader.Read())
                {
                    retVal++;
                }
            }
            return retVal;
        }

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
        public override List<GroupCountResult> GetCount(ITransaction transaction, ClassMapping mapping, DaoCriteria crit, ICollection<AbstractGroupExpression> groupExpressions)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds the last generated id number for a column.
        /// </summary>
        /// <param name="transaction">The transaction to do this as part of.</param>
        /// <param name="mapping">The class mapping for the table being queried.</param>
        /// <param name="idCol">The ID column for which to find the last-generated ID.</param>
        public override object GetLastAutoGeneratedId(ITransaction transaction, ClassMapping mapping, string idCol)
        {
            int retVal = 0;
            string key = mapping.Table + "." + idCol;
            lock (_lastAutogenIds)
            {
                if (_lastAutogenIds.ContainsKey(key))
                {
                    retVal = _lastAutogenIds[key];
                }
                else
                {
                    _lastAutogenIds[key] = retVal;
                }
            }
            return retVal;
        }

        /// <summary>
        /// Finds the next generated id number for a column (for inserts for example).
        /// </summary>
        /// <param name="mapping">The class mapping for the table being queried.</param>
        /// <param name="idCol">The ID column for which to generate an ID.</param>
        private object GetNextAutoGeneratedId(ClassMapping mapping, string idCol)
        {
            int retVal = 1;
            string key = mapping.Table + "." + idCol;
            lock (_lastAutogenIds)
            {
                if (_lastAutogenIds.ContainsKey(key))
                {
                    retVal = _lastAutogenIds[key] + 1;
                }
                _lastAutogenIds[key] = retVal;
            }
            return retVal;
        }

        /// <summary>
        /// Gets the next id number from a sequence in the data source.
        /// </summary>
        /// <param name="transaction">The transaction to do this as part of.</param>
        /// <param name="sequenceName">The name of the sequence.</param>
        /// <returns>The next number from the sequence.</returns>
        public override int GetNextSequenceValue(ITransaction transaction, string sequenceName)
        {
            int retVal = 1;
            lock (_sequences)
            {
                if (_sequences.ContainsKey(sequenceName))
                {
                    retVal = _sequences[sequenceName];
                }
                _sequences[sequenceName] = retVal + 1;
            }
            return retVal;
        }

        private IDictionary<string, MemoryObject> GetTable(ClassMapping mapping)
        {
            IDictionary<string, MemoryObject> retVal;
            lock (_datastore)
            {
                if (_datastore.ContainsKey(mapping.Table))
                {
                    retVal = _datastore[mapping.Table];
                }
                else
                {
                    retVal = new CheckedDictionary<string, MemoryObject>();
                    _datastore[mapping.Table] = retVal;
                }
            }
            return retVal;
        }
    }
}
