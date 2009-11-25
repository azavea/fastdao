using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Avencia.Open.DAO.Util;

namespace Avencia.Open.DAO.SQL
{
    /// <summary>
    /// This class holds static utility methods having to do with constructing SQL statements
    /// (typically statements that would then be used with the SqlConnectionUtilities class).
    /// 
    /// This class is stateless and thus is threadsafe.
    /// </summary>
    public static class SQLUtilities
    {
        /// <summary>
        /// Converts the sql statement and parameters into a nicely formatted
        /// string for output in error messages or log statements.
        /// </summary>
        /// <param name="sql">The SQL statement that needs the list of params.</param>
        /// <param name="sqlParams">The list of params to format.</param>
        /// <returns>A string containing the sql and the list of parameters, nicely formatted.</returns>
        public static string SqlParamsToString(string sql, IEnumerable sqlParams)
        {
            StringBuilder sb = DbCaches.StringBuilders.Get();
            sb.Append("SQL: {");
            sb.Append(sql ?? "[null string]");
            sb.Append("} Params: {");
            if (sqlParams == null)
            {
                sb.Append("[null list]");
            }
            else
            {
                bool first = true;
                foreach (object val in sqlParams)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        sb.Append(", ");
                    }
                    if (val == null)
                    {
                        sb.Append("[real null]");
                    }
                    else if (val is DBNull)
                    {
                        sb.Append("[db null]");
                    }
                    else
                    {
                        sb.Append("\"");
                        sb.Append(val);
                        sb.Append("\"");
                    }
                }
                if (first)
                {
                    // it was empty
                    sb.Append("[empty list]");
                }
            }

            sb.Append("}");
            string retVal = sb.ToString();
            DbCaches.StringBuilders.Return(sb);
            return retVal;
        }

        /// <summary>
        /// Generates an insert statement to insert the values from the dictionary into
        /// the specified table.  Uses parameters for the values, the values will be taken
        /// from the dictionary and inserted into sqlParams in the same order the column names
        /// are inserted in the sql string.
        /// </summary>
        /// <param name="table">Name of the table to be inserted into.</param>
        /// <param name="columns">Dictionary of object column values keyed by string column names.</param>
        /// <param name="sqlParams">List to insert sql param values (in order) into.</param>
        /// <returns>A parameterized sql statement.</returns>
        public static string MakeInsertStatement(string table, IDictionary<string, object> columns,
                                                 IList<object> sqlParams) 
        {
            StringBuilder sb = DbCaches.StringBuilders.Get();
            sb.Append("INSERT INTO ");
            sb.Append(table);
            sb.Append("(");
            StringBuilder valStr = DbCaches.StringBuilders.Get();
            bool first = true;
            foreach (string key in columns.Keys) 
            {
                if (first)
                {
                    first = false;
                } else
                {
                    valStr.Append(",");
                    sb.Append(",");
                }
                object val = columns[key];
                valStr.Append("?");
                sb.Append(key);
                sqlParams.Add(val);
            }
            sb.Append(") VALUES (");
            sb.Append(valStr);
            DbCaches.StringBuilders.Return(valStr);
            sb.Append(")");

            string retVal = sb.ToString();
            DbCaches.StringBuilders.Return(sb);
            return retVal;
        }

        /// <summary>
        /// Generates a delete statement to delete rows where "key" = whereCols["key"] for everything
        /// in whereCols.
        /// </summary>
        /// <param name="table">Name of the table to be inserted into.</param>
        /// <param name="whereCols">Dictionary of object column values keyed by string column names.
        ///							  These columns are used in the "where" clause of the statement.  If
        ///							  this collection is null or empty, all rows will be updated.</param>
        /// <param name="sqlParams">List to insert sql param values (in order) into.</param>
        /// <returns>A parameterized sql statement.</returns>
        public static string MakeDeleteStatement(string table, IDictionary<string, object> whereCols,
                                                 IList<object> sqlParams) 
        {
            // Create the string list of where clauses (and put the matching values in sqlParams).
            StringBuilder sb = DbCaches.StringBuilders.Get();
            sb.Append("DELETE FROM ");
            sb.Append(table);
            sb.Append(MakeWhereClause(whereCols, sqlParams));
            string retVal = sb.ToString();
            DbCaches.StringBuilders.Return(sb);
            return retVal;
        }

        /// <summary>
        /// Generates an update statement to update rows where "key" = whereCols["key"] for everything
        /// in whereCols.
        /// </summary>
        /// <param name="table">Name of the table to be inserted into.</param>
        /// <param name="whereCols">Dictionary of object column values keyed by string column names.
        ///							  These columns are used in the "where" clause of the statement.  If
        ///							  this collection is null or empty, all rows will be updated.</param>
        /// <param name="columns">Dictionary of object column values keyed by string column names.  These
        ///						  columns are the values that will be set on the row(s).  This collection
        ///						  may not be null or empty.</param>
        /// <param name="sqlParams">List to insert sql param values (in order) into.</param>
        /// <returns>A parameterized sql statement.</returns>
        public static string MakeUpdateStatement(string table, IDictionary<string, object> whereCols,
                                                 IDictionary<string, object> columns, IList<object> sqlParams) 
        {
            // Create the string list of columns to update (and put the matching values in sqlParams).
            StringBuilder sb = DbCaches.StringBuilders.Get();
            sb.Append("UPDATE ");
            sb.Append(table);
            sb.Append(" SET ");
            bool first = true;
            foreach (string key in columns.Keys)
            {
                if (!first)
                {
                    sb.Append(", ");
                }
                else
                {
                    first = false;
                }
                sb.Append(key);
                sb.Append(" = ?");
                object val = columns[key];
                sqlParams.Add(val);
            }

            // Create the string list of where clauses (and put the matching values in sqlParams).
            sb.Append(MakeWhereClause(whereCols, sqlParams));

            string retVal = sb.ToString();
            DbCaches.StringBuilders.Return(sb);
            return retVal;
        }

        /// <summary>
        /// Helper method to generate a where clause, where keys = values.  You can simply
        /// concatenate this on the end of an SQL statement ("SELECT something " + MakeWhereClause(...)).
        /// </summary>
        /// <param name="whereCols">Dictionary of object column values keyed by string column names.</param>
        /// <param name="sqlParams">List to insert sql param values (in order) into.</param>
        /// <returns>Either be a zero-length string (if whereCols is empty) or
        ///			 a properly formatted " WHERE blah1 = ? AND blah2 = ? AND etc".</returns>
        public static string MakeWhereClause(IDictionary<string, object> whereCols, IList<object> sqlParams) 
        {
            StringBuilder whereClause = DbCaches.StringBuilders.Get();
            if ((whereCols != null) && (whereCols.Count > 0)) 
            {
                whereClause.Append(" WHERE ");
                bool first = true;
                foreach (string key in whereCols.Keys) 
                {
                    if (!first) 
                    {
                        whereClause.Append(" AND ");
                    } 
                    else 
                    {
                        first = false;
                    }
                    object val = whereCols[key];
                    whereClause.Append(key);
                    if (val != null) 
                    {
                        whereClause.Append(" = ?");
                        sqlParams.Add(val);
                    } 
                    else 
                    {
                        whereClause.Append(" IS NULL");
                    }
                }
            }

            string retVal = whereClause.ToString();
            DbCaches.StringBuilders.Return(whereClause);
            return retVal;
        }
    }
}