using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Avencia.Open.Common.Caching;
using Avencia.Open.DAO.Criteria;
using Avencia.Open.DAO.Exceptions;
using Avencia.Open.DAO.Util;

namespace Avencia.Open.DAO.SQL
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
        public override int Insert(ClassMapping mapping, IDictionary<string, object> propValues)
        {
            List<object> sqlParams = DbCaches.ObjectLists.Get();

            PreProcessPropertyValues(mapping.Table, propValues);
            string sql = SQLUtilities.MakeInsertStatement(mapping.Table, propValues, sqlParams);
            int numRecs = SqlConnectionUtilities.XSafeCommand(_connDesc, sql, sqlParams);

            if (numRecs != 1)
            {
                throw new Exception("Should have inserted one record, but sql command returned " +
                                    numRecs + " rows affected. SQL: " + SQLUtilities.SqlParamsToString(sql, sqlParams));
            }

            DbCaches.ObjectLists.Return(sqlParams);

            return numRecs;
        }

        /// <exclude/>
        public override void InsertBatch(ClassMapping mapping, List<IDictionary<string, object>> propValueDictionaries)
        {
            List<string> sqls = DbCaches.StringLists.Get();
            List<IEnumerable> sqlParamLists = DbCaches.EnumerableLists.Get();

            foreach(Dictionary<string, object> propValues in propValueDictionaries)
            {
                List<object> sqlParams = DbCaches.ObjectLists.Get();

                PreProcessPropertyValues(mapping.Table, propValues);
                string sql = SQLUtilities.MakeInsertStatement(mapping.Table, propValues, sqlParams);

                sqls.Add(sql);
                sqlParamLists.Add(sqlParams);
            }

            SqlConnectionUtilities.XSafeCommandBatch(_connDesc, sqls, sqlParamLists, true);

            DbCaches.StringLists.Return(sqls);
            foreach (List<object> objList in sqlParamLists)
            {
                DbCaches.ObjectLists.Return(objList);
            }
            DbCaches.EnumerableLists.Return(sqlParamLists);
        }

        /// <exclude/>
        public override int Update(ClassMapping mapping, DaoCriteria crit, IDictionary<string, object> propValues)
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

                return SqlConnectionUtilities.XSafeCommand(_connDesc, query.Sql.ToString(), query.Params);
            }
            finally
            {
                _sqlQueryCache.Return(query);
            }
        }

        /// <exclude/>
        public override void UpdateBatch(ClassMapping mapping, List<DaoCriteria> criteriaList, 
                                         List<IDictionary<string, object>> propValueDictionaries)
        {
            List<string> sqls = DbCaches.StringLists.Get();
            List<IEnumerable> sqlParamLists = DbCaches.EnumerableLists.Get();

            for (int i = 0; i < criteriaList.Count; i++)
            {
                SqlDaQuery query = _sqlQueryCache.Get();
                IDictionary<string, object> propValues = propValueDictionaries[i];

                PreProcessPropertyValues(mapping.Table, propValues);
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
                ExpressionsToQuery(query, criteriaList[i], mapping);

                sqls.Add(query.Sql.ToString());
                // Have to clone the list, since we're about to return the query
                // (and its list) to the cache.
                List<object> paramList = DbCaches.ObjectLists.Get();
                paramList.AddRange(query.Params);
                sqlParamLists.Add(paramList);
                _sqlQueryCache.Return(query);
            }

            SqlConnectionUtilities.XSafeCommandBatch(_connDesc, sqls, sqlParamLists, true);

            DbCaches.StringLists.Return(sqls);
            foreach (List<object> objList in sqlParamLists)
            {
                DbCaches.ObjectLists.Return(objList);
            }
            DbCaches.EnumerableLists.Return(sqlParamLists);
        }

        /// <exclude/>
        public override int Delete(ClassMapping mapping, DaoCriteria crit)
        {
            SqlDaQuery query = _sqlQueryCache.Get();

            query.Sql.Append("DELETE FROM ");
            query.Sql.Append(mapping.Table);
            ExpressionsToQuery(query, crit, mapping);

            int retVal = SqlConnectionUtilities.XSafeCommand(_connDesc, query.Sql.ToString(), query.Params);

            DisposeOfQuery(query);
            return retVal;
        }

        /// <exclude/>
        public override int GetCount(ClassMapping mapping, DaoCriteria crit)
        {
            SqlDaQuery query = _sqlQueryCache.Get();

            query.Sql.Append("SELECT COUNT(*) FROM ");
            query.Sql.Append(mapping.Table);
            ExpressionsToQuery(query, crit, mapping);

            int retVal = SqlConnectionUtilities.XSafeIntQuery(_connDesc, query.Sql.ToString(), query.Params);

            DisposeOfQuery(query);
            return retVal;
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
                        orderClauseToAddTo.Append(mapping.AllDataColsByObjAttrs[order.Property]);
                        orderClauseToAddTo.Append(" ASC");
                        break;
                    case SortType.Desc:
                        orderClauseToAddTo.Append(mapping.AllDataColsByObjAttrs[order.Property]);
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
        public override void ExecuteQuery(ClassMapping mapping, IDaQuery query,
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
            SqlConnectionUtilities.XSafeQuery(_connDesc, sqlQuery.Sql.ToString(), sqlQuery.Params, invokeMe, parameters);
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
        public override object GetLastAutoGeneratedId(ClassMapping mapping, string idCol)
        {
            string sql = _connDesc.MakeLastAutoGeneratedIdQuery(mapping.Table, idCol);
            Hashtable parameters = new Hashtable();
            SqlConnectionUtilities.XSafeQuery(_connDesc, sql, null, ReadScalarValue, parameters);
            return parameters[0];
        }

        /// <exclude/>
        public override int GetNextSequenceValue(string sequenceName)
        {
            return SqlConnectionUtilities.XSafeIntQuery(_connDesc,
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