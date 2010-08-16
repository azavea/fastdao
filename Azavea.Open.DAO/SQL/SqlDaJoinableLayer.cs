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
using System.Collections.Generic;
using System.Text;
using Azavea.Open.Common.Caching;
using Azavea.Open.DAO.Criteria;
using Azavea.Open.DAO.Criteria.Joins;
using Azavea.Open.DAO.Exceptions;
using Azavea.Open.DAO.Util;

namespace Azavea.Open.DAO.SQL
{
    /// <summary>
    /// A base class for layers that use SQL and support joins in SQL.
    /// </summary>
    public class SqlDaJoinableLayer : SqlDaLayer, IDaJoinableLayer
    {
        /// <summary>
        /// Since there are different types of query for different dao layers,
        /// each keeps their own cache.
        /// </summary>
        protected static readonly ClearingCache<SqlDaJoinQuery> _sqlJoinQueryCache =
            new ClearingCache<SqlDaJoinQuery>();
        /// <summary>
        /// Instantiates the data access layer with the connection descriptor for the DB.
        /// </summary>
        /// <param name="connDesc">The connection descriptor that is being used by this FastDaoLayer.</param>
        /// <param name="supportsNumRecords">If true, methods that return numbers of records affected will be
        ///                                 returning accurate numbers.  If false, they will probably return
        ///                                 FastDAO.UNKNOWN_NUM_ROWS.</param>
        public SqlDaJoinableLayer(AbstractSqlConnectionDescriptor connDesc, bool supportsNumRecords)
            : base(connDesc, supportsNumRecords)
        {
        }

        /// <summary>
        /// This returns whether or not this layer can perform the requested
        /// join natively.  This lets a layer that can have native joins determine
        /// whether this particular join is able to be done natively.
        /// </summary>
        /// <typeparam name="R">The type of object returned by the other DAO.</typeparam>
        /// <param name="crit">The criteria specifying the requested join.</param>
        /// <param name="rightConn">The connection info for the other DAO we're joining with.</param>
        /// <param name="rightMapping">Class mapping for the right table we would be querying against.</param>
        /// <returns>True if we can perform the join natively, false if we cannot.</returns>
        public bool CanJoin<R>(DaoJoinCriteria crit, IConnectionDescriptor rightConn, ClassMapping rightMapping) where R : new()
        {
            if (!_connDesc.Equals(rightConn))
            {
                return false;
            }
            return true;
        }

        private static string Shorten(string input)
        {
            StringBuilder sb = DbCaches.StringBuilders.Get();
            try
            {
                // Concatenate the first char, the last char, and every other char
                // in between.
                sb.Append(input[0]);
                // commented out the following to get by a problem where 
                // aliases were > 30 characters in Oracle, causing an error
                // TODO: create a better scheme for making alias names where field names are
                // shortened in a manner similar to DOS file names in Windows
                //int loopMax = input.Length - 1;
                //for (int x = 2; x < loopMax; x += 2)
                //{
                //    sb.Append(input[x]);
                //}
                //sb.Append(input[loopMax]);
                return sb.ToString();
            }
            finally
            {
                DbCaches.StringBuilders.Return(sb);
            }
        }

        /// <summary>
        /// This is not garaunteed to succeed unless CanJoin(crit, rightDao) returns true.
        /// </summary>
        /// <param name="crit">The criteria specifying the requested join.</param>
        /// <param name="leftMapping">Class mapping for the left table we're querying against.</param>
        /// <param name="rightMapping">Class mapping for the right table we're querying against.</param>
        public IDaJoinQuery CreateJoinQuery(DaoJoinCriteria crit, ClassMapping leftMapping,
                                                 ClassMapping rightMapping)
        {
            if (crit == null)
            {
                throw new ArgumentNullException("crit",
                                                "Unlike regular gets, you cannot perform a join with a null criteria.  At minimum you must create one and specify the join type.");
            }
            string leftAlias = "L_" + Shorten(leftMapping.Table);
            string leftPrefix = leftAlias + ".";
            string rightAlias = "R_" + Shorten(rightMapping.Table);
            string rightPrefix = rightAlias + ".";
            bool aliasingCols = _connDesc.NeedToAliasColumns();
            bool usingAs = _connDesc.NeedAsForColumnAliases();
            string colAliasPrefix = _connDesc.ColumnAliasPrefix();
            string colAliasSuffix = _connDesc.ColumnAliasSuffix();
            // Running out of descriptive var names... the above are like "[" and "]", this is
            // "table." or whatever.
            // Don't use punctuation to in the alias, not all databases (Access) can tolerate that.
            string leftColAliasPrefix = aliasingCols ? leftAlias : leftPrefix;
            string rightColAliasPrefix = aliasingCols ? rightAlias : rightPrefix;

            SqlDaJoinQuery retVal = _sqlJoinQueryCache.Get();
            retVal.SetPrefixes(leftColAliasPrefix, rightColAliasPrefix);

            retVal.Sql.Append("SELECT ");
            bool first = true;
            foreach (string leftCol in leftMapping.AllDataColsByObjAttrs.Values)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    retVal.Sql.Append(", ");
                }
                retVal.Sql.Append(leftPrefix).Append(leftCol);
                if (aliasingCols)
                {
                    retVal.Sql.Append(usingAs ? " AS " : " ")
                        .Append(colAliasPrefix)
                        .Append(leftColAliasPrefix).Append(leftCol)
                        .Append(colAliasSuffix);
                }
            }
            foreach (string rightCol in rightMapping.AllDataColsByObjAttrs.Values)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    retVal.Sql.Append(", ");
                }
                retVal.Sql.Append(rightPrefix).Append(rightCol);
                if (aliasingCols)
                {
                    retVal.Sql.Append(usingAs ? " AS " : " ")
                        .Append(colAliasPrefix)
                        .Append(rightColAliasPrefix).Append(rightCol)
                        .Append(colAliasSuffix);
                }
            }
            retVal.Sql.Append(" FROM ");
            
            TablesToQuery(retVal, crit, leftAlias, rightAlias, leftMapping, rightMapping);
            JoinExpressionsToQuery(retVal, crit, leftPrefix, rightPrefix, leftMapping, rightMapping);

            if (crit.Orders.Count > 0)
            {
                retVal.Sql.Append(" ORDER BY ");
                JoinOrderListToSql(retVal.Sql, crit, leftMapping, rightMapping, leftPrefix, rightPrefix);
            }

            // Don't return the objects to the caches, we'll do that in DisposeOfQuery.
            return retVal;
        }

        /// <summary>
        /// This performs a count instead of an actual query.  Depending on the data access layer
        /// implementation, this may or may not be significantly faster than actually executing
        /// the normal query and seeing how many results you get back.  Generally it should be
        /// faster.
        /// </summary>
        /// <param name="crit">The criteria specifying the requested join.</param>
        /// <param name="leftMapping">Class mapping for the left table we're querying against.</param>
        /// <param name="rightMapping">Class mapping for the right table we're querying against.</param>
        /// <returns>The number of results that you would get if you ran the actual query.</returns>
        public int GetCount(DaoJoinCriteria crit, ClassMapping leftMapping, ClassMapping rightMapping)
        {
            if (crit == null)
            {
                throw new ArgumentNullException("crit",
                                                "Unlike regular gets, you cannot perform a join with a null criteria.  At minimum you must create one and specify the join type.");
            }
            string leftAlias = "L_" + Shorten(leftMapping.Table);
            string leftPrefix = leftAlias + ".";
            string rightAlias = "R_" + Shorten(rightMapping.Table);
            string rightPrefix = rightAlias + ".";

            SqlDaJoinQuery query = _sqlJoinQueryCache.Get();

            query.Sql.Append("SELECT COUNT(*) FROM ");

            TablesToQuery(query, crit, leftAlias, rightAlias, leftMapping, rightMapping);
            JoinExpressionsToQuery(query, crit, leftPrefix, rightPrefix, leftMapping, rightMapping);

            int retVal = SqlConnectionUtilities.XSafeIntQuery(_connDesc, query.Sql.ToString(), query.Params);

            DisposeOfQuery(query);
            return retVal;
        }

        /// <summary>
        /// Append all the join expressions ("ON blah = blah WHERE a.blah = 5 AND b.blah = 10" etc)
        /// to the query.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="crit"></param>
        /// <param name="leftPrefix"></param>
        /// <param name="rightPrefix"></param>
        /// <param name="leftMapping"></param>
        /// <param name="rightMapping"></param>
        private void JoinExpressionsToQuery(SqlDaJoinQuery query, DaoJoinCriteria crit, string leftPrefix, string rightPrefix,
            ClassMapping leftMapping, ClassMapping rightMapping)
        {

            if (crit.Expressions.Count > 0)
            {
                query.Sql.Append(" ON ");
                JoinExpressionListToQuery(query, crit.BoolType, crit.Expressions,
                                          leftMapping, rightMapping, leftPrefix, rightPrefix);
            }
            bool addedWhere = false;
            if ((crit.LeftCriteria != null) && (crit.LeftCriteria.Expressions.Count > 0))
            {
                query.Sql.Append(" WHERE ");
                addedWhere = true;
                ExpressionListToQuery(query, crit.LeftCriteria.BoolType,
                                      crit.LeftCriteria.Expressions, leftMapping, leftPrefix);
            }
            if ((crit.RightCriteria != null) && (crit.RightCriteria.Expressions.Count > 0))
            {
                if (addedWhere)
                {
                    query.Sql.Append(" AND ");
                }
                else
                {
                    query.Sql.Append(" WHERE ");
                }
                ExpressionListToQuery(query, crit.RightCriteria.BoolType,
                                      crit.RightCriteria.Expressions, rightMapping, rightPrefix);
            }
        }

        /// <summary>
        /// Append the names of the tables ("BlahTable as left, FooTable as right") to the query.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="crit"></param>
        /// <param name="leftAlias"></param>
        /// <param name="rightAlias"></param>
        /// <param name="leftMapping"></param>
        /// <param name="rightMapping"></param>
        private void TablesToQuery(SqlDaJoinQuery query, DaoJoinCriteria crit, string leftAlias, string rightAlias,
            ClassMapping leftMapping, ClassMapping rightMapping)
        {
            query.Sql.Append(leftMapping.Table).Append(" ")
                .Append(_connDesc.TableAliasPrefix()).Append(leftAlias)
                .Append(_connDesc.TableAliasSuffix());
            switch (crit.TypeOfJoin)
            {
                case JoinType.Inner:
                    query.Sql.Append(" INNER JOIN ");
                    break;
                case JoinType.LeftOuter:
                    query.Sql.Append(" LEFT JOIN ");
                    break;
                case JoinType.RightOuter:
                    query.Sql.Append(" RIGHT JOIN ");
                    break;
                case JoinType.Outer:
                    query.Sql.Append(" ").Append(_connDesc.FullOuterJoinKeyword()).Append(" ");
                    break;
                default:
                    throw new NotSupportedException("Join type " + crit.TypeOfJoin +
                                                    " is not yet supported.");
            }
            query.Sql.Append(rightMapping.Table).Append(" ")
                .Append(_connDesc.TableAliasPrefix()).Append(rightAlias)
                .Append(_connDesc.TableAliasSuffix());
        }

        /// <summary>
        /// Override to handle join queries differently.
        /// </summary>
        /// <param name="query">Query you're done using.</param>
        public override void DisposeOfQuery(IDaQuery query)
        {
            if (query is SqlDaJoinQuery)
            {
                SqlDaJoinQuery sqlQuery = (SqlDaJoinQuery)query;
                _sqlJoinQueryCache.Return(sqlQuery);
            }
            else
            {
                base.DisposeOfQuery(query);
            }
        }

        /// <summary>
        /// Converts the list of SortOrders from this criteria into SQL, and appends to the
        /// given string builder.
        /// </summary>
        private static void JoinOrderListToSql(StringBuilder orderClauseToAddTo, DaoJoinCriteria crit,
                                               ClassMapping leftMapping, ClassMapping rightMapping, string leftPrefix, string rightPrefix)
        {
            bool first = true;
            foreach (JoinSortOrder order in crit.Orders)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    orderClauseToAddTo.Append(", ");
                }
                ClassMapping mappingToUse;
                string prefixToUse;
                if (order.IsForLeftDao)
                {
                    mappingToUse = leftMapping;
                    prefixToUse = leftPrefix;
                }
                else
                {
                    mappingToUse = rightMapping;
                    prefixToUse = rightPrefix;
                }
                switch (order.Direction)
                {
                    case SortType.Asc:
                        orderClauseToAddTo.Append(prefixToUse).
                            Append(mappingToUse.AllDataColsByObjAttrs[order.Property]).
                            Append(" ASC");
                        break;
                    case SortType.Desc:
                        orderClauseToAddTo.Append(prefixToUse).
                            Append(mappingToUse.AllDataColsByObjAttrs[order.Property]).
                            Append(" DESC");
                        break;
                    case SortType.Computed:
                        orderClauseToAddTo.Append(order.Property);
                        break;
                    default:
                        throw new NotSupportedException("Sort type '" + order.Direction +
                                                        "' not supported.");
                }
            }
        }

        /// <summary>
        /// Converts the list of expressions from this criteria into SQL, and appends to the 
        /// given string builder.
        /// </summary>
        /// <param name="queryToAddTo">Query we're adding the expression to.</param>
        /// <param name="boolType">Whether to AND or OR the expressions together.</param>
        /// <param name="expressions">The expressions to add to the query.</param>
        /// <param name="leftMapping">Class mapping for the class for the "left" table.</param>
        /// <param name="rightMapping">Class mapping for the class for the "right" table.</param>
        /// <param name="leftPrefix">What to prefix column names from the left table with,
        ///                         I.E. "LeftTable." for "LeftTable.Column".
        ///                         May be null if no prefix is desired.  May be something other than
        ///                         the table name if the tables are being aliased.</param>
        /// <param name="rightPrefix">What to prefix column names from the right table with,
        ///                         I.E. "RightTable." for "RightTable.Column".
        ///                         May be null if no prefix is desired.  May be something other than
        ///                         the table name if the tables are being aliased.</param>
        private void JoinExpressionListToQuery(SqlDaQuery queryToAddTo,
                                               BooleanOperator boolType,
                                               IEnumerable<IJoinExpression> expressions,
                                               ClassMapping leftMapping, ClassMapping rightMapping, string leftPrefix, string rightPrefix)
        {
            // starts out false for the first one.
            bool needsBooleanOperator = false;
            string boolText = BoolTypeToString(boolType);
            foreach (IJoinExpression expr in expressions)
            {
                try
                {
                    if (expr == null)
                    {
                        throw new NullReferenceException("Can't convert a null join expression to SQL.");
                    }
                    // After the first guy writes something, we need an operator.
                    if (JoinExpressionToQuery(queryToAddTo, expr, leftMapping, rightMapping,
                                              leftPrefix, rightPrefix, needsBooleanOperator ? boolText : ""))
                    {
                        needsBooleanOperator = true;
                    }
                }
                catch (Exception e)
                {
                    throw new UnableToConstructSqlException("Unable to add join expression to query: " + expr, _connDesc, e);
                }
            }
        }

        /// <summary>
        /// Converts a single JoinExpression to SQL (mapping the columns as appropriate) and appends
        /// to the given string builder.
        /// 
        /// Remember to wrap the SQL in parends if necessary.
        /// </summary>
        /// <param name="queryToAddTo">Query we're adding the expression to.</param>
        /// <param name="expr">The expression.  NOTE: It should NOT be null. This method does not check.</param>
        /// <param name="leftMapping">Class mapping for the class for the "left" table.</param>
        /// <param name="rightMapping">Class mapping for the class for the "right" table.</param>
        /// <param name="leftPrefix">What to prefix column names from the left table with,
        ///                         I.E. "LeftTable." for "LeftTable.Column".
        ///                         May be null if no prefix is desired.  May be something other than
        ///                         the table name if the tables are being aliased.</param>
        /// <param name="rightPrefix">What to prefix column names from the right table with,
        ///                         I.E. "RightTable." for "RightTable.Column".
        ///                         May be null if no prefix is desired.  May be something other than
        ///                         the table name if the tables are being aliased.</param>
        /// <param name="booleanOperator">The boolean operator (AND or OR) to insert before
        ///                               this expression.  Blank ("") if we don't need one.</param>
        /// <returns>Whether or not this expression modified the sql string.
        ///          Typically true, but may be false for special query types 
        ///          that use other parameters for certain types of expressions.</returns>
        protected virtual bool JoinExpressionToQuery(SqlDaQuery queryToAddTo, IJoinExpression expr,
                                                     ClassMapping leftMapping, ClassMapping rightMapping,
                                                     string leftPrefix, string rightPrefix, string booleanOperator)
        {
            // add the operator if one was specified.
            queryToAddTo.Sql.Append(booleanOperator);
            // Add some parends.
            queryToAddTo.Sql.Append("(");
            if (expr is EqualJoinExpression)
            {
                EqualJoinExpression equal = (EqualJoinExpression)expr;
                queryToAddTo.Sql.Append(leftPrefix);
                queryToAddTo.Sql.Append(leftMapping.AllDataColsByObjAttrs[equal.LeftProperty]);
                queryToAddTo.Sql.Append(expr.TrueOrNot() ? " = " : " <> ");
                queryToAddTo.Sql.Append(rightPrefix);
                queryToAddTo.Sql.Append(rightMapping.AllDataColsByObjAttrs[equal.RightProperty]);
            }
            else if (expr is GreaterJoinExpression)
            {
                GreaterJoinExpression greater = (GreaterJoinExpression)expr;
                queryToAddTo.Sql.Append(leftPrefix);
                queryToAddTo.Sql.Append(leftMapping.AllDataColsByObjAttrs[greater.LeftProperty]);
                queryToAddTo.Sql.Append(expr.TrueOrNot() ? " > " : " <= ");
                queryToAddTo.Sql.Append(rightPrefix);
                queryToAddTo.Sql.Append(rightMapping.AllDataColsByObjAttrs[greater.RightProperty]);
            }
            else if (expr is LesserJoinExpression)
            {
                LesserJoinExpression lesser = (LesserJoinExpression)expr;
                queryToAddTo.Sql.Append(leftPrefix);
                queryToAddTo.Sql.Append(leftMapping.AllDataColsByObjAttrs[lesser.LeftProperty]);
                queryToAddTo.Sql.Append(expr.TrueOrNot() ? " < " : " >= ");
                queryToAddTo.Sql.Append(rightPrefix);
                queryToAddTo.Sql.Append(rightMapping.AllDataColsByObjAttrs[lesser.RightProperty]);
            }
            else
            {
                throw new NotSupportedException("Expression type '" + expr.GetType() + "' is not supported.");
            }
            // Remember to close the parend.
            queryToAddTo.Sql.Append(")");
            return true;
        }
    }
}