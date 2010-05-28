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
using System.Data;
using System.Reflection;
using System.Text;
using Azavea.Open.Common.Caching;
using Azavea.Open.Common.Collections;
using Azavea.Open.DAO.Criteria;
using Azavea.Open.DAO.Criteria.Grouping;
using Azavea.Open.DAO.Exceptions;
using Azavea.Open.DAO.Util;

namespace Azavea.Open.DAO.SQL
{
    /// <summary>
    /// A SQL-specific implementation of an IDaLayer.  
    /// Provided functionality to run data access function that are specific to data sources that take sql commands.
    /// </summary>
    public class SqlDaLayer : AbstractDaLayer
    {
        /// <summary>
        /// From this class on down, we can always treat it as a SqlConnectionDescriptor.
        /// </summary>
        protected new AbstractSqlConnectionDescriptor _connDesc;
        /// <summary>
        /// Since there are different types of query for different dao layers,
        /// each keeps their own cache.
        /// </summary>
        protected static readonly ClearingCache<SqlDaQuery> _sqlQueryCache = new ClearingCache<SqlDaQuery>();

        /// <summary>
        /// Tells us whether or not to fully qualify column names ("Table.Column") in the SQL.
        /// This is generally harmless for actual databases, but may not always be desirable.
        /// </summary>
        protected Boolean _fullyQualifyColumnNames = true;

        /// <summary>
        /// Sometimes it is not desirable to fully qualify column names, even for a database,
        /// e.g. when doing a query that is a join of foreign tables.  This allows this value
        /// to be set in those instances.
        /// </summary>
        /// <param name="fullyQualifyColumnNames">Whether or not to fully qualify column names.</param>
        public void SetFullyQualifyColumnNames(bool fullyQualifyColumnNames)
        {
            _fullyQualifyColumnNames = fullyQualifyColumnNames;
        }

        /// <summary>
        /// Instantiates the data access layer with the connection descriptor for the DB.
        /// </summary>
        /// <param name="connDesc">The connection descriptor that is being used by this FastDaoLayer.</param>
        /// <param name="supportsNumRecords">If true, methods that return numbers of records affected will be
        ///                                 returning accurate numbers.  If false, they will probably return
        ///                                 FastDAO.UNKNOWN_NUM_ROWS.</param>
        public SqlDaLayer(AbstractSqlConnectionDescriptor connDesc, bool supportsNumRecords)
            : base(connDesc, supportsNumRecords)
        {
            // We have a new _connDesc attribute, so make sure we set it here.
            _connDesc = connDesc;
        }

        #region Members for children to override

        /// <summary>
        /// Override this method if you need to do any work to convert values from the
        /// object's properties into normal SQL parameters.  Default implementation
        /// does nothing.
        /// 
        /// This is called prior to inserting or updating these values in the table.
        /// </summary>
        /// <param name="table">The table these values will be inserted or updated into.</param>
        /// <param name="propValues">A dictionary of "column"/value pairs for the object to insert or update.</param>
        protected virtual void PreProcessPropertyValues(string table,
                                                        IDictionary<string, object> propValues) { }

        #endregion

        #region IFastDaoLayer Members

        /// <exclude/>
        public override int Insert(ITransaction transaction, ClassMapping mapping, IDictionary<string, object> propValues)
        {
            List<object> sqlParams = DbCaches.ObjectLists.Get();

            PreProcessPropertyValues(mapping.Table, propValues);
            string sql = SqlUtilities.MakeInsertStatement(mapping.Table, propValues, sqlParams);
            int numRecs = SqlConnectionUtilities.XSafeCommand(_connDesc, (SqlTransaction)transaction, sql, sqlParams);

            if (numRecs != 1)
            {
                throw new Exception("Should have inserted one record, but sql command returned " +
                                    numRecs + " rows affected. SQL: " + SqlUtilities.SqlParamsToString(sql, sqlParams));
            }

            DbCaches.ObjectLists.Return(sqlParams);

            return numRecs;
        }

        /// <exclude/>
        public override int Update(ITransaction transaction, ClassMapping mapping, DaoCriteria crit, IDictionary<string, object> propValues)
        {
            PreProcessPropertyValues(mapping.Table, propValues);

            SqlDaQuery query = _sqlQueryCache.Get();
            try
            {
                // Make the "update table set blah = something" part.
                query.Sql.Append("UPDATE ");
                query.Sql.Append(mapping.Table);
                query.Sql.Append(" SET ");
                bool first = true;
                foreach (string key in propValues.Keys)
                {
                    if (!first)
                    {
                        query.Sql.Append(", ");
                    }
                    else
                    {
                        first = false;
                    }
                    query.Sql.Append(key);
                    query.Sql.Append(" = ?");
                    query.Params.Add(propValues[key]);
                }
                // Add the "where blah, blah, blah" part.
                ExpressionsToQuery(query, crit, mapping);

                return SqlConnectionUtilities.XSafeCommand(_connDesc, (SqlTransaction)transaction, query.Sql.ToString(), query.Params);
            }
            finally
            {
                _sqlQueryCache.Return(query);
            }
        }

        /// <exclude/>
        public override int Delete(ITransaction transaction, ClassMapping mapping, DaoCriteria crit)
        {
            SqlDaQuery query = _sqlQueryCache.Get();

            query.Sql.Append("DELETE FROM ");
            query.Sql.Append(mapping.Table);
            ExpressionsToQuery(query, crit, mapping);

            int retVal = SqlConnectionUtilities.XSafeCommand(_connDesc, (SqlTransaction)transaction, query.Sql.ToString(), query.Params);

            DisposeOfQuery(query);
            return retVal;
        }

        /// <exclude/>
        public override int GetCount(ITransaction transaction, ClassMapping mapping, DaoCriteria crit)
        {
            SqlDaQuery query = _sqlQueryCache.Get();

            query.Sql.Append("SELECT COUNT(*) FROM ");
            query.Sql.Append(mapping.Table);
            ExpressionsToQuery(query, crit, mapping);

            int retVal = SqlConnectionUtilities.XSafeIntQuery(_connDesc, (SqlTransaction)transaction, query.Sql.ToString(), query.Params);

            DisposeOfQuery(query);
            return retVal;
        }

        /// <exclude/>
        public override List<GroupCountResult> GetCount(ITransaction transaction,
            ClassMapping mapping, DaoCriteria crit,
            ICollection<AbstractGroupExpression> groupExpressions)
        {
            SqlDaQuery query = _sqlQueryCache.Get();

            query.Sql.Append("SELECT COUNT(*) ");
            if (_connDesc.NeedAsForColumnAliases())
            {
                query.Sql.Append("AS ");
            }
            query.Sql.Append(_connDesc.ColumnAliasPrefix()).
                Append("gb_count").Append(_connDesc.ColumnAliasSuffix());
            GroupBysToStartOfQuery(query, groupExpressions, mapping);
            query.Sql.Append(" FROM ");
            query.Sql.Append(mapping.Table);
            ExpressionsToQuery(query, crit, mapping);
            GroupBysToEndOfQuery(query, groupExpressions, mapping);
            OrdersToQuery(query, crit, mapping);
            Hashtable parameters = DbCaches.Hashtables.Get();

            parameters["groupBys"] = groupExpressions;
            parameters["mapping"] = mapping;
Console.WriteLine(query.Sql);
            SqlConnectionUtilities.XSafeQuery(_connDesc, (SqlTransaction)transaction,
                query.Sql.ToString(), query.Params, ReadGroupByCount, parameters);
            List<GroupCountResult> retVal = (List<GroupCountResult>) parameters["results"];

            DisposeOfQuery(query);
            DbCaches.Hashtables.Return(parameters);
            return retVal;
        }

        /// <summary>
        /// Reads the results from the data reader produced by the group by
        /// query, creates the GroupCountResults, and returns them in the 
        /// parameters collection.
        /// </summary>
        /// <param name="parameters">Input and output parameters for the method.</param>
        /// <param name="reader">Data reader to read from.</param>
        protected virtual void ReadGroupByCount(Hashtable parameters, IDataReader reader)
        {
            ICollection<AbstractGroupExpression> groupExpressions =
                (ICollection<AbstractGroupExpression>) parameters["groupBys"];
            ClassMapping mapping = (ClassMapping) parameters["mapping"];
            List<GroupCountResult> results = new List<GroupCountResult>();
            parameters["results"] = results;

            // Read each row and store it as a GroupCountResult.
            while (reader.Read())
            {
                // We aliased the count as "gb_count".
                // Some databases return other numeric types, like "decimal", so you can't
                // just call reader.GetInt.
                string colName = "gb_count";
                if (_connDesc.ColumnAliasWrappersInResults())
                {
                    colName = _connDesc.ColumnAliasPrefix() + colName + _connDesc.ColumnAliasSuffix();
                }
                int count = (int)CoerceType(typeof(int), reader.GetValue(reader.GetOrdinal(colName)));
                IDictionary<string, object> values = new CheckedDictionary<string, object>();
                int groupByNum = 0;
                foreach (AbstractGroupExpression expr in groupExpressions)
                {
                    values[expr.Name] = GetGroupByValue(mapping, reader, groupByNum, expr);
                    groupByNum++;
                }
                results.Add(new GroupCountResult(count, values));
            }
        }

        /// <summary>
        /// Reads a single "group by" field value from the data reader, coerces it
        /// to the correct type if necessary/possible, and returns it.
        /// </summary>
        /// <param name="mapping">Mapping of class fields / names / etc.</param>
        /// <param name="reader">Data reader to get the value from.</param>
        /// <param name="number">Which group by field is this (0th, 1st, etc).</param>
        /// <param name="expression">The group by expression we're reading the value for.</param>
        /// <returns>The value to put in the GroupValues collection of the GroupCountResult.</returns>
        protected virtual object GetGroupByValue(ClassMapping mapping, IDataReader reader,
            int number, AbstractGroupExpression expression)
        {
            // We aliased the group bys in order as "gb_0", "gb_1", etc.
            string colName = "gb_" + number;
            if (_connDesc.ColumnAliasWrappersInResults())
            {
                colName = _connDesc.ColumnAliasPrefix() + colName + _connDesc.ColumnAliasSuffix();
            }
            int colNum = reader.GetOrdinal(colName);
            object value = reader.IsDBNull(colNum) ? null : reader.GetValue(colNum);
            if (expression is MemberGroupExpression)
            {
                // For group bys, we leave nulls as nulls even if the type would
                // normally be non-nullable (I.E. int, float, etc).
                if (value != null)
                {
                    string memberName = ((MemberGroupExpression) expression).MemberName;
                    Type desiredType = null;
                    // If we actually have a member info, coerce the value to that type.
                    if (mapping.AllObjMemberInfosByObjAttr.ContainsKey(memberName))
                    {
                        MemberInfo info = mapping.AllObjMemberInfosByObjAttr[memberName];
                        // Don't call MemberType getter twice
                        MemberTypes type = info.MemberType;
                        if (type == MemberTypes.Field)
                        {
                            FieldInfo fInfo = ((FieldInfo) info);
                            desiredType = fInfo.FieldType;
                        }
                        else if (type == MemberTypes.Property)
                        {
                            PropertyInfo pInfo = ((PropertyInfo) info);
                            desiredType = pInfo.PropertyType;
                        }
                    }
                    else if (mapping.DataColTypesByObjAttr.ContainsKey(memberName))
                    {
                        // We don't have a memberinfo, but we do have a type in the mapping,
                        // so coerce to that type.
                        desiredType = mapping.DataColTypesByObjAttr[memberName];
                    }
                    // If we have a type to coerce it to, coerce it, otherwise return as-is.
                    if (desiredType != null)
                    {
                        value = CoerceType(desiredType, value);
                    }
                }
            }
            else
            {
                throw new ArgumentException(
                    "Group expression '" + expression + "' is an unsupported type.",
                    "expression");
            }
            return value;
        }

        /// <summary>
        /// Builds the query based on a serializable criteria.
        /// </summary>
        /// <param name="crit">The criteria to use for "where" comparisons.</param>
        /// <param name="mapping">The mapping of the table for which to build the query string.</param>
        /// <returns>A query that can be run by ExecureQuery.</returns>
        public override IDaQuery CreateQuery(ClassMapping mapping, DaoCriteria crit)
        {
            SqlDaQuery retVal = _sqlQueryCache.Get();

            retVal.Sql.Append("SELECT * FROM ");
            retVal.Sql.Append(mapping.Table);
            ExpressionsToQuery(retVal, crit, mapping);
            OrdersToQuery(retVal, crit, mapping);

            // Don't return the objects, we'll do that in DisposeOfQuery.
            return retVal;
        }

        /// <summary>
        /// Builds the query based on a string, such as a sql string.
        /// NOTE: Not all FastDaoLayers are required to support this, if it is not
        /// supported a NotSupportedException will be thrown.
        /// TODO: This will be removed when FastDAO.QueryFor and IterateOverObjects
        /// that take strings are removed.
        /// </summary>
        /// <param name="queryStr">The sql statement to execute that is expected to return a large
        ///                      number of rows.</param>
        /// <param name="queryParams">The parameters for the sql statement.  If there are none, this
        ///                            can be null.</param>
        /// <param name="mapping">The mapping of the table for which to build the query string.</param>
        /// <returns>A query that can be run by ExecureQuery.</returns>
        public virtual IDaQuery CreateQuery(string queryStr, IEnumerable queryParams, ClassMapping mapping)
        {
            SqlDaQuery retVal = _sqlQueryCache.Get();
            retVal.Sql.Append(queryStr);
            if (queryParams != null)
            {
                foreach (object param in queryParams)
                {
                    retVal.Params.Add(param);
                }
            }
            // Don't return the param list or the query, we'll do that in DisposeOfQuery.
            return retVal;
        }

        /// <summary>
        /// Should be called when you're done with the query.  Allows us to cache the
        /// objects for reuse.
        /// </summary>
        /// <param name="query">Query you're done using.</param>
        public override void DisposeOfQuery(IDaQuery query)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query", "Cannot dispose of null query.");
            }
            if (!(query is SqlDaQuery))
            {
                throw new ArgumentException("Cannot dispose of a query not created by me.");
            }
            SqlDaQuery sqlQuery = (SqlDaQuery) query;
            _sqlQueryCache.Return(sqlQuery);
        }

        /// <summary>
        /// Adds the group by fields to the "column" list ("column"
        /// since they may not all be columns) in the beginning of the
        /// select (I.E. "SELECT COUNT(*), Field1, Field2, etc).
        /// </summary>
        /// <param name="query">Query to append to.</param>
        /// <param name="groupExpressions">Group by expressions.</param>
        /// <param name="mapping">Class mapping for the class we're dealing with.</param>
        public virtual void GroupBysToStartOfQuery(SqlDaQuery query,
            ICollection<AbstractGroupExpression> groupExpressions, ClassMapping mapping)
        {
            int exprNum = 0;
            foreach (AbstractGroupExpression expression in groupExpressions)
            {
                query.Sql.Append(", ");
                if (expression is MemberGroupExpression)
                {
                    query.Sql.Append(
                        mapping.AllDataColsByObjAttrs[((MemberGroupExpression)expression).MemberName]);
                    query.Sql.Append(_connDesc.NeedAsForColumnAliases() ? " AS " : " ")
                        .Append(_connDesc.ColumnAliasPrefix())
                        .Append("gb_").Append(exprNum).Append(_connDesc.ColumnAliasSuffix());
                }
                else
                {
                    throw new ArgumentException(
                        "Group expression '" + expression + "' is an unsupported type.",
                        "groupExpressions");
                }
                exprNum++;
            }
        }
        /// <summary>
        /// Adds the group by fields to the end of the query, including
        /// the keyword "GROUP BY" if necessary.
        /// </summary>
        /// <param name="query">Query to append to.</param>
        /// <param name="groupExpressions">Group by expressions.</param>
        /// <param name="mapping">Class mapping for the class we're dealing with.</param>
        public virtual void GroupBysToEndOfQuery(SqlDaQuery query,
            ICollection<AbstractGroupExpression> groupExpressions, ClassMapping mapping)
        {
            if (groupExpressions.Count > 0)
            {
                query.Sql.Append(" GROUP BY ");
            }
            bool first = true;
            foreach (AbstractGroupExpression expression in groupExpressions)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    query.Sql.Append(", ");
                }
                if (expression is MemberGroupExpression)
                {
                    query.Sql.Append(
                        mapping.AllDataColsByObjAttrs[((MemberGroupExpression) expression).MemberName]);
                }
                else
                {
                    throw new ArgumentException(
                        "Group expression '" + expression + "' is an unsupported type.",
                        "groupExpressions");
                }
            }
        }

        /// <summary>
        /// Takes a DaoCriteria, converts it to a " WHERE ..." chunk of SQL.
        /// The SQL will begin with a space if non-empty.
        /// </summary>
        /// <param name="queryToAddTo">Query we're adding the expression to.</param>
        /// <param name="crit">Serializable critera to get the expressions from.</param>
        /// <param name="mapping">Class mapping for the class we're dealing with.</param>
        public virtual void ExpressionsToQuery(SqlDaQuery queryToAddTo, DaoCriteria crit,
                                               ClassMapping mapping)
        {
            if (crit != null)
            {
                if (crit.Expressions.Count > 0)
                {
                    string colPrefix = _fullyQualifyColumnNames ? mapping.Table + "." : null;
                    queryToAddTo.Sql.Append(" WHERE ");
                    ExpressionListToQuery(queryToAddTo, crit.BoolType, crit.Expressions,
                                          mapping, colPrefix);
                }
            }
        }

        /// <summary>
        /// Returns a nicely spaced AND or OR depending on the boolean type.
        /// </summary>
        /// <param name="boolType"></param>
        /// <returns></returns>
        protected static string BoolTypeToString(BooleanOperator boolType)
        {
            switch (boolType)
            {
                case BooleanOperator.And:
                    return " AND ";
                case BooleanOperator.Or:
                    return " OR ";
                default:
                    throw new NotSupportedException("Type of expression condition '" +
                                                    boolType + " not supported.");
            }
        }

        /// <summary>
        /// Converts the list of expressions from this criteria into SQL, and appends to the 
        /// given string builder.
        /// </summary>
        /// <param name="queryToAddTo">Query we're adding the expression to.</param>
        /// <param name="boolType">Whether to AND or OR the expressions together.</param>
        /// <param name="expressions">The expressions to add to the query.</param>
        /// <param name="mapping">Class mapping for the class we're dealing with.</param>
        /// <param name="colPrefix">What to prefix column names with, I.E. "Table." for "Table.Column".
        ///                         May be null if no prefix is desired.  May be something other than
        ///                         the table name if the tables are being aliased.</param>
        protected void ExpressionListToQuery(SqlDaQuery queryToAddTo,
                                             BooleanOperator boolType,
                                             IEnumerable<IExpression> expressions,
                                             ClassMapping mapping, string colPrefix)
        {
            // starts out false for the first one.
            bool needsBooleanOperator = false;
            string boolText = BoolTypeToString(boolType);
            foreach (IExpression expr in expressions)
            {
                try
                {
                    if (expr == null)
                    {
                        throw new NullReferenceException("Can't convert a null expression to SQL.");
                    }
                    // After the first guy writes something, we need an operator.
                    if (ExpressionToQuery(queryToAddTo, expr, mapping, colPrefix,
                                          needsBooleanOperator ? boolText : ""))
                    {
                        needsBooleanOperator = true;
                    }
                }
                catch (Exception e)
                {
                    throw new UnableToConstructSqlException("Unable to add expression to query: " + expr, _connDesc, e);
                }
            }
        }

        /// <summary>
        /// Converts a single Expression to SQL (mapping the columns as appropriate) and appends
        /// to the given string builder.
        /// 
        /// Remember to wrap the SQL in parends if necessary.
        /// </summary>
        /// <param name="queryToAddTo">Query we're adding the expression to.</param>
        /// <param name="expr">The expression.  NOTE: It should NOT be null. This method does not check.</param>
        /// <param name="mapping">Class mapping for the class we're dealing with.</param>
        /// <param name="colPrefix">What to prefix column names with, I.E. "Table." for "Table.Column".
        ///                         May be null if no prefix is desired.  May be something other than
        ///                         the table name if the tables are being aliased.</param>
        /// <param name="booleanOperator">The boolean operator (AND or OR) to insert before
        ///                               this expression.  Blank ("") if we don't need one.</param>
        /// <returns>Whether or not this expression modified the sql string.
        ///          Typically true, but may be false for special query types 
        ///          that use other parameters for certain types of expressions.</returns>
        protected virtual bool ExpressionToQuery(SqlDaQuery queryToAddTo, IExpression expr,
                                                 ClassMapping mapping, string colPrefix, string booleanOperator)
        {
            string col;
            Type dbDataType;
            bool trueOrNot = expr.TrueOrNot();

            // add the operator if one was specified.
            queryToAddTo.Sql.Append(booleanOperator);
            // Add some parends.
            queryToAddTo.Sql.Append("(");
            if (expr is BetweenExpression)
            {
                BetweenExpression between = (BetweenExpression)expr;
                col = colPrefix + mapping.AllDataColsByObjAttrs[between.Property];
                dbDataType = mapping.DataColTypesByObjAttr[between.Property];
                queryToAddTo.Sql.Append(col);
                queryToAddTo.Sql.Append(trueOrNot ? " >= " : " < ");
                AppendParameter(queryToAddTo, between.Min, dbDataType);
                queryToAddTo.Sql.Append(trueOrNot ? " AND " : " OR ");
                queryToAddTo.Sql.Append(col);
                queryToAddTo.Sql.Append(trueOrNot ? " <= " : " > ");
                AppendParameter(queryToAddTo, between.Max, dbDataType);
            }
            else if (expr is EqualExpression)
            {
                EqualExpression equal = (EqualExpression)expr;
                col = colPrefix + mapping.AllDataColsByObjAttrs[equal.Property];
                if (equal.Value == null)
                {
                    queryToAddTo.Sql.Append(col);
                    queryToAddTo.Sql.Append(trueOrNot ? " IS NULL" : " IS NOT NULL");
                }
                else
                {
                    queryToAddTo.Sql.Append(col);
                    queryToAddTo.Sql.Append(trueOrNot ? " = " : " <> ");
                    dbDataType = mapping.DataColTypesByObjAttr[equal.Property];
                    AppendParameter(queryToAddTo, equal.Value, dbDataType);
                }
            }
            else if (expr is EqualInsensitiveExpression)
            {
                EqualInsensitiveExpression iequal = (EqualInsensitiveExpression)expr;
                col = colPrefix + mapping.AllDataColsByObjAttrs[iequal.Property];
                if (iequal.Value == null)
                {
                    queryToAddTo.Sql.Append(col);
                    queryToAddTo.Sql.Append(trueOrNot ? " IS NULL" : " IS NOT NULL");
                }
                else
                {
                    string lower = _connDesc.LowerCaseFunction();
                    queryToAddTo.Sql.Append(lower).Append("(");
                    queryToAddTo.Sql.Append(col).Append(") ");
                    queryToAddTo.Sql.Append(trueOrNot ? "= " : "<> ").Append(lower).Append("(");
                    dbDataType = mapping.DataColTypesByObjAttr[iequal.Property];
                    AppendParameter(queryToAddTo, iequal.Value, dbDataType);
                    queryToAddTo.Sql.Append(")");
                }
            }
            else if (expr is GreaterExpression)
            {
                GreaterExpression greater = (GreaterExpression)expr;
                queryToAddTo.Sql.Append(colPrefix);
                queryToAddTo.Sql.Append(mapping.AllDataColsByObjAttrs[greater.Property]);
                queryToAddTo.Sql.Append(trueOrNot ? " > " : " <= ");
                dbDataType = mapping.DataColTypesByObjAttr[greater.Property];
                AppendParameter(queryToAddTo, greater.Value, dbDataType);
            }
            else if (expr is LesserExpression)
            {
                LesserExpression lesser = (LesserExpression)expr;
                queryToAddTo.Sql.Append(colPrefix);
                queryToAddTo.Sql.Append(mapping.AllDataColsByObjAttrs[lesser.Property]);
                queryToAddTo.Sql.Append(trueOrNot ? " < " : " >= ");
                dbDataType = mapping.DataColTypesByObjAttr[lesser.Property];
                AppendParameter(queryToAddTo, lesser.Value, dbDataType);
            }
            else if (expr is BitwiseAndExpression)
            {
                BitwiseAndExpression bitwise = (BitwiseAndExpression)expr;
                string colName = colPrefix + mapping.AllDataColsByObjAttrs[bitwise.Property];
                SqlClauseWithValue clause = _connDesc.MakeBitwiseAndClause(colName);

                queryToAddTo.Sql.Append(clause.PartBeforeValue);
                dbDataType = mapping.DataColTypesByObjAttr[bitwise.Property];
                AppendParameter(queryToAddTo, bitwise.Value, dbDataType);
                queryToAddTo.Sql.Append(clause.PartAfterValue);
                DbCaches.Clauses.Return(clause);

                queryToAddTo.Sql.Append(trueOrNot ? " = " : ") <> ");
                AppendParameter(queryToAddTo, bitwise.Value, dbDataType);
            }
            else if (expr is LikeExpression)
            {
                LikeExpression like = (LikeExpression)expr;
                queryToAddTo.Sql.Append(colPrefix);
                queryToAddTo.Sql.Append(mapping.AllDataColsByObjAttrs[like.Property]);
                queryToAddTo.Sql.Append(trueOrNot ? " LIKE " : " NOT LIKE ");
                dbDataType = mapping.DataColTypesByObjAttr[like.Property];
                AppendParameter(queryToAddTo, like.Value, dbDataType);
            }
            else if (expr is LikeInsensitiveExpression)
            {
                LikeInsensitiveExpression ilike = (LikeInsensitiveExpression)expr;
                queryToAddTo.Sql.Append(colPrefix);
                queryToAddTo.Sql.Append(mapping.AllDataColsByObjAttrs[ilike.Property]);
                queryToAddTo.Sql.Append(trueOrNot ? " ILIKE " : " NOT ILIKE ");
                dbDataType = mapping.DataColTypesByObjAttr[ilike.Property];
                AppendParameter(queryToAddTo, ilike.Value, dbDataType);
            }
            else if (expr is PropertyInListExpression)
            {
                PropertyInListExpression inList = (PropertyInListExpression)expr;
                IEnumerable listVals = inList.Values;
                queryToAddTo.Sql.Append(colPrefix);
                queryToAddTo.Sql.Append(mapping.AllDataColsByObjAttrs[inList.Property]);
                dbDataType = mapping.DataColTypesByObjAttr[inList.Property];
                queryToAddTo.Sql.Append(trueOrNot ? " IN (" : " NOT IN (");
                bool firstIn = true;
                foreach (object val in listVals)
                {
                    if (val == null)
                    {
                        throw new NullReferenceException(
                            "Cannot include a null value in a list of possible values for " +
                            inList.Property + ".");
                    }
                    if (firstIn)
                    {
                        firstIn = false;
                    }
                    else
                    {
                        queryToAddTo.Sql.Append(", ");
                    }
                    AppendParameter(queryToAddTo, val, dbDataType);
                }
                if (firstIn)
                {
                    throw new ArgumentException("Cannot query for " + inList.Property +
                                                " values in an empty list.");
                }
                queryToAddTo.Sql.Append(")");
            }
            else if (expr is CriteriaExpression)
            {
                CriteriaExpression critExp = (CriteriaExpression)expr;
                queryToAddTo.Sql.Append(trueOrNot ? "(" : " NOT (");
                ExpressionListToQuery(queryToAddTo, critExp.NestedCriteria.BoolType,
                                      critExp.NestedCriteria.Expressions, mapping, colPrefix);
                queryToAddTo.Sql.Append(")");
            }
            else if (expr is HandWrittenExpression)
            {
                if (!trueOrNot)
                {
                    throw new ArgumentException("You'll have to manually NOT your custom SQL.");
                }
                HandWrittenExpression hand = (HandWrittenExpression)expr;
                // We'll assume it's SQL, hopefully parameterized.
                queryToAddTo.Sql.Append(hand.Expression);
                // If there are any parameters, add 'em.
                if (hand.Parameters != null)
                {
                    foreach (object aParam in hand.Parameters)
                    {
                        queryToAddTo.Params.Add(aParam);
                    }
                }
            }
            else
            {
                throw new NotSupportedException("Expression type '" + expr.GetType() + "' is not supported.");
            }
            // Remember to close the parend.
            queryToAddTo.Sql.Append(")");
            return true;
        }

        /// <summary>
        /// Takes a DaoCriteria, converts it to an " ORDER BY ..." chunk of SQL.
        /// The SQL will begin with a space if non-empty.
        /// </summary>
        public virtual void OrdersToQuery(SqlDaQuery queryToAddTo, DaoCriteria crit,
                                          ClassMapping mapping)
        {
            if (crit != null)
            {
                if (crit.Orders.Count > 0)
                {
                    queryToAddTo.Sql.Append(" ORDER BY ");
                    OrderListToSql(queryToAddTo.Sql, crit, mapping);
                }
            }
        }
        /// <summary>
        /// Converts the list of SortOrders from this criteria into SQL, and appends to the
        /// given string builder.
        /// </summary>
        protected void OrderListToSql(StringBuilder orderClauseToAddTo, DaoCriteria crit,
                                      ClassMapping mapping)
        {
            bool first = true;
            foreach (SortOrder order in crit.Orders)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    orderClauseToAddTo.Append(", ");
                }
                switch (order.Direction)
                {
                    case SortType.Asc:
                        orderClauseToAddTo.Append(order is GroupCountSortOrder
                            ? _connDesc.ColumnAliasPrefix() + "gb_count" + _connDesc.ColumnAliasSuffix()
                            : mapping.AllDataColsByObjAttrs[order.Property]);
                        orderClauseToAddTo.Append(" ASC");
                        break;
                    case SortType.Desc:
                        orderClauseToAddTo.Append(order is GroupCountSortOrder
                            ? "gb_count"
                            : mapping.AllDataColsByObjAttrs[order.Property]);
                        orderClauseToAddTo.Append(" DESC");
                        break;
                    case SortType.Computed:
                        orderClauseToAddTo.Append(order.Property);
                        break;
                    default:
                        throw new NotSupportedException("Sort type '" + order.Direction + "' not supported.");
                }
            }
        }


        /// <exclude/>
        public override void ExecuteQuery(ITransaction transaction, ClassMapping mapping, IDaQuery query,
                                          DataReaderDelegate invokeMe, Hashtable parameters)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query", "Cannot execute a null query.");
            }
            if (!(query is SqlDaQuery))
            {
                throw new ArgumentException("Cannot execute a query not created by me.");
            }
            SqlDaQuery sqlQuery = (SqlDaQuery)query;
            SqlConnectionUtilities.XSafeQuery(_connDesc, (SqlTransaction)transaction, sqlQuery.Sql.ToString(), sqlQuery.Params, invokeMe, parameters);
        }

        /// <exclude/>
        public override void Truncate(ClassMapping classMap)
        {
            SqlConnectionUtilities.TruncateTable(_connDesc, classMap.Table);
        }

        /// <summary>
        /// Since it is implementation-dependent whether to use the sql parameters collection
        /// or not, this method should be implemented in each implementation.
        /// </summary>
        /// <param name="queryToAddTo">Query to add the parameter to.</param>
        /// <param name="columnType">Type of data actually stored in the DB column.  For example,
        ///                          Enums may be stored as strings.  May be null if no type cast
        ///                          is necessary.</param>
        /// <param name="value">Actual value that we need to append to our SQL.</param>
        public virtual void AppendParameter(SqlDaQuery queryToAddTo, object value,
                                            Type columnType)
        {
            queryToAddTo.Sql.Append("?");
            if (columnType != null)
            {
                value = CoerceType(columnType, value);
            }
            queryToAddTo.Params.Add(value);
        }

        /// <exclude/>
        public override object GetLastAutoGeneratedId(ITransaction transaction, ClassMapping mapping, string idCol)
        {
            string sql = _connDesc.MakeLastAutoGeneratedIdQuery(mapping.Table, idCol);
            Hashtable parameters = new Hashtable();
            SqlConnectionUtilities.XSafeQuery(_connDesc, (SqlTransaction)transaction, sql, null, ReadScalarValue, parameters);
            return parameters[0];
        }

        /// <exclude/>
        public override int GetNextSequenceValue(ITransaction transaction, string sequenceName)
        {
            return SqlConnectionUtilities.XSafeIntQuery(_connDesc, (SqlTransaction)transaction,
                                                   _connDesc.MakeSequenceValueQuery(sequenceName), null);
        }
        /// <exclude/>
        public void ReadScalarValue(Hashtable parameters, IDataReader reader)
        {
            if (reader.Read())
            {
                object o = reader[0];

                if (o is DBNull)
                {
                    parameters.Add(0, null);
                }
                else
                {
                    parameters.Add(0, o);
                }
            }
            else
            {
                parameters.Add(0, null);
            }
        }

        #endregion


    }
}