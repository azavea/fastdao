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
using System.Data.SqlClient;
using System.Text;
using Azavea.Open.Common;
using Azavea.Open.DAO.SQL;
using Azavea.Open.DAO.Util;
using SqlTransaction=Azavea.Open.DAO.SQL.SqlTransaction;

namespace Azavea.Open.DAO.SQLServer
{
    /// <summary>
    /// This class represents the info necessary to connect to a SQL Server database.
    /// TODO: Add support for SQL Server Spatial.
    /// </summary>
    public class SQLServerDescriptor : AbstractSqlConnectionDescriptor, ITransactionalConnectionDescriptor
    {
        /// <exclude/>
        protected readonly string _connectionStr;
        /// <exclude/>
        protected readonly string _cleanConnStr;

        /// <summary>
        /// The server name. May not be null.
        /// </summary>
        public readonly string Server;
        /// <summary>
        /// The database name.  May not be null.
        /// </summary>
        public readonly string Database;
        /// <summary>
        /// The user name, if necessary to log into the database.  May be null.
        /// </summary>
        public readonly string User;
        /// <summary>
        /// The password for the User.  May be null.
        /// </summary>
        public readonly string Password;

        /// <summary>
        /// This constructor reads all the appropriate values from a config file.
        /// </summary>
        /// <param name="config">Config to get params from.</param>
        /// <param name="component">Section of the config XML to look in for db params.</param>
        /// <param name="decryptionDelegate">Delegate to call to decrypt password fields.
        ///                                  May be null if passwords are in plain text.</param>
        public SQLServerDescriptor(Config config, string component,
            ConnectionInfoDecryptionDelegate decryptionDelegate)
            : this(config.GetParameter(component, "Server", null),
                   config.GetParameter(component, "Database", null),
                   config.GetParameter(component, "User", null),
                   GetDecryptedConfigParameter(config, component, "Password", decryptionDelegate)) {}

        /// <summary>
        /// Constructor that lets you pass everything as parameters rather than requiring a config.
        /// </summary>
        /// <param name="server">Server name.  May not be null.</param>
        /// <param name="database">Database name on that server.  May not be null.</param>
        /// <param name="user">Database user name, may be null.</param>
        /// <param name="password">Password for the user. May be null.</param>
        public SQLServerDescriptor(string server, string database, string user, string password)
        {
            _connectionStr = MakeConnectionString(server, database, user, password);
            _cleanConnStr = MakeConnectionString(server, database, user, null);
            Server = server;
            Database = database;
            User = user;
            Password = password;
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

        /// <summary>
        /// Assembles a connection string that can be used to get a database connection.
        /// All the parameters are optional for the purposes of this method, although obviously
        /// it would be possible to create a useless connection string if you leave out important
        /// parameters.
        /// </summary>
        /// <param name="server">Server name that is hosting the database</param>
        /// <param name="database">Database name on the server.</param>
        /// <param name="user">User name to use when accessing the db.</param>
        /// <param name="password">Password for above user.</param>
        /// <returns>A connection string that can be used to create SqlConnections.</returns>
        public static string MakeConnectionString(string server,
                                                  string database, string user, string password)
        {
            string connStr = "";

            // Assemble the string.  Only include parameters that have values.
            if (StringHelper.IsNonBlank(server))
            {
                connStr += "server=" + server + ";";
            }
            if (StringHelper.IsNonBlank(user))
            {
                connStr += "uid=" + user + ";";
            }
            if (StringHelper.IsNonBlank(password))
            {
                connStr += "pwd=" + password + ";";
            }
            if (StringHelper.IsNonBlank(database))
            {
                connStr += "database=" + database + ";";
            }
            return connStr;
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
            return new SqlConnection(_connectionStr);
        }

        /// <exclude/>
        public override void SetParametersOnCommand(IDbCommand cmd, IEnumerable parameters)
        {
            // TODO TODO TODO TODO
            // This is almost a complete cut-n-paste from the SQLiteDescriptor.SetParametersOnCommand.
            // The only difference is the class of the IDbParameter object instantiated.
            // We need to refactor this into some sort of helper method, or even better,
            // make it so we can construct the SQL with @params in the first place rather
            // than always using ?'s even if the DB does not understand them.
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
                    cmd.Parameters.Add(new SqlParameter(paramName, param));

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
            return new SqlDataAdapter((SqlCommand)cmd);
        }

        /// <exclude/>
        public override bool UsePooling()
        {
            return true;
        }

        /// <exclude/>
        public override SqlClauseWithValue MakeModulusClause(string fieldName)
        {
            StringBuilder sb = DbCaches.StringBuilders.Get();
            SqlClauseWithValue retVal = DbCaches.Clauses.Get();
            sb.Append("(");
            sb.Append(fieldName);
            sb.Append(" % ");
            retVal.PartBeforeValue = sb.ToString();
            retVal.PartAfterValue = ")";
            DbCaches.StringBuilders.Return(sb);
            return retVal;
        }

        /// <exclude/>
        public override SqlClauseWithValue MakeBitwiseAndClause(string columnName)
        {
            StringBuilder sb = DbCaches.StringBuilders.Get();
            SqlClauseWithValue retVal = DbCaches.Clauses.Get();
            sb.Append(" (");
            sb.Append(columnName);
            sb.Append(" & ");
            retVal.PartBeforeValue = sb.ToString();
            retVal.PartAfterValue = ")";
            DbCaches.StringBuilders.Return(sb);
            return retVal;
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
            return "[";
        }

        /// <exclude/>
        public override string ColumnAliasSuffix()
        {
            return "]";
        }
    }
}