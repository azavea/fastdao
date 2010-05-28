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
using Azavea.Open.Common;
using Azavea.Open.DAO.SQL;
using Azavea.Open.DAO.Util;
using FirebirdSql.Data.FirebirdClient;

namespace Azavea.Open.DAO.Firebird
{
    /// <summary>
    /// This class is a ConnectionDescriptor implementation for using FastDAO
    /// to communicate with a Firebird database (embedded or server).
    /// </summary>
    public class FirebirdDescriptor : AbstractSqlConnectionDescriptor, ITransactionalConnectionDescriptor
	{
        /// <summary>
        /// The server name, may be null if this is an embedded (I.E. file system) db.
        /// </summary>
        public readonly string Server;
        /// <summary>
        /// The database name, either the DB name or the DB file name.
        /// </summary>
        public readonly string Database;
        /// <summary>
        /// The user name, if necessary to log into the database.
        /// </summary>
        public readonly string User;
        /// <summary>
        /// The password for the User.  This is the plain text password.
        /// </summary>
        public readonly string Password;

        private readonly string _connectionStr;
        private readonly string _cleanConnStr;
        private readonly bool _usePooling = true;

        /// <summary>
        /// Constructor for talking to an embedded firebird database.
        /// </summary>
        /// <param name="databasePath">Path to the db file.</param>
        public FirebirdDescriptor(string databasePath)
        {
            if (!StringHelper.IsNonBlank(databasePath))
            {
                throw new ArgumentNullException("databasePath", "Database file path cannot be null/blank.");
            }
            FbConnectionStringBuilder builder = new FbConnectionStringBuilder();
            builder.Database = Database = databasePath;
            builder.ServerType = FbServerType.Embedded;
            // For the embedded server, we want to disable pooling so we don't wind up locking
            // the file indefinitely.
            builder.Pooling = false;
            _usePooling = false;
            // There is no password, so the strings are the same.
            _cleanConnStr = builder.ToString();
            _connectionStr = builder.ToString();
        }
        /// <summary>
        /// Constructor for talking to a server-based firebird database.
        /// </summary>
        /// <param name="server">Server name or IP.  May not be null.</param>
        /// <param name="database">Database name on that server.  May not be null.</param>
        /// <param name="user">Database user name, may be null.</param>
        /// <param name="password">Plain text password for the user.
        ///                        May be null.</param>
        public FirebirdDescriptor(string server, string database,
            string user, string password)
        {
            if (!StringHelper.IsNonBlank(server))
            {
                throw new ArgumentNullException("server", "Server parameter cannot be null/blank.");
            }
            if (!StringHelper.IsNonBlank(database))
            {
                throw new ArgumentNullException("database", "Database name parameter cannot be null/blank.");
            }
            FbConnectionStringBuilder builder = new FbConnectionStringBuilder();
            builder.DataSource = Server = server;
            builder.Database = Database = database;
            builder.UserID = User = user;

            // First make it without a password.
            _cleanConnStr = builder.ToString();
            // Now with the password for the real one.
            Password = password;
            builder.Password = Password;
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
        public FirebirdDescriptor(Config config, string component,
            ConnectionInfoDecryptionDelegate decryptionDelegate)
        {
            FbConnectionStringBuilder builder = new FbConnectionStringBuilder();
            Server = config.GetParameter(component, "Server", null);
            if (StringHelper.IsNonBlank(Server))
            {
                builder.DataSource = Server;
            }
            else
            {
                builder.ServerType = FbServerType.Embedded;
                // For the embedded server, we want to disable pooling so we don't wind up locking
                // the file indefinitely.
                builder.Pooling = false;
                _usePooling = false;
            }
            builder.Database = Database = config.GetParameterWithSubstitution(component, "Database", true);
            builder.UserID = User = config.GetParameter(component, "User", null);

            // First make it without a password.
            _cleanConnStr = builder.ToString();
            // Now with the password for the real one.
            Password = GetDecryptedConfigParameter(config, component, "Password", decryptionDelegate);
            builder.Password = Password;
            _connectionStr = builder.ToString();
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
        public override DbConnection CreateNewConnection()
        {
            return new FbConnection(_connectionStr);
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
                    cmd.Parameters.Add(new FbParameter(paramName, param));

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

        /// <exclude/>
        public override DbDataAdapter CreateNewAdapter(IDbCommand cmd)
        {
            return new FbDataAdapter((FbCommand) cmd);
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
            sb.Append("MOD(");
            sb.Append(columnName);
            sb.Append(", ");
            retVal.PartBeforeValue = sb.ToString();
            retVal.PartAfterValue = ")";
            DbCaches.StringBuilders.Return(sb);
            return retVal;
        }

        /// <exclude/>
        public override string MakeSequenceValueQuery(string sequenceName)
        {
            return "SELECT GEN_ID(" + sequenceName + ", 1) FROM RDB$DATABASE";
        }

        /// <exclude/>
        public override bool NeedToAliasColumns()
        {
            return true;
        }

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
