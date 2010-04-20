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
using System.Text.RegularExpressions;
using Azavea.Open.Common;
using Azavea.Open.DAO.Criteria;
using Azavea.Open.DAO.Util;

namespace Azavea.Open.DAO.Unqueryable
{
    /// <summary>
    /// Base class for data readers for data sources with no native query
    /// capability, so instead all the filtering has to happen as you read
    /// over the data.
    /// </summary>
    public abstract class UnqueryableDataReader : CachingDataReader
    {
        /// <summary>
        /// The data access layer we're reading from.
        /// </summary>
        protected readonly UnqueryableDaLayer Layer;
        /// <summary>
        /// The criteria we're filtering by as we read the data.
        /// </summary>
        protected readonly DaoCriteria Criteria;
        /// <summary>
        /// The class mapping for the data objects we're returning.
        /// </summary>
        protected readonly ClassMapping Mapping;

        private bool _useOrderedResults;
        private List<SortableResult> _orderedResults;
        private int _orderedIndex;

        /// <summary>
        /// Create the data reader.
        /// </summary>
        /// <param name="layer">Data access layer that will give us the data we need.</param>
        /// <param name="mapping">ClassMapping for the type we're returning.</param>
        /// <param name="criteria">Since there is no way to filter before we read the file,
        ///                     the reader checks each row read to see if it matches the
        ///                     criteria, if not, it is skipped.</param>
        protected UnqueryableDataReader(UnqueryableDaLayer layer,
            ClassMapping mapping, DaoCriteria criteria)
            : base(mapping.AllDataColsInOrder.Count)
        {
            Layer = layer;
            Criteria = criteria;
            Mapping = mapping;
        }

        /// <summary>
        /// This should be called by the child constructor after _indexesByName has been set.
        /// </summary>
        protected void PreProcessSorts()
        {
            // Now, if we were asked to sort the results, we have to actually find all
            // the matching results by reading the entire data source and then sort them in
            // memory.
            if ((Criteria != null) && (Criteria.Orders.Count > 0))
            {
                IList<ColumnSortOrder> sortOrders = new List<ColumnSortOrder>();
                // Figure out the columns to sort by.
                foreach (SortOrder order in Criteria.Orders)
                {
                    bool ascending;
                    switch (order.Direction)
                    {
                        case SortType.Asc:
                            ascending = true;
                            break;
                        case SortType.Desc:
                            ascending = false;
                            break;
                        default:
                            throw new LoggingException("Unsupported sort order on criteria: " + order);
                    }
                    int colIndex = _indexesByName[Mapping.AllDataColsByObjAttrs[order.Property]];
                    sortOrders.Add(new ColumnSortOrder(colIndex, ascending));
                }
                _orderedResults = new List<SortableResult>();
                // ReSharper disable DoNotCallOverridableMethodsInConstructor
                while (FetchNextRow())
                // ReSharper restore DoNotCallOverridableMethodsInConstructor
                {
                    _orderedResults.Add(new SortableResult(_valsByIndex, sortOrders));
                }
                // Now sort them.
                _orderedResults.Sort();
                // Now tell all future calls to FetchNextRow to use those results.
                _useOrderedResults = true;
            }
        }

        /// <summary>
        /// Gets the number of rows changed, inserted, or deleted by execution of the SQL statement.
        /// </summary>
        /// <returns>
        /// The number of rows changed, inserted, or deleted; 0 if no rows were affected or the statement failed; and -1 for SELECT statements.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int RecordsAffected
        {
            // Modifying the data source is done via the data access layer, not via the data reader.
            get { return 0; }
        }

        /// <summary>
        /// This method gets the actual data value from the actual data source.
        /// </summary>
        /// <param name="i">Column number to get, zero-based.</param>
        /// <returns>A primitive, string, date, NTS geometry, or null if the column
        ///          had no value.</returns>
        protected override object GetDataObject(int i)
        {
            throw new NotImplementedException("This should never be called because the FetchNextRow should pre-populate all values. Make sure your data is returning a value for every mapped column.  You tried to get column "
                                              + i + (_namesByIndex.Length > i
                                                         ? ", which may be named " + _namesByIndex[i]
                                                         : ", but there are only " + _namesByIndex.Length + " columns mapped."));
        }

        /// <summary>
        /// Moves the cursor (or whatever the implementation equivilent is) to the next row
        /// if there is one.
        /// </summary>
        /// <returns>Whether or not there was another row to fetch.</returns>
        protected override bool FetchNextRow()
        {
            bool foundData;
            if (_useOrderedResults)
            {
                // In this case, use the already-read, sorted results instead of
                // reading directly from the CSV source.
                if (_orderedIndex < _orderedResults.Count)
                {
                    _orderedResults[_orderedIndex].CopyTo(_valsByIndex);
                    foundData = true;
                    _orderedIndex++;
                }
                else
                {
                    foundData = false;
                }
            }
            else
            {
                // Not using cached results, so read from the reader.
                do
                {
                    IList rawRow = ReadRawRow();
                    // If it was a valid row, copy it into the real data array.
                    if (rawRow == null)
                    {
                        foundData = false;
                    }
                    else
                    {
                        foundData = true;
                        int x;
                        for (x = 0; x < rawRow.Count; x++)
                        {
                            _valsByIndex[x] = rawRow[x];
                            if (_valsByIndex[x] != null)
                            {
                                // Coerce it to the appropriate type if the mapping has one.
                                string colName = _namesByIndex[x];
                                Type dataType = Mapping.DataColTypesByDataCol[colName];
                                if (dataType != null)
                                {
                                    _valsByIndex[x] = Layer.CoerceType(dataType, _valsByIndex[x]);
                                }
                            }
                        }
                        // null out any remaining values.
                        while (x < _valsByIndex.Length)
                        {
                            _valsByIndex[x++] = null;
                        }
                    }
                    // Keep going until we find a row that matches the criteria, or 
                    // we run out of rows.
                } while (foundData && (!DataMatchesCriteria(Criteria)));
            }
            return foundData;
        }

        /// <summary>
        /// Returns the "row" from the data source, which we will then determine whether it
        /// matches the criteria, needs to be sorted, etc.
        /// </summary>
        protected abstract IList ReadRawRow();

        /// <summary>
        /// Determines whether the data in the current row matches the criteria specified.
        /// </summary>
        /// <param name="criteria">Criteria that the data has to meet to be returned.</param>
        /// <returns>True if so, false otherwise.</returns>
        protected bool DataMatchesCriteria(DaoCriteria criteria)
        {
            // No criteria means every row matches.
            if (criteria == null)
            {
                return true;
            }
            if (criteria.Expressions.Count == 0)
            {
                return true;
            }
            bool allMatch = true;
            bool anyMatch = false;
            foreach (IExpression expr in criteria.Expressions)
            {
                bool matches = true;
                if (expr is EqualExpression)
                {
                    EqualExpression realExpr = (EqualExpression)expr;
                    object actualValue = GetRealValue(realExpr.Property);
                    if (realExpr.Value == null)
                    {
                        if (actualValue != null)
                        {
                            if (_log.IsDebugEnabled)
                            {
                                _log.Debug("This row is no good, " + realExpr.Property + " is not null.");
                            }
                            matches = false;
                        }
                    }
                    else
                    {
                        // TODO: This doesn't work because we read strings but the params are not strings.
                        if (!realExpr.Value.Equals(Layer.CoerceType(realExpr.Value.GetType(), actualValue)))
                        {
                            if (_log.IsDebugEnabled)
                            {
                                _log.Debug("This row is no good, " + realExpr.Property + "('" +
                                           actualValue + "')" + " != '" + realExpr.Value + "'");
                            }
                            matches = false;
                        }
                    }
                }
                else if (expr is EqualInsensitiveExpression)
                {
                    EqualInsensitiveExpression realExpr = (EqualInsensitiveExpression)expr;
                    object actualValue = GetRealValue(realExpr.Property);
                    if (realExpr.Value == null)
                    {
                        if (actualValue != null)
                        {
                            if (_log.IsDebugEnabled)
                            {
                                _log.Debug("This row is no good, " + realExpr.Property + " is not null.");
                            }
                            matches = false;
                        }
                    }
                    else
                    {
                        if (!realExpr.Value.ToString().ToUpper().Equals(actualValue.ToString().ToUpper()))
                        {
                            if (_log.IsDebugEnabled)
                            {
                                _log.Debug("This row is no good, " + realExpr.Property + "('" +
                                           actualValue + "')" + " != '" + realExpr.Value + "'");
                            }
                            matches = false;
                        }
                    }
                }
                else if (expr is BetweenExpression)
                {
                    BetweenExpression realExpr = (BetweenExpression)expr;
                    object actualValue = GetRealValue(realExpr.Property);
                    if ((realExpr.Min == null) || (realExpr.Max == null) || (actualValue == null))
                    {
                        // To be compatible with DB behavior, null is not comparable
                        // with a value and so never matches.
                        if (_log.IsDebugEnabled)
                        {
                            _log.Debug("This row is no good, " + realExpr.Property + " is null.");
                        }
                        matches = false;
                    }
                    else
                    {
                        if (!((realExpr.Min is IComparable) && (realExpr.Max is IComparable)))
                        {
                            throw new LoggingException("You asked for between non-comparable values: " + realExpr);
                        }
                        actualValue = Layer.CoerceType(realExpr.Max.GetType(), actualValue);
                        if ((((IComparable)realExpr.Max).CompareTo(actualValue) >= 0) ||
                            (((IComparable)realExpr.Min).CompareTo(actualValue) <= 0))
                        {
                            if (_log.IsDebugEnabled)
                            {
                                _log.Debug("This row is no good, " + realExpr.Property + "('" +
                                           actualValue + "')" + " is not between '" + realExpr.Min +
                                           "' and '" + realExpr.Max + "'");
                            }
                            matches = false;
                        }
                    }
                }
                else if (expr is GreaterExpression)
                {
                    GreaterExpression realExpr = (GreaterExpression)expr;
                    object actualValue = GetRealValue(realExpr.Property);
                    if ((realExpr.Value == null) || (actualValue == null))
                    {
                        // To be compatible with DB behavior, null is not comparable
                        // with a value and so never matches.
                        if (_log.IsDebugEnabled)
                        {
                            _log.Debug("This row is no good, " + realExpr.Property + " is null.");
                        }
                        matches = false;
                    }
                    else
                    {
                        if (!(realExpr.Value is IComparable))
                        {
                            throw new LoggingException("You asked for greater than a non-comparable value: " + realExpr);
                        }
                        actualValue = Layer.CoerceType(realExpr.Value.GetType(), actualValue);
                        if (((IComparable)realExpr.Value).CompareTo(actualValue) >= 0)
                        {
                            if (_log.IsDebugEnabled)
                            {
                                _log.Debug("This row is no good, " + realExpr.Property + "('" +
                                           actualValue + "')" + " <= '" + realExpr.Value + "'");
                            }
                            matches = false;
                        }
                    }
                }
                else if (expr is LesserExpression)
                {
                    LesserExpression realExpr = (LesserExpression)expr;
                    object actualValue = GetRealValue(realExpr.Property);
                    if ((realExpr.Value == null) || (actualValue == null))
                    {
                        // To be compatible with DB behavior, null is not comparable
                        // with a value and so never matches.
                        if (_log.IsDebugEnabled)
                        {
                            _log.Debug("This row is no good, " + realExpr.Property + " is null.");
                        }
                        matches = false;
                    }
                    else
                    {
                        if (!(realExpr.Value is IComparable))
                        {
                            throw new LoggingException("You asked for lesser than a non-comparable value: " + realExpr);
                        }
                        actualValue = Layer.CoerceType(realExpr.Value.GetType(), actualValue);
                        if (((IComparable)realExpr.Value).CompareTo(actualValue) <= 0)
                        {
                            if (_log.IsDebugEnabled)
                            {
                                _log.Debug("This row is no good, " + realExpr.Property + "('" +
                                           actualValue + "')" + " >= '" + realExpr.Value + "'");
                            }
                            matches = false;
                        }
                    }
                }
                else if (expr is LikeExpression)
                {
                    LikeExpression realExpr = (LikeExpression)expr;
                    object actualValue = GetRealValue(realExpr.Property);
                    if (realExpr.Value == null)
                    {
                        if (actualValue != null)
                        {
                            if (_log.IsDebugEnabled)
                            {
                                _log.Debug("This row is no good, " + realExpr.Property + " is not null.");
                            }
                            matches = false;
                        }
                    }
                    else
                    {
                        if (actualValue == null)
                        {
                            if (_log.IsDebugEnabled)
                            {
                                _log.Debug("This row is no good, " + realExpr.Property + " is null.");
                            }
                            matches = false;
                        }
                        else
                        {
                            // Escapes any regex-specific chars.
                            string regexStr = Regex.Escape(realExpr.Value.ToString());
                            // Luckily, DB 'like' wildcards (% and _) are not regex chars so we can do
                            // a simple replace.
                            regexStr = regexStr.Replace("%", ".*");
                            regexStr = regexStr.Replace("_", ".");
                            if (Regex.IsMatch(actualValue.ToString(), regexStr))
                            {
                                if (_log.IsDebugEnabled)
                                {
                                    _log.Debug("This row is no good, " + realExpr.Property + "('" +
                                               actualValue + "')" + " isn't like '" + realExpr.Value + "'");
                                }
                                matches = false;
                            }
                        }
                    }
                }
                else if (expr is LikeInsensitiveExpression)
                {
                    LikeInsensitiveExpression realExpr = (LikeInsensitiveExpression)expr;
                    object actualValue = GetRealValue(realExpr.Property);
                    if (realExpr.Value == null)
                    {
                        if (actualValue != null)
                        {
                            if (_log.IsDebugEnabled)
                            {
                                _log.Debug("This row is no good, " + realExpr.Property + " is not null.");
                            }
                            matches = false;
                        }
                    }
                    else
                    {
                        if (actualValue == null)
                        {
                            if (_log.IsDebugEnabled)
                            {
                                _log.Debug("This row is no good, " + realExpr.Property + " is null.");
                            }
                            matches = false;
                        }
                        else
                        {
                            // Escapes any regex-specific chars.
                            string regexStr = Regex.Escape(realExpr.Value.ToString());
                            // Luckily, DB 'like' wildcards (% and _) are not regex chars so we can do
                            // a simple replace.
                            regexStr = regexStr.Replace("%", ".*");
                            regexStr = regexStr.Replace("_", ".");
                            if (Regex.IsMatch(actualValue.ToString().ToUpper(), regexStr.ToUpper()))
                            {
                                if (_log.IsDebugEnabled)
                                {
                                    _log.Debug("This row is no good, " + realExpr.Property + "('" +
                                               actualValue + "')" + " isn't like '" + realExpr.Value + "'");
                                }
                                matches = false;
                            }
                        }
                    }
                }
                else if (expr is PropertyInListExpression)
                {
                    PropertyInListExpression realExpr = (PropertyInListExpression)expr;
                    object actualValue = GetRealValue(realExpr.Property);
                    matches = false;
                    foreach (object val in realExpr.Values)
                    {
                        if (val == null)
                        {
                            if (actualValue == null)
                            {
                                matches = true;
                                break;
                            }
                        }
                        else
                        {
                            if (actualValue == null)
                            {
                                break;
                            }
                            actualValue = Layer.CoerceType(val.GetType(), actualValue);
                            if (val.Equals(actualValue))
                            {
                                matches = true;
                                break;
                            }
                        }
                    }

                    if (!matches)
                    {
                        if (_log.IsDebugEnabled)
                        {
                            _log.Debug("This row is no good, " + realExpr.Property + " is not in this list: " +
                                       StringHelper.Join(realExpr.Values));
                        }
                    }
                }
                else if (expr is CriteriaExpression)
                {
                    matches = DataMatchesCriteria(((CriteriaExpression)expr).NestedCriteria);
                }
                else
                {
                    throw new LoggingException("Unsupported expression in query: " + expr);
                }

                // Invert it if it's a NOT
                if (!expr.TrueOrNot())
                {
                    matches = !matches;
                }
                allMatch = allMatch && matches;
                anyMatch = anyMatch || matches;
                if (!allMatch && (criteria.BoolType == BooleanOperator.And))
                {
                    return false;
                }
                if (anyMatch && (criteria.BoolType == BooleanOperator.Or))
                {
                    return true;
                }
            }
            // We only hit this if the criteria was of type 'or'.
            return anyMatch;
        }

        private object GetRealValue(string objPropertyName)
        {
            return _valsByIndex[_indexesByName[Mapping.AllDataColsByObjAttrs[objPropertyName]]];
        }

        /// <summary>
        /// Used to keep track of the column indexes and which directions we're sorting them.
        /// </summary>
        public class ColumnSortOrder
        {
            /// <summary>
            /// The numeric (0-based) column index in the value array.
            /// </summary>
            public int ColIndex;
            /// <summary>
            /// True for ascending sort, false for descending.
            /// </summary>
            public bool Ascending;
            /// <summary>
            /// Create the sort order from the two params it needs.
            /// </summary>
            /// <param name="index"></param>
            /// <param name="ascending"></param>
            public ColumnSortOrder(int index, bool ascending)
            {
                ColIndex = index;
                Ascending = ascending;
            }
        }

        /// <summary>
        /// Used for holding the results when we need to sort them.
        /// </summary>
        public class SortableResult : IComparable<SortableResult>
        {
            private readonly object[] _values;
            private readonly IList<ColumnSortOrder> _sortOrders;

            /// <summary>
            /// Creates a sortable result from the real value array.
            /// </summary>
            /// <param name="values"></param>
            /// <param name="sortOrders"></param>
            public SortableResult(object[] values, IList<ColumnSortOrder> sortOrders)
            {
                _sortOrders = sortOrders;
                _values = new object[values.Length];
                Array.Copy(values, _values, values.Length);
            }
            /// <summary>
            /// Copies the result values back into the real array.
            /// </summary>
            /// <param name="dest"></param>
            public void CopyTo(object[] dest)
            {
                Array.Copy(_values, dest, _values.Length);
            }

            /// <summary>
            ///                     Compares the current object with another object of the same type.
            /// </summary>
            /// <returns>
            ///                     A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: 
            ///                     Value 
            ///                     Meaning 
            ///                     Less than zero 
            ///                     This object is less than the <paramref name="other" /> parameter.
            ///                     Zero 
            ///                     This object is equal to <paramref name="other" />. 
            ///                     Greater than zero 
            ///                     This object is greater than <paramref name="other" />. 
            /// </returns>
            /// <param name="other">
            ///                     An object to compare with this object.
            ///                 </param>
            public int CompareTo(SortableResult other)
            {
                foreach (ColumnSortOrder order in _sortOrders)
                {
                    int valCompare = ((IComparable)_values[order.ColIndex]).CompareTo(
                        other._values[order.ColIndex]);
                    if (valCompare != 0)
                    {
                        return order.Ascending ? valCompare : (valCompare * -1);
                    }
                }
                return 0;
            }
        }
    }
}
