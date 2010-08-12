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
using System.Data;
using System.Data.Common;
using System.Text;
using System.Data.SQLite;
using Azavea.Open.Common;
using Azavea.Open.DAO.SQL;
using Azavea.Open.DAO.Util;

namespace Azavea.Open.DAO.SQLite
{
    /// <summary>
    /// This class is a ConnectionDescriptor implementation for using FastDAO
    /// to communicate with a SQLite database.
    /// </summary>
    public class SQLiteDescriptor : AbstractSqlConnectionDescriptor, ITransactionalConnectionDescriptor
	{
        private readonly string _databasePath;
        private readonly string _connectionStr;
        private readonly string _cleanConnStr;
        private readonly bool _usePooling = true;

        /// <summary>
        /// Constructor for talking to a SQLite database.
        /// </summary>
        /// <param name="databasePath">Path to the db file.</param>
        public SQLiteDescriptor(string databasePath)
        {
            if (!StringHelper.IsNonBlank(databasePath))
            {
                throw new ArgumentNullException("databasePath", "Database file path cannot be null/blank.");
            }
            SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder();
            builder.DataSource = _databasePath = databasePath;
            
            // we want to disable pooling so we don't wind up locking
            // the file indefinitely.
            builder.Pooling = false;
            _usePooling = false;
            // There is no password, so the strings are the same.
            _cleanConnStr = builder.ToString();
            _connectionStr = builder.ToString();
        }
       
        /// <summary>
        /// This constructor reads all the appropriate values from our standard config file
        /// in the normal format.
        /// </summary>
        /// <param name="config">Config to get params from.</param>
        /// <param name="component">Section of the config XML to look in for db params.</param>
        /// <param name="decryptionDelegate">Delegate to call to decrypt password fields.
        ///                                  May be null if passwords are in plain text.</param>
        public SQLiteDescriptor(Config config, string component,
            ConnectionInfoDecryptionDelegate decryptionDelegate)
        {
            SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder();
            
            builder.Pooling = false;
            _usePooling = false;
            
            builder.DataSource = _databasePath = config.GetParameterWithSubstitution(component, "Database", true);

            // We don't currently support passwords, so the clean conn str is the same
            // as the real one.
            _cleanConnStr = builder.ToString();
            _connectionStr = builder.ToString();
        }

        /// <summary>
        /// The fully qualified path to the database file.
        /// </summary>
        public string DatabasePath
		{
			get{return _databasePath;}
		}

        /// <exclude/>
        public override string ToCleanString()
        {
            return _cleanConnStr;
        }

        /// <exclude/>
        public override string ToCompleteString()
        {
            return _connectionStr;
        }

        /// <exclude/>
        public override DbConnection CreateNewConnection()
        {
            return new SQLiteConnection(_connectionStr);
        }

        /// <exclude/>
        public override void SetParametersOnCommand(IDbCommand cmd, IEnumerable parameters)
        {
            IEnumerator enumer = parameters.GetEnumerator();

            if (enumer.MoveNext())
            {
                // There's at least one, so split apart the sql.
                string[] sqlPieces = cmd.CommandText.Split('?');
                StringBuilder sb = DbCaches.StringBuilders.Get();
                for (int x = 0; x < (sqlPieces.Length - 1); x++)
                {
                    // First append the sql fragment.
                    sb.Append(sqlPieces[x]);
                    // The name of the param has to be prepended with a @ in the sql, but not
                    // on the parameter object.
                    sb.Append("@");
                    string paramName = DbCaches.ParamNames.Get(x);
                    sb.Append(paramName);

                    // Get the value to insert.
                    object param = enumer.Current ?? DBNull.Value;
                    // Construct and add the parameter object.
                    cmd.Parameters.Add(new SQLiteParameter(paramName, param));

                    // Move the enumerator to the next item, if there should be another one.
                    if ((x + 1) < (sqlPieces.Length - 1))
                    {
                        if (!enumer.MoveNext())
                        {
                            throw new ArgumentException("Command sql has " +
                                                        (sqlPieces.Length - 1) + " params, but you only passed " +
                                                        (x + 1) +
                                                        " parameters: " +
                                                        SqlUtilities.SqlParamsToString(cmd.CommandText, parameters) +
                                                        "  You may get this if your sql string has a ? in it.  In that case you" +
                                                        " can try parameterizing whatever string constant has the ? and passing" +
                                                        " the constant from your code.  Not super elegant, but a whole lot easier than" +
                                                        " making this code understand quoted or escaped characters in the sql string.");
                        }
                    }
                }
                // Check that we don't have leftover parameters.
                if (enumer.MoveNext())
                {
                    throw new ArgumentException("Command sql has " +
                                                (sqlPieces.Length - 1) + " params, but you passed more than that: " +
                                                SqlUtilities.SqlParamsToString(cmd.CommandText, parameters));
                }
                // Append the last sql fragment.
                sb.Append(sqlPieces[sqlPieces.Length - 1]);
                cmd.CommandText = sb.ToString();
                DbCaches.StringBuilders.Return(sb);
            }
        }

        /// <summary>
        /// Begins the transaction.  Returns a NEW ConnectionDescriptor that you should
        /// use for operations you wish to be part of the transaction.
        /// 
        /// NOTE: You MUST call Commit or Rollback on the returned ITransaction when you are done.
        /// </summary>
        /// <returns>The ConnectionDescriptor object to pass to calls that you wish to have
        ///          happen as part of this transaction.</returns>
        public ITransaction BeginTransaction()
        {
            return new SqlTransaction(this);
        }

        /// <exclude/>
        public override DbDataAdapter CreateNewAdapter(IDbCommand cmd)
        {
            return new SQLiteDataAdapter((SQLiteCommand) cmd);
        }
        /// <exclude/>
        public override bool UsePooling()
        {
            return _usePooling;
        }

        /// <exclude/>
        public override SqlClauseWithValue MakeModulusClause(string columnName)
        {
            StringBuilder sb = DbCaches.StringBuilders.Get();
            SqlClauseWithValue retVal = DbCaches.Clauses.Get();
            sb.Append("(");
            sb.Append(columnName);
            sb.Append(" % ");
            retVal.PartBeforeValue = sb.ToString();
            retVal.PartAfterValue = ")";
            DbCaches.StringBuilders.Return(sb);
            return retVal;
        }

        /// <exclude/>
        public override string MakeSequenceValueQuery(string sequenceName)
        {
            return "SELECT seq from sqlite_sequence where name == \"" + sequenceName + "\"";
        }

        /// <exclude/>
        public override bool NeedToAliasColumns()
        {
            return true;
        }

        /// <exclude/>
        public override bool NeedAsForColumnAliases()
        {
            return false;
        }

        /// <exclude/>
        public override string ColumnAliasPrefix()
        {
            return "\"";
        }

        /// <exclude/>
        public override string ColumnAliasSuffix()
        {
            return "\"";
        }

        /// <exclude/>
        public override string TableAliasPrefix()
        {
            return "";
        }

        /// <exclude/>
        public override string TableAliasSuffix()
        {
            return "";
        }

        /// <exclude/>
        public override bool SupportsTruncate()
        {
            return false;
        }
    }
}
